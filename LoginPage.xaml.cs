using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using RentreyApp.Services;
using RentreyApp.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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

        // --- Updated Logic for OnCreateAccountClicked in LoginPage.xaml.cs ---
        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            var newUser = new User
            {
                FirstName = FirstNameEntry.Text,
                LastName = LastNameEntry.Text,
                Address = AddressEntry.Text,
                State = this.State,
                TaxFileNumber = TaxFileNumberEntry.Text,
                Points = 790
            };

            var databaseService = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<DatabaseService>();

            if (databaseService == null)
            {
                await DisplayAlert("Error", "Database service could not be initialized.", "OK");
                return;
            }

            await databaseService.SaveUserAsync(newUser);

            // ✅ Use MAUI Preferences instead of Application.Properties
            Preferences.Set("LoggedInUserId", newUser.Id);
            Preferences.Set("UserName", newUser.FirstName);
            Preferences.Set("UserPoints", newUser.Points);

            // ⭐ FIX: Change route to refer to the primary Tab name "Home"
            await Shell.Current.GoToAsync("//HomePage");
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
