﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ping.OidcClient.Tokens;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using IdentityModel.OidcClient.Results;
using static IdentityModel.OidcClient.OidcClientOptions;

namespace Ping.OidcClient
{
    /// <summary>
    /// Base class for performing authentication and authorization operations with Ping using the
    /// underlying <see cref="IdentityModel.OidcClient.OidcClient"/>.
    /// </summary>
    public abstract class PingClientBase : IPingClient
    {
        private readonly IdTokenRequirements _idTokenRequirements;
        private readonly PingClientOptions _options;
        private readonly string _userAgent;
        private IdentityModel.OidcClient.OidcClient _oidcClient;
        private IdentityModel.OidcClient.OidcClient OidcClient
        {
            get
            {
                return _oidcClient ?? (_oidcClient = new IdentityModel.OidcClient.OidcClient(CreateOidcClientOptions(_options)));
            }
        }

        /// <summary>
        /// Create a new instance of <see cref="PingClientBase"/>.
        /// </summary>
        /// <param name="options"><see cref="PingClientOptions"/> specifying the configuration options for this client.</param>
        /// <param name="platformName">Platform name that forms part of the user-agent when communicating with Ping servers.</param>
        protected PingClientBase(PingClientOptions options, string platformName)
        {
            _options = options;
            _idTokenRequirements = new IdTokenRequirements($"https://{_options.Authority}/", _options.ClientId, options.Leeway, options.MaxAge);
            _userAgent = CreateAgentString(platformName);
        }

        /// <inheritdoc />
        public async Task<LoginResult> LoginAsync(object extraParameters = null, CancellationToken cancellationToken = default)
        {
            var finalExtraParameters = AppendTelemetry(extraParameters);
            if (_options.MaxAge.HasValue)
                finalExtraParameters["max_age"] = _options.MaxAge.Value.TotalSeconds.ToString("0");

            var loginRequest = new LoginRequest { FrontChannelExtraParameters = finalExtraParameters };

            Debug.WriteLine($"Using Callback URL '{OidcClient.Options.RedirectUri}'. Ensure this is an Allowed Callback URL for application/client ID {_options.ClientId}.");

            var result = await OidcClient.LoginAsync(loginRequest, cancellationToken);

            if (!result.IsError)
                await IdTokenValidator.AssertTokenMeetsRequirements(_idTokenRequirements, result.IdentityToken); // Nonce is created & tested by OidcClient

            return result;
        }

        /// <inheritdoc/>
        public async Task<BrowserResultType> LogoutAsync(object extraParameters = null, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"Using Callback URL '{OidcClient.Options.PostLogoutRedirectUri}'. Ensure this is an Allowed Logout URL for application/client ID {_options.ClientId}.");

            var logoutParameters = AppendTelemetry(extraParameters);
            logoutParameters["client_id"] = OidcClient.Options.ClientId;
            logoutParameters["returnTo"] = OidcClient.Options.PostLogoutRedirectUri;

            var endSessionUrl = new RequestUrl($"https://{_options.Authority}/idp/startSLO.ping").Create(logoutParameters);

            var logoutRequest = new LogoutRequest();
            var browserOptions = new BrowserOptions(endSessionUrl, OidcClient.Options.PostLogoutRedirectUri ?? string.Empty)
            {
                Timeout = TimeSpan.FromSeconds(logoutRequest.BrowserTimeout),
                DisplayMode = logoutRequest.BrowserDisplayMode
            };

            var browserResult = await OidcClient.Options.Browser.InvokeAsync(browserOptions, cancellationToken);

            return browserResult.ResultType;
        }

        /// <inheritdoc/>
        public Task<AuthorizeState> PrepareLoginAsync(object extraParameters = null, CancellationToken cancellationToken = default)
        {
            return OidcClient.PrepareLoginAsync(AppendTelemetry(extraParameters), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<LoginResult> ProcessResponseAsync(string data, AuthorizeState state, object extraParameters = null, CancellationToken cancellationToken = default)
        {
            return OidcClient.ProcessResponseAsync(data, state, AppendTelemetry(extraParameters), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, object extraParameters = null, CancellationToken cancellationToken = default)
        {
            var result = await OidcClient.RefreshTokenAsync(refreshToken, AppendTelemetry(extraParameters), cancellationToken);

            if (!result.IsError)
                await IdTokenValidator.AssertTokenMeetsRequirements(_idTokenRequirements, result.IdentityToken); // Nonce is created & tested by OidcClient

            return result;
        }

        /// <inheritdoc/>
        public Task<UserInfoResult> GetUserInfoAsync(string accessToken)
        {
            return OidcClient.GetUserInfoAsync(accessToken);
        }

        private OidcClientOptions CreateOidcClientOptions(PingClientOptions options)
        {
            var scopes = options.Scope.Split(' ').ToList();
            if (!scopes.Contains("openid"))
                scopes.Insert(0, "openid");

            var oidcClientOptions = new OidcClientOptions
            {
                Authority = $"https://{options.Authority}",
                ClientId = options.ClientId,
                Scope = String.Join(" ", scopes),
                LoadProfile = options.LoadProfile,
                Browser = options.Browser,
                Flow = AuthenticationFlow.AuthorizationCode,
                ResponseMode = AuthorizeResponseMode.Redirect,
                RedirectUri = options.RedirectUri,
                PostLogoutRedirectUri = options.PostLogoutRedirectUri,
                ClockSkew = options.Leeway,

                Policy = {
                    RequireAuthorizationCodeHash = false,
                    RequireAccessTokenHash = false
                }
            };

#pragma warning disable CS0618 // ClientSecret will be removed in a future update.
            if (!String.IsNullOrWhiteSpace(oidcClientOptions.ClientSecret))
                oidcClientOptions.ClientSecret = options.ClientSecret;
#pragma warning restore CS0618

            if (options.RefreshTokenMessageHandler != null)
                oidcClientOptions.RefreshTokenInnerHttpHandler = options.RefreshTokenMessageHandler;

            if (options.BackchannelHandler != null)
                oidcClientOptions.BackchannelHandler = options.BackchannelHandler;

            return oidcClientOptions;
        }

        private Dictionary<string, string> AppendTelemetry(object values)
        {
            var dictionary = ObjectToDictionary(values);

            return dictionary;
        }

        private string CreateAgentString(string platformName)
        {
            var sdkVersion = typeof(PingClientBase).GetTypeInfo().Assembly.GetName().Version;
            var agentJson = $"{{\"name\":\"oidc-net\",\"version\":\"{sdkVersion.Major}.{sdkVersion.Minor}.{sdkVersion.Revision}\",\"env\":{{\"platform\":\"{platformName}\"}}}}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(agentJson));
        }

        private Dictionary<string, string> ObjectToDictionary(object values)
        {
            if (values is Dictionary<string, string> dictionary)
                return dictionary;

            dictionary = new Dictionary<string, string>();
            if (values != null)
                foreach (var prop in values.GetType().GetRuntimeProperties())
                {
                    var value = prop.GetValue(values) as string;
                    if (!string.IsNullOrEmpty(value))
                        dictionary.Add(prop.Name, value);
                }

            return dictionary;
        }
    }
}