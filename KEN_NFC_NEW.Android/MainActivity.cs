using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using Android.Nfc;
using Plugin.NFC;
using Android.Content;
using Acr.UserDialogs;
using System.Collections.Generic;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.IO;
using Java.IO;

namespace KEN_NFC_NEW.Droid {

    [Activity(Label = "KEN_NFC", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    [IntentFilter(new[] {NfcAdapter.ActionTagDiscovered}, Categories = new[] {Intent.CategoryDefault}, DataMimeType = MainPage.MIME_TYPE)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            CrossNFC.Init(this);
            Rg.Plugins.Popup.Popup.Init(this);

            Instance = this;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            Acr.UserDialogs.Extended.UserDialogs.Init(this);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume() {
            base.OnResume();
            CrossNFC.OnResume();
        }

        protected override void OnNewIntent(Intent intent) {
            base.OnNewIntent(intent);
            CrossNFC.OnNewIntent(intent);
        }

        public override void OnBackPressed()
        {
            Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }
    }
}