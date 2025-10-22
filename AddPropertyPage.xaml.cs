using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Threading.Tasks;
using RentreyApp.Services;
using System;
using System.Diagnostics;
using System.IO;
using Rentrey;
using System.Collections.Generic;
using System.Linq; // Added for Linq usage if needed later

namespace Rentrey.Maui
{
    // ⭐ ADDED: IQueryAttributable to receive return data
    public partial class AddPropertyPage : ContentPage, INotifyPropertyChanged, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;

        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(nameof(ImageSource)); }
        }

        private double _latitude;
        public double Latitude
        {
            get => _latitude;
            set { _latitude = value; OnPropertyChanged(nameof(Latitude)); }
        }

        private double _longitude;
        public double Longitude
        {
            get => _longitude;
            set { _longitude = value; OnPropertyChanged(nameof(Longitude)); }
        }

        public AddPropertyPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            BindingContext = this;
            // MessagingCenter code removed, as IQueryAttributable is now the standard method.
        }


        // ⭐ Implement ApplyQueryAttributes to handle data returned from MapSelectionPage
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // The query returns strings or objects that might need casting
            if (query.TryGetValue("Latitude", out object latObj) && latObj is double lat)
            {
                Latitude = lat;
            }
            else if (query.TryGetValue("Latitude", out object latStr) && latStr is string latS && double.TryParse(latS, out double latD))
            {
                Latitude = latD;
            }

            if (query.TryGetValue("Longitude", out object longObj) && longObj is double lon)
            {
                Longitude = lon;
            }
            else if (query.TryGetValue("Longitude", out object longStr) && longStr is string lonS && double.TryParse(lonS, out double lonD))
            {
                Longitude = lonD;
            }

            // Consume the query to avoid re-applying it when navigating back
            query.Clear();
        }

        // Handle image selection from phone
        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();

                if (photo != null)
                {
                    // Copy the file to local app storage for permanent access
                    string imagesDir = Path.Combine(FileSystem.AppDataDirectory, "user_images");
                    Directory.CreateDirectory(imagesDir);

                    var newFilePath = Path.Combine(imagesDir, photo.FileName);
                    using (var stream = await photo.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFilePath))
                    {
                        await stream.CopyToAsync(newStream);
                    }

                    // Set the ImageSource property to the local file path
                    ImageSource = newFilePath;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Image picking failed: {ex.Message}");
                await DisplayAlert("Error", "Unable to select image. Please check permissions.", "OK");
            }
        }

        // ⭐ UPDATED: Navigate to the new MapSelectionPage
        private async void OnSelectLocationClicked(object sender, EventArgs e)
        {
            // Navigate to the MapSelectionPage to get coordinates
            await Shell.Current.GoToAsync("///MapSelectionPage");
        }

        // ⭐ NEW: Cancel handler - Navigates back to the MapPage
        private async void OnCancelClicked(object sender, EventArgs e)
        {
            // Navigate back one page (to MapPage) without passing parameters
            await Shell.Current.GoToAsync("//SearchTab");
        }


        private async void OnSubmitPropertyClicked(object sender, EventArgs e)
        {
            // 1. Validation and Parsing

            // Attempt to parse/validate the Entries via x:Name
            if (string.IsNullOrWhiteSpace(AddressEntry.Text) ||
                !int.TryParse(PriceEntry.Text, out int price) ||
                !int.TryParse(MinRatingEntry.Text, out int minRating) ||
                !int.TryParse(PrefRatingEntry.Text, out int prefRating) ||
                !int.TryParse(BedsEntry.Text, out int beds) ||
                !int.TryParse(BathsEntry.Text, out int baths) ||
                !int.TryParse(ParkingEntry.Text, out int parking) ||
                // ⭐ VALIDATION: Check if Latitude and Longitude bound properties were set (non-zero)
                Latitude == 0 || Longitude == 0)
            {
                await DisplayAlert("Input Error", "Please ensure all fields are filled correctly and a location is selected on the map (Lat/Long are non-zero).", "OK");
                return;
            }

            // 2. Create the new Property object
            var newProperty = new Property
            {
                ListingId = Guid.NewGuid().ToString().Substring(0, 8),
                ImageSource = ImageSource ?? "default_house.png",
                Details = $"{beds} 🛏️ {baths} 🛁 {parking} 🚗",
                Address = AddressEntry.Text,
                Price = price,
                Latitude = Latitude, // Use bound property
                Longitude = Longitude, // Use bound property
                MinimumRating = minRating,
                PreferredRating = prefRating,
                ScrapedAt = DateTime.Now,
                IsUserAdded = true
            };

            // 3. Save to Database
            try
            {
                await _databaseService.SavePropertyAsync(newProperty);
                await DisplayAlert("Success!", $"Property '{newProperty.Address}' added to the database.", "OK");

                // 4. ⭐ FIXED: Navigate to the root Home Tab
                await Shell.Current.GoToAsync("//HomeTab");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database Save Error: {ex.Message}");
                await DisplayAlert("Error", "Failed to save property. Please check input formats.", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Removing obsolete MessagingCenter code
        }

    }
}