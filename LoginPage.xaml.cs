using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using RentreyApp.Services;
using RentreyApp.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace Rentrey.Maui
{
    public partial class LoginPage : ContentPage, INotifyPropertyChanged
    {
        // Removed old properties: StatesList and State

        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = this;
        }

        // Helper method to create a simple hash for password simulation
        // NOTE: In a real application, you must use a strong library like BCrypt.
        private string CreateSimpleHash(string input)
        {
            // Simple hash combining input with a hardcoded "salt" for simulation
            return (input + "rentreysalt").GetHashCode().ToString();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // 1. Validate Input
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Login Error", "Please enter both email and password.", "OK");
                return;
            }

            var databaseService = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<DatabaseService>();

            if (databaseService == null)
            {
                await DisplayAlert("Error", "Database service not found.", "OK");
                return;
            }

            // 2. Fetch user by Email (Assuming GetUserByEmailAsync exists/will be implemented)
            var user = await databaseService.GetUserByEmailAsync(EmailEntry.Text);

            if (user == null)
            {
                await DisplayAlert("Login Failed", "Invalid email or password.", "OK");
                return;
            }

            // 3. Authenticate (Simulated Hash Check)
            string inputPasswordHash = CreateSimpleHash(PasswordEntry.Text);

            if (user.PasswordHash != inputPasswordHash)
            {
                await DisplayAlert("Login Failed", "Invalid email or password.", "OK");
                return;
            }

            // 4. Successful Login: Set Session Data in Preferences
            Preferences.Set("LoggedInUserId", user.Id);
            Preferences.Set("UserName", user.FirstName);
            Preferences.Set("UserPoints", user.Points);

            // 5. Navigate to the main app (Home Tab)
            await Shell.Current.GoToAsync("//HomeTab");
        }

        private async void OnNavigateToCreateAccountClicked(object sender, EventArgs e)
        {
            // ⭐ FIX: Changed navigation to use the absolute route prefix (///) as suggested by the error message.
            await Shell.Current.GoToAsync("///CreateAccountPage");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}