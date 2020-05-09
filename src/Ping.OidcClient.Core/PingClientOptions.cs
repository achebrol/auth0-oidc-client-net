using IdentityModel.OidcClient.Browser;
using System;
using System.Net.Http;

namespace Ping.OidcClient
{
    /// <summary>
    /// Specifies options that can be passed to <see cref="PingClientBase"/> implementations.
    /// </summary>
    public class PingClientOptions
    {
        /// <summary>
        /// The <see cref="IBrowser"/> implementation responsible for displaying the Ping Login screen. Leave this
        /// unassigned to accept the recommended implementation for platform.
        /// </summary>
        public IBrowser Browser { get; set; }

        /// <summary>
        /// Your Ping Client ID.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Your Ping Client Secret.
        /// </summary>
        [Obsolete("Client Secrets should not be used in non-confidential clients such as native desktop and mobile apps. " +
            "This property will be removed in a future release.")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Ping Federate Domain.
        /// </summary>
        /// <remarks>
        /// e.g. idptest.aa.com
        /// </remarks>
        public string Authority { get; set; } = "idp.aa.com";

        /// <summary>
        /// Siteminder Domain.
        /// </summary>
        /// <remarks>
        /// e.g. smlogin.aa.com
        /// </remarks>
        public string SiteminderAuthority { get; set; } = "smlogin.aa.com";


        /// <summary>
        /// Indicates whether the user profile should be loaded from the /userinfo endpoint.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        public bool LoadProfile { get; set; } = true;

        /// <summary>
        /// The scopes you want to request.
        /// </summary>
        public string Scope { get; set; } = "openid profile email";

        /// <summary>
        /// Allow overriding the RetryMessageHandler.
        /// </summary>
        /// <example>
        /// var handler = new HttpClientHandler();
        /// var options = new PingClientOptions
        /// {
        ///    RefreshTokenMessageHandler = handler
        /// };
        /// </example>
        public HttpMessageHandler RefreshTokenMessageHandler { get; set; }

        /// <summary>
        /// Allow overriding the BackchannelHandler.
        /// </summary>
        /// <example>
        /// <code>
        /// var handler = new HttpClientHandler();
        /// var options = new PingClientOptions
        /// {
        ///    BackchannelHandler = handler
        /// };
        /// </code>
        /// </example>
        public HttpMessageHandler BackchannelHandler { get; set; }

        /// <summary>
        /// Override the Redirect URI used to return from logout.
        /// </summary>
        /// <remarks>
        /// Defaults to a platform-specific value you can observe in the debug console window when performing a logout.
        /// On iOS this is made from the app bundle ID and on Android from a lower-cased version of the package name.
        /// Whether you use the default or manually set this value it must be added to the 
        /// Allowed Logout URLs for this application/client to allow the logout process to complete.
        /// </remarks>
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Override the the Redirect URI used to return from login.
        /// </summary>
        /// <remarks>
        /// Defaults to a platform-specific value you can observe in the debug console window when performing a login.
        /// On iOS this is made from the app bundle ID and on Android from a lower-cased version of the package name.
        /// Whether you use the default or manually set this value it must be added to the 
        /// Allowed Callback URLs for this application/client to allow the login process to complete.
        /// </remarks>
        public string RedirectUri { get; set; }

        /// <summary>
        /// The amount of leeway to accommodate potential clock skew when validating an ID token's claims.
        /// Defaults to 5 minutes.
        /// </summary>
        public TimeSpan Leeway { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Optional limit on the how long since the user was last authenticated.
        /// </summary>
        public TimeSpan? MaxAge { get; set; }

        /// <summary>
        /// Create a new instance of the <see cref="PingClientOptions"/> class used to configure options for
        /// <see cref="PingClientBase"/> implementations by way of their constructors.
        /// </summary>
        public PingClientOptions()
        {
        }
    }
}