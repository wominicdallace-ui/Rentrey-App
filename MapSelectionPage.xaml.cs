using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps; // Contains Location, Distance
using System;
using System.Threading.Tasks; // Added for async operations
using System.Collections.Generic; // Added for Dictionary usage
using System.Diagnostics; // Added for Debug output

namespace Rentrey.Maui
{
    public partial class MapSelectionPage : ContentPage
    {
        private Location _selectedLocation;

        // Fixed Default Location: Sydney, guaranteed to be non-null
        private readonly Location SydneyLocation = new Location(-33.8688, 151.2093);

        // Reference to the Pin object (if defined in UpdatePinAndUI or constructor)
        private Pin _propertyPin;


        public MapSelectionPage()
        {
            InitializeComponent();

            // Initialization logic for Pin and Map centering
            _propertyPin = new Pin { Label = "Selected Location", Type = PinType.Place, Location = SydneyLocation };
            // Assuming your XAML map has x:Name="LocationMap"
            LocationMap.Pins.Add(_propertyPin);

            // Center the map immediately on the safe default location
            LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                SydneyLocation, Distance.FromKilometers(50)));

            // Initial state: Button must be disabled until location is selected
            ConfirmButton.IsEnabled = false;

            // Try to move to user location asynchronously after initial setup
            _ = TryGetCurrentLocationAsync();
        }

        private async Task TryGetCurrentLocationAsync()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

                if (location != null)
                {
                    LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                        location, Distance.FromKilometers(2)));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting location for map center: {ex.Message}");
            }
        }


        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            _selectedLocation = e.Location;

            // Clear existing pins
            LocationMap.Pins.Clear();

            // Create a new pin at the selected location
            var pin = new Pin
            {
                Label = "Selected Location",
                Location = _selectedLocation,
                Type = PinType.Place
            };
            LocationMap.Pins.Add(pin);

            // Update UI elements (assuming CoordinatesLabel and ConfirmButton exist in XAML)
            CoordinatesLabel.Text = $"Lat: {_selectedLocation.Latitude:F4}, Long: {_selectedLocation.Longitude:F4}";
            ConfirmButton.IsEnabled = true;

            // Center the map on the new location
            LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(_selectedLocation, Distance.FromKilometers(1)));
        }

        // ⭐ FIXED: Changed navigation to use the absolute route to the target page.
        private async void OnConfirmLocationClicked(object sender, EventArgs e)
        {
            if (_selectedLocation == null)
            {
                await DisplayAlert("Error", "No location selected. Please tap the map.", "OK");
                return;
            }

            // Prepare parameters for returning to the previous page
            var parameters = new Dictionary<string, object>
            {
                // Passing coordinates back as strings for robust IQueryAttributable handling
                { "Latitude", _selectedLocation.Latitude.ToString() },
                { "Longitude", _selectedLocation.Longitude.ToString() }
            };

            await Shell.Current.GoToAsync("///AddPropertyPageRoute", parameters);
        }
    }
}