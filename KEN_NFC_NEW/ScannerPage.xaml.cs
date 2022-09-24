using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KEN_NFC_NEW
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScannerPage : ContentPage
    {
        public ScannerPage()
        {
            InitializeComponent();
        }

        private void BackButtonClicked(object sender, EventArgs e)
        {
            if (!Transporter.replaceMode)
                App.Current.MainPage = new NavigationPage(new MainPage());
        }

        protected override bool OnBackButtonPressed()
        {
            if (!Transporter.replaceMode)
                App.Current.MainPage = new NavigationPage(new MainPage());

            return true;
        }

        async private void ScanResult(ZXing.Result result)
        {
            Device.BeginInvokeOnMainThread(() => 
            {
                Transporter.code = result.Text;
                App.Current.MainPage = new NavigationPage(new MainPage());
            });
            
        }
    }
}