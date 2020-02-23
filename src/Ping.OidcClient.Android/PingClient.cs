using Android.App;
using Android.Content;
using System;
using System.Linq;

namespace Ping.OidcClient
{
    public class PingClient : PingClientBase
    {
        public override void InitializeAsync(PingClientOptions options)
        {
            options.Browser = options.Browser ?? new AutoSelectBrowser();
            base.InitializeAsync(options);
        }
    }
}
