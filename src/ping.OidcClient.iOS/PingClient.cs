using System.Threading.Tasks;
using static Foundation.NSBundle;

namespace Ping.OidcClient
{
    /// <summary>
    /// Primary class for performing authentication and authorization operations with Ping using the
    /// underlying <see cref="IdentityModel.OidcClient.OidcClient"/>.
    /// </summary>
    public class PingClient : PingClientBase
    {


        public override void InitializeAsync(PingClientOptions options)
        {
            options.Browser = options.Browser ?? new AutoSelectBrowser();
            base.InitializeAsync(options);
        }
    }
}
