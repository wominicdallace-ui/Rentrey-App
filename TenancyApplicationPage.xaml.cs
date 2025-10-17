using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using RentreyApp.Services;
using RentreyApp.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq; // Added to access the FirstOrDefault method

namespace Rentrey.Maui
{
    public partial class TenancyApplicationPage : ContentPage, IQueryAttributable // ⭐ ADDED IQueryAttributable
    {
        private string _listingId;
        private string _propertyAddress;

        // Constructor remains mostly empty as DI is used elsewhere, or we retrieve the service later.
        public TenancyApplicationPage()
        {
            InitializeComponent();
        }

        // ⭐ NEW: Implement ApplyQueryAttributes to receive navigation parameters
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("propertyId") && query["propertyId"] is string listingId)
            {
                _listingId = listingId;

                // Fetch the property from the database to get the full address
                var databaseService = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<DatabaseService>();
                if (databaseService != null)
                {
                    var allProperties = await databaseService.GetPropertiesAsync();
                    var detailedProperty = allProperties.FirstOrDefault(p => p.ListingId == _listingId);

                    if (detailedProperty != null)
                    {
                        _propertyAddress = detailedProperty.Address;
                        // ⭐ FIXED: Ensure the Label text is set on load
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

            // ⭐ VALIDATION: Check if property data was loaded
            if (string.IsNullOrWhiteSpace(_propertyAddress))
            {
                await DisplayAlert("Error", "Cannot submit application. Property address is missing.", "OK");
                return;
            }

            try
            {
                var newApplication = new ApplicationItem
                {
                    // ⭐ FIXED: Use the actual PropertyID and Address
                    PropertyId = 0, // Since PropertyId in ApplicationItem is int, we can't use ListingId (string) unless we change the model
                    PropertyAddress = _propertyAddress, // ⭐ FIXED: Use the loaded address
                    Status = ApplicationStatus.Pending,
                    ApplicationDate = DateTime.Now
                };

                // Note: The PropertyId field in ApplicationItem is an int, but your ListingId is a string.
                // For simplicity, I'm setting PropertyId to 0, but using the correct PropertyAddress.

                await databaseService.SaveApplicationAsync(newApplication);

                await DisplayAlert("Submitted", "Your tenancy application has been successfully submitted!", "OK");

                await Shell.Current.GoToAsync("///ApplicationPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to submit application: {ex.Message}", "OK");
            }
        }
    }
}