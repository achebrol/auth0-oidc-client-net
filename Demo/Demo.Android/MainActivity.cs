using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using Ping.OidcClient;
using Xamarin.Forms;

namespace Demo.Droid
{
    [Activity(Label = "Demo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView },
            Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
            DataScheme = "com.aa.lm.my-tasks",
        DataPath = "oauth2/code/cb")] //xamarinformsclients

    // DataHost = "callback")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            DependencyService.Register<PingClient>();
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();
            ActivityMediator.Instance.Cancel();
        }

        /// <summary>
        /// Method executed when the Activity receives a new intent that may continue a pending
        /// <see cref="IBrowser"/> implementation by way of the <see cref="ActivityMediator"/>.
        /// </summary>
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            ActivityMediator.Instance.Send(intent.DataString);
        }


    }
}