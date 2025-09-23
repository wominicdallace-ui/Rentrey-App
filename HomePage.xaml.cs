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
        private readonly ProptrackService _proptrackService;
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
        
        private ObservableCollection<Property> _newlyAddedProperties;
        public ObservableCollection<Property> NewlyAddedProperties
        {
            get => _newlyAddedProperties;
            set
            {
                if (_newlyAddedProperties != value)
                {
                    _newlyAddedProperties = value;
                    OnPropertyChanged(nameof(NewlyAddedProperties));
                }
            }
        }

        public string UserName { get; set; }
        public string Points { get; set; }
        public double ProgressRatio { get; set; }
        public string LastUpdated { get; set; }

        public ObservableCollection<RecentUpdate> RecentUpdates { get; set; }

        public ICommand NavigateToPropertyCommand { get; }

        public HomePage(DatabaseService databaseService, ProptrackService proptrackService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _proptrackService = proptrackService;
            
            // Call the method to load data
            LoadData();

            // Set a default profile image
            ProfileImageSource = "profile_icon.png";

            // Initialize data properties
            UserName = "Lachlan";
            Points = "790 / 1000 Points";
            ProgressRatio = 0.79; // 790 out of 1000
            LastUpdated = "Last Updated: 02/08/25";

            // Initialize the Recent Updates collection with placeholder data
            RecentUpdates = new ObservableCollection<RecentUpdate>
            {
                new RecentUpdate { IconSource = "payment_icon.png", Title = "On-Time Payment", Description = "You earned 10 points for on-time rent payment!" },
                new RecentUpdate { IconSource = "house_icon.png", Title = "New Update from Landlord", Description = "Your Landlord has updated your lease agreement." }
            };

            // Initialize the command for navigation
            NavigateToPropertyCommand = new Command<Property>(OnNavigateToProperty);

            // Set the BindingContext
            BindingContext = this;
        }

        private async void LoadData()
        {
            // First, try to load data from the local database
            var localProperties = await _databaseService.GetPropertiesAsync();
            if (localProperties.Any())
            {
                NewlyAddedProperties = new ObservableCollection<Property>(localProperties);
            }
            else
            {
                // If the database is empty, get data from the API
                var apiProperties = await _proptrackService.GetListingsAsync();
                if (apiProperties.Any())
                {
                    // Save the new data to the local database for future use
                    foreach (var prop in apiProperties)
                    {
                        await _databaseService.SavePropertyAsync(prop);
                    }
                    NewlyAddedProperties = new ObservableCollection<Property>(apiProperties);
                }
                else
                {
                    // Handle no data case
                }
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
                // Fallback for devices without camera support
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
                { "property", property }
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
