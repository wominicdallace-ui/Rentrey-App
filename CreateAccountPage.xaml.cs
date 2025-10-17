using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using RentreyApp.Models;
using RentreyApp.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rentrey.Maui
{
    public partial class CreateAccountPage : ContentPage, INotifyPropertyChanged
    {
        public ObservableCollection<string> StatesList { get; set; }
        public string State { get; set; }

        public CreateAccountPage()
        {
            InitializeComponent();

            // Initialize the list of Australian States/Territories
            StatesList = new ObservableCollection<string>
            {
                "New South Wales", "Victoria", "Queensland", "South Australia",
                "Western Australia", "Tasmania", "Australian Capital Territory", "Northern Territory"
            };

            this.BindingContext = this;
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Registration Error", "Please fill out all required fields (Name, Email, Password).", "OK");
                return;
            }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Registration Error", "Passwords do not match.", "OK");
                return;
            }

            // 2. Access Database Service
            var databaseService = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<DatabaseService>();

            if (databaseService == null)
            {
                await DisplayAlert("Error", "Database service not found.", "OK");
                return;
            }

            // 3. Check for existing user with this email
            var existingUser = await databaseService.GetUserByEmailAsync(EmailEntry.Text);
            if (existingUser != null)
            {
                await DisplayAlert("Registration Failed", "An account with this email already exists.", "OK");
                return;
            }

            // 4. Create User Record
            var newUser = new User
            {
                FirstName = FirstNameEntry.Text,
                LastName = LastNameEntry.Text,
                Email = EmailEntry.Text,
                PhoneNumber = PhoneNumberEntry.Text,
                PasswordHash = PasswordHelper.CreateSimpleHash(PasswordEntry.Text), // Hash the password
                Address = AddressEntry.Text,
                State = this.State,
                TaxFileNumber = TaxFileNumberEntry.Text,
                Points = 790 // Initial starting points
            };

            // 5. Save and Log In
            try
            {
                await databaseService.SaveUserAsync(newUser);

                // Set session data immediately (auto-login)
                Preferences.Set("LoggedInUserId", newUser.Id);
                Preferences.Set("UserName", newUser.FirstName);
                Preferences.Set("UserPoints", newUser.Points);

                // Navigate to the main app (Home Tab), clearing the navigation stack
                await Shell.Current.GoToAsync("//HomeTab");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during registration: {ex.Message}");
                await DisplayAlert("Error", "Registration failed due to a database error. Please try again.", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}