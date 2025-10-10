using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using RentreyApp.Services;
using RentreyApp.Models;

namespace Rentrey.Maui
{
    public partial class TenancyApplicationPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public TenancyApplicationPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "rentrey.db3"));
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
            try
            {
                // ✅ Save the application to the database
                var newApplication = new ApplicationItem
                {
                    PropertyId = 101,
                    PropertyAddress = "123 Example Street",
                    Status = ApplicationStatus.Pending,
                    ApplicationDate = DateTime.Now
                };

                await _databaseService.SaveApplicationAsync(newApplication);

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
