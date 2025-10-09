using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Rentrey.Maui
{
    public partial class LoginPage : ContentPage, INotifyPropertyChanged
    {
        public ObservableCollection<string> StatesList { get; set; }
        public string State { get; set; }

        public LoginPage()
        {
            InitializeComponent();

            StatesList = new ObservableCollection<string>
            {
                "New South Wales",
                "Victoria",
                "Queensland",
                "South Australia",
                "Western Australia",
                "Tasmania",
                "Australian Capital Territory",
                "Northern Territory"
            };

            this.BindingContext = this;
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            // You can now access the selected state using this.State
            string userName = FirstNameEntry.Text;

            // This is the correct way to navigate from the login page to the main app page.
            await Shell.Current.GoToAsync("//HomePage");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}