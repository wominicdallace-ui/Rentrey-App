using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.IO;
using System.Threading.Tasks;
using Rentrey;
using RentreyApp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Maui.Graphics;

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

        // ... (existing bound properties and collections) ...
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

        public string UserName { get; set; }
        public string Points { get; set; }
        public double ProgressRatio { get; set; }
        public string LastUpdated { get; set; }

        private Color _rankColor;
        public Color RankColor
        {
            get => _rankColor;
            set { if (_rankColor != value) { _rankColor = value; OnPropertyChanged(nameof(RankColor)); } }
        }

        // ⭐ NEW: Property to control visibility of user-specific sections
        private bool _isUserLoggedIn;
        public bool IsUserLoggedIn
        {
            get => _isUserLoggedIn;
            set { if (_isUserLoggedIn != value) { _isUserLoggedIn = value; OnPropertyChanged(nameof(IsUserLoggedIn)); } }
        }


        public ObservableCollection<RecentUpdate> RecentUpdates { get; set; }

        public ICommand NavigateToPropertyCommand { get; }

        public HomePage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;

            RecommendedProperties = new ObservableCollection<Property>();

            // LoadUserData() is now called from OnAppearing to refresh the header visibility
            LoadData();

            ProfileImageSource = "profile_icon.png";
            LastUpdated = "Last Updated: 02/08/25";

            RecentUpdates = new ObservableCollection<RecentUpdate>
            {
                new RecentUpdate { IconSource = "payment_icon.png", Title = "On-Time Payment", Description = "You earned 10 points for on-time rent payment!" },
                new RecentUpdate { IconSource = "profile_nav.png", Title = "New Update from Landlord", Description = "Your Landlord has updated your lease agreement." }
            };

            NavigateToPropertyCommand = new Command<Property>(OnNavigateToProperty);

            BindingContext = this;
        }

        // ⭐ NEW: Call LoadUserData from OnAppearing to ensure the header toggles when navigating back from login
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserData();
        }

        // ⭐ REFACTORED: Loads user data, calculates rank, color, progress, AND handles UI visibility
        private void LoadUserData()
        {
            int userPoints = Preferences.Get("UserPoints", 0);
            string userName = Preferences.Get("UserName", "Guest");

            // --- Toggle Visibility Based on Login Status ---
            bool isLoggedIn = userName != "Guest" && Preferences.Get("LoggedInUserId", 0) > 0;
            IsUserLoggedIn = isLoggedIn; // ⭐ Set the new visibility property

            // UI TOGGLE LOGIC for Header
            ProfileFrame.IsVisible = isLoggedIn;
            ProfileInfoStack.IsVisible = isLoggedIn;
            CreateAccountButton.IsVisible = !isLoggedIn;

            // --- Rank Logic ---
            const int rankThreshold = 1000;
            int pointsBase;

            if (userPoints >= 3000) // Platinum (3000+)
            {
                pointsBase = 3000;
                RankColor = Color.FromHex("#7E2FDE"); // Purple
                ProgressRatio = 1.0;
            }
            else if (userPoints >= 2000) // Gold (2000-2999)
            {
                pointsBase = 2000;
                RankColor = Color.FromHex("#FFD700"); // Gold
                ProgressRatio = (double)(userPoints - pointsBase) / rankThreshold;
            }
            else if (userPoints >= 1000) // Silver (1000-1999)
            {
                pointsBase = 1000;
                RankColor = Color.FromHex("#C0C0C0"); // Silver
                ProgressRatio = (double)(userPoints - pointsBase) / rankThreshold;
            }
            else // Bronze (0-999)
            {
                pointsBase = 0;
                RankColor = Color.FromHex("#CD7F32"); // Bronze
                ProgressRatio = (double)userPoints / rankThreshold;
            }

            // --- Update Bound Properties ---
            UserName = userName;
            // Points display format (e.g., 790 / 1000 Points)
            Points = $"{userPoints} / {pointsBase + rankThreshold} Points";

            // Notify UI bindings
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(Points));
            OnPropertyChanged(nameof(ProgressRatio));
        }

        // ⭐ NEW: Method to handle the Create Account Button click
        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("LoginPage");
        }


        private async void LoadData()
        {
            // ... (LoadData implementation remains the same) ...
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
                    Debug.WriteLine($"Fetched {properties?.Count() ?? 0} properties from database.");
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