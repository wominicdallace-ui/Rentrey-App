using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.IO;
using System.Threading.Tasks;
using Rentrey;
using RentreyApp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Rentrey.Maui
{
    // Data model for the recent updates section
    public class RecentUpdate
    {
        public string IconSource { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        private string _profileImageSource;
        public string ProfileImageSource
        {
            get => _profileImageSource;
            set
            {
                if (_profileImageSource != value)
                {
                    _profileImageSource = value;
                    OnPropertyChanged(nameof(ProfileImageSource));
                }
            }
        }

        private ObservableCollection<Property> _recommendedProperties;
        public ObservableCollection<Property> RecommendedProperties
        {
            get => _recommendedProperties;
            set
            {
                if (_recommendedProperties != value)
                {
                    _recommendedProperties = value;
                    OnPropertyChanged(nameof(RecommendedProperties));
                }
            }
        }
        // NOTE: The old 'NewlyAddedProperties' property is now removed to eliminate conflicts.

        public string UserName { get; set; }
        public string Points { get; set; }
        public double ProgressRatio { get; set; }
        public string LastUpdated { get; set; }

        public ObservableCollection<RecentUpdate> RecentUpdates { get; set; }

        public ICommand NavigateToPropertyCommand { get; }

        public HomePage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;

            RecommendedProperties = new ObservableCollection<Property>();

            LoadData();

            ProfileImageSource = "profile_icon.png";
            UserName = "Lachlan";
            Points = "790 / 1000 Points";
            ProgressRatio = 0.79;
            LastUpdated = "Last Updated: 02/08/25";

            RecentUpdates = new ObservableCollection<RecentUpdate>
            {
                new RecentUpdate { IconSource = "payment_icon.png", Title = "On-Time Payment", Description = "You earned 10 points for on-time rent payment!" },
                new RecentUpdate { IconSource = "profile_nav.png", Title = "New Update from Landlord", Description = "Your Landlord has updated your lease agreement." }
            };

            NavigateToPropertyCommand = new Command<Property>(OnNavigateToProperty);

            BindingContext = this;
        }

        private async void LoadData()
        {
            try
            {
                var properties = await _databaseService.GetPropertiesAsync();
                if (properties.Any())
                {
                    // Filter for properties with a price and sort by price to get the cheapest 5 properties
                    var cheapestProperties = properties.Where(p => p.Price > 0).OrderBy(p => p.Price).Take(5).ToList();

                    RecommendedProperties.Clear();
                    foreach (var prop in cheapestProperties)
                    {
                        RecommendedProperties.Add(prop);
                    }
                }
                else
                {
                    Debug.WriteLine("No properties found in database.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while loading data: {ex.Message}");
            }
        }

        private async Task<PermissionStatus> GetPermissionAsync<T>() where T : Permissions.BasePermission, new()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<T>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<T>();
            }

            return status;
        }

        private async void LoadImage(object sender, EventArgs e)
        {
            FileResult photo = null;

            if (MediaPicker.Default.IsCaptureSupported)
            {
                string action = await DisplayActionSheet("Select Image Source", "Cancel", null, "Take Photo", "Choose from Gallery");

                if (action == "Take Photo")
                {
                    PermissionStatus status = await GetPermissionAsync<Permissions.Camera>();
                    if (status == PermissionStatus.Granted)
                    {
                        photo = await MediaPicker.CapturePhotoAsync();
                    }
                    else
                    {
                        await DisplayAlert("Permission Denied", "Camera permission is required to take photos.", "OK");
                        return;
                    }
                }
                else if (action == "Choose from Gallery")
                {
                    PermissionStatus status = await GetPermissionAsync<Permissions.Photos>();
                    if (status == PermissionStatus.Granted)
                    {
                        photo = await MediaPicker.PickPhotoAsync();
                    }
                    else
                    {
                        await DisplayAlert("Permission Denied", "Photo library permission is required to pick photos.", "OK");
                        return;
                    }
                }
            }
            else
            {
                photo = await MediaPicker.PickPhotoAsync();
            }

            if (photo != null)
            {
                string imagesDir = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "images");
                System.IO.Directory.CreateDirectory(imagesDir);

                var newFile = System.IO.Path.Combine(imagesDir, photo.FileName);
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                {
                    await stream.CopyToAsync(newStream);
                }
                ProfileImageSource = newFile;
            }
        }

        private async void OnNavigateToProperty(Property property)
        {
            if (property == null)
                return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "propertyId", property.ListingId }
            };

            await Shell.Current.GoToAsync("PropertyPage", navigationParameter);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}