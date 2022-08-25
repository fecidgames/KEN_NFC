using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KEN_NFC_NEW
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScannerPage : ContentPage
    {
        Task ShowAlert(string message, string title = null) => DisplayAlert(string.IsNullOrWhiteSpace(title) ? "QR-resultaat" : title, message, "OK");

        public ScannerPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked_Back(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new MainPage());
        }

        protected override bool OnBackButtonPressed()
        {
            App.Current.MainPage = new NavigationPage(new MainPage());
            return true;
        }

        async private Task ShowMSG(string title, 
            string msg,
            string btntext,
            Action callback)
        {
            await DisplayAlert(title, msg, btntext);
            callback?.Invoke();
        }

        async private void ZXingScannerView_OnScanResult(ZXing.Result result)
        {
            Device.BeginInvokeOnMainThread(() => 
            {
                ShowMSG("Resultaat", result.Text + ",\n" + result.BarcodeFormat.ToString(), "OK", async () => { ScannerView.IsScanning = true; } );
                ScannerView.IsScanning = false;
            });
            
        }
    }
}