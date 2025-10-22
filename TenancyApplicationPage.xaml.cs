using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel; // Required for MainThread/MessagingCenter
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using RentreyApp.Models;
using RentreyApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// ⭐ FIX: Add missing namespace for Debug.WriteLine
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rentrey.Maui
{
    public partial class TenancyApplicationPage : ContentPage, IQueryAttributable
    {
        private string _listingId;
        private string _propertyAddress;

        // ⭐ Constants for point reward and message
        private const int ApplicationPointReward = 50;
        private const string ApplicationRewardDescription = "You successfully submitted an Application!";

        public TenancyApplicationPage()
        {
            InitializeComponent();
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("propertyId") && query["propertyId"] is string listingId)
            {
                _listingId = listingId;

                var databaseService = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<DatabaseService>();
                if (databaseService != null)
                {
                    var allProperties = await databaseService.GetPropertiesAsync();
                    var detailedProperty = allProperties.FirstOrDefault(p => p.ListingId == _listingId);

                    if (detailedProperty != null)
                    {
                        _propertyAddress = detailedProperty.Address;
                        PropertyLabel.Text = _propertyAddress;
                    }
                }
            }
        }


        private async void OnDownloadPdfClicked(object sender, EventArgs e)
        {
            try
            {
                string filePath = Path.Combine(FileSystem.CacheDirectory, "TenancyApplicationForm.pdf");
                await File.WriteAllTextAsync(filePath, "Tenancy Application Form\n\nPlease fill out your details below...");

                await DisplayAlert("Download Complete", $"PDF saved to:\n{filePath}", "OK");
            }
            catch (Exception ex)
            {
                // ⭐ FIX: Debug.WriteLine now works
                Debug.WriteLine($"Error downloading PDF: {ex.Message}");
                await DisplayAlert("Error", $"Failed to download PDF: {ex.Message}", "OK");
            }
        }

        private async void OnSubmitPdfClicked(object sender, EventArgs e)
        {
            var databaseService = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<DatabaseService>();

            if (databaseService == null)
            {
                await DisplayAlert("Error", "Database service could not be initialized.", "OK");
                return;
            }

            // Validation: Check if property data was loaded
            if (string.IsNullOrWhiteSpace(_propertyAddress))
            {
                await DisplayAlert("Error", "Cannot submit application. Property address is missing.", "OK");
                return;
            }

            try
            {
                // --- 1. Save Application ---
                var newApplication = new ApplicationItem
                {
                    PropertyId = 0,
                    PropertyAddress = _propertyAddress,
                    Status = ApplicationStatus.Pending,
                    ApplicationDate = DateTime.Now
                };
                await databaseService.SaveApplicationAsync(newApplication);

                // --- 2. Update User Points and Notify ---
                int userId = Preferences.Get("LoggedInUserId", 0);
                if (userId > 0)
                {
                    var user = await databaseService.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        user.Points += ApplicationPointReward;
                        await databaseService.SaveUserAsync(user);

                        // Update session and send message to AccountPage/HomePage
                        Preferences.Set("UserPoints", user.Points);

                        // ⭐ NEW: Send message to AccountPage to update Recent Points list
                        MessagingCenter.Send(this, "ApplicationSubmitted",
                            new PointEntry { Points = ApplicationPointReward, Description = ApplicationRewardDescription });
                    }
                }


                await DisplayAlert("Submitted", $"Your tenancy application has been successfully submitted! (+{ApplicationPointReward} points)", "OK");

                // Navigate to the root route of the Application Tab
                await Shell.Current.GoToAsync("///ApplicationTab");
            }
            catch (Exception ex)
            {
                // ⭐ FIX: Debug.WriteLine now works
                Debug.WriteLine($"Error submitting application: {ex.Message}");
                await DisplayAlert("Error", $"Failed to submit application: {ex.Message}", "OK");
            }
        }
    }
}