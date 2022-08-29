using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rg.Plugins.Popup.Extensions;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KEN_NFC_NEW
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReplacePopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public ReplacePopup()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Transporter.oldCode = Value_Entry.Text;
            Navigation.PopPopupAsync();
            ScannerPage nextPage = new ScannerPage();
            nextPage.Title = "Scan de nieuwe barcode";

            App.Current.MainPage = new NavigationPage(nextPage);
        }
    }
}