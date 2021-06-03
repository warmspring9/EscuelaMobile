using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QrPrototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotFoundDialog : PopupPage
    {
        public NotFoundDialog()
        {
            InitializeComponent();
        }

        private void exit(object sender, EventArgs e)
        {
            App.Current.MainPage.Navigation.PopPopupAsync(true);
        }
    }
}