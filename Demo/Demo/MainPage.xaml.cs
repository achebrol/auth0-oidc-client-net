using Ping.OidcClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Demo
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly IPingClient _pingClient;
        public MainPage()
        {
            InitializeComponent();
            _pingClient = DependencyService.Get<IPingClient>();
            _pingClient = new PingClient(new PingClientOptions
            {
                Authority = "idptest.aa.com",
                ClientId = "LM-MYTASKS-APP",
                Scope = "openid profile offline_access",
                RedirectUri = "com.aa.lm.my-tasks://oauth2/code/cb",
            });

            LoginButton.Clicked += LoginButton_Clicked;
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            var loginResult = await _pingClient.LoginAsync();

            var sb = new StringBuilder();

            if (loginResult.IsError)
            {
                ResultLabel.Text = "An error occurred during login...";

                sb.AppendLine("An error occurred during login:");
                sb.AppendLine(loginResult.Error);
            }
            else
            {
                ResultLabel.Text = $"Welcome {loginResult.User.Identity.Name}";

                sb.AppendLine($"ID Token: {loginResult.IdentityToken}");
                sb.AppendLine($"Access Token: {loginResult.AccessToken}");
                sb.AppendLine($"Refresh Token: {loginResult.RefreshToken}");
                sb.AppendLine();
                sb.AppendLine("-- Claims --");
                foreach (var claim in loginResult.User.Claims)
                {
                    sb.AppendLine($"{claim.Type} = {claim.Value}");
                }
            }

            System.Diagnostics.Debug.WriteLine(sb.ToString());

        }
    }
}
