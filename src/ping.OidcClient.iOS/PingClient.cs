using static Foundation.NSBundle;

namespace Ping.OidcClient
{
    /// <summary>
    /// Primary class for performing authentication and authorization operations with Ping using the
    /// underlying <see cref="IdentityModel.OidcClient.OidcClient"/>.
    /// </summary>
    public class PingClient : PingClientBase
    {
        /// <summary>
        /// Creates a new instance of the Ping OIDC Client.
        /// </summary>
        /// <param name="options">The <see cref="PingClientOptions"/> specifying the configuration for the Ping OIDC Client.</param>
        public PingClient(PingClientOptions options)
            : base(options, "xamarin-ios")
        {
            options.Browser = options.Browser ?? new AutoSelectBrowser();
            options.RedirectUri = options.RedirectUri;
            options.PostLogoutRedirectUri = options.PostLogoutRedirectUri;
        }
    }
}
