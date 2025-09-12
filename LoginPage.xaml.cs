using Microsoft.Maui.Controls;

namespace Rentrey.Maui
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            string userName = FirstNameEntry.Text;

            // Navigate to the HomePage tab
            await Shell.Current.GoToAsync("//HomePage");
        }
    }
}
