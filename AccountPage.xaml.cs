using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using System;
using RentreyApp.Services;
using RentreyApp.Models;
using System.Linq;

namespace Rentrey.Maui
{
    public class RatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double ratio)
            {
                // This is used for the LinearGradientBrush StartPoint/EndPoint in XAML
                return new Point(ratio, 0);
            }
            return new Point(0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PointEntry
    {
        public int Points { get; set; }
        public string Description { get; set; }
    }

    public class Badge
    {
        public string IconSource { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public partial class AccountPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        // ⭐ FIXED: ProfileImageSource definition restored
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

        // ⭐ FIXED: UserLocation definition restored
        private string _userLocation;
        public string UserLocation
        {
            get => _userLocation;
            set
            {
                if (_userLocation != value)
                {
                    _userLocation = value;
                    OnPropertyChanged(nameof(UserLocation));
                }
            }
        }

        private int _loggedInUserId;

        private string _userName;
        public string UserName
        {
            get => _userName;
            set { if (_userName != value) { _userName = value; OnPropertyChanged(nameof(UserName)); } }
        }

        private string _points;
        public string Points
        {
            get => _points;
            set { if (_points != value) { _points = value; OnPropertyChanged(nameof(Points)); } }
        }

        private Color _rankColor;
        public Color RankColor
        {
            get => _rankColor;
            set { if (_rankColor != value) { _rankColor = value; OnPropertyChanged(nameof(RankColor)); } }
        }

        private string _nextRankText;
        public string NextRankText
        {
            get => _nextRankText;
            set { if (_nextRankText != value) { _nextRankText = value; OnPropertyChanged(nameof(NextRankText)); } }
        }


        private double _progressRatio;
        public double ProgressRatio
        {
            get => _progressRatio;
            set { if (_progressRatio != value) { _progressRatio = value; OnPropertyChanged(nameof(ProgressRatio)); } }
        }

        public string LastUpdated { get; set; }
        private string _rankText;
        public string RankText
        {
            get => _rankText;
            set { if (_rankText != value) { _rankText = value; OnPropertyChanged(nameof(RankText)); } }
        }
        public string PointsAwayText { get; set; }

        public ObservableCollection<PointEntry> RecentPoints { get; set; }
        public ObservableCollection<Badge> EarnedBadges { get; set; }

        public AccountPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;

            // Initialize collections and static data
            RecentPoints = new ObservableCollection<PointEntry>
            {
                new PointEntry { Points = 10, Description = "You earned 10 points for on-time rent payment!" },
                new PointEntry { Points = 50, Description = "You earned 50 points for early rent payment!" },
                new PointEntry { Points = 730, Description = "You earned 730 points for your credit score!" }
            };

            EarnedBadges = new ObservableCollection<Badge>
            {
                new Badge { IconSource = "badge_verified.png", Title = "Verified Tenant", Description = "Successfully verified your identity" },
                new Badge { IconSource = "badge_issue.png", Title = "Issue Reporter", Description = "Reported a maintenance issue" },
                new Badge { IconSource = "badge_reviewer.png", Title = "Property Reviewer", Description = "Reviewed a past property" },
                new Badge { IconSource = "badge_new.png", Title = "New User", Description = "Joined the Rentrey community" }
            };

            // Subscribe to Application Submitted message to update points list
            MessagingCenter.Subscribe<TenancyApplicationPage, PointEntry>(this, "ApplicationSubmitted", (sender, entry) =>
            {
                // Run on the main thread to update ObservableCollection/UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Add the new reward entry
                    RecentPoints.Insert(0, entry);

                    // Limit the list to 4 items
                    const int maxRecentItems = 4;
                    if (RecentPoints.Count > maxRecentItems)
                    {
                        RecentPoints.RemoveAt(RecentPoints.Count - 1);
                    }

                    // Reload User Data to update score/rank display
                    LoadUserData();
                });
            });


            ProfileImageSource = "profilepicture.png";
            LastUpdated = "Last Updated: 02/08/25";

            this.BindingContext = this;
        }

        // Must unsubscribe when the page is destroyed (optional but good practice)
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<TenancyApplicationPage, PointEntry>(this, "ApplicationSubmitted");
        }

        // Load data from Preferences when the page appears
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserData();
            _ = GetUserLocationAsync();
        }

        // Updated Rank Logic
        private void LoadUserData()
        {
            // Default ID 0 should fail to find a user, prompting login/creation
            _loggedInUserId = Preferences.Get("LoggedInUserId", 0);

            string userName = Preferences.Get("UserName", "Guest");
            int userPoints = Preferences.Get("UserPoints", 0);

            // --- Rank Logic ---
            const int rankThreshold = 1000;
            string currentRank;
            int pointsBase;

            // Legendary Rank: 6000+ points (MAX RANK)
            if (userPoints >= 6000)
            {
                currentRank = "Legendary";
                pointsBase = 6000;
                RankColor = Color.FromHex("#FF8C00"); // Orange/Crimson glow
                ProgressRatio = 1.0;
                PointsAwayText = "MAX RANK ACHIEVED!";
            }
            // Crimson Rank: 5000 - 5999 points
            else if (userPoints >= 5000)
            {
                currentRank = "Crimson";
                pointsBase = 5000;
                RankColor = Color.FromHex("#DC143C"); // Crimson Red
                ProgressRatio = (double)(userPoints - pointsBase) / rankThreshold;
                PointsAwayText = $"{rankThreshold - (userPoints - pointsBase)} points away from Legendary!";
            }
            // Emerald Rank: 4000 - 4999 points
            else if (userPoints >= 4000)
            {
                currentRank = "Emerald";
                pointsBase = 4000;
                RankColor = Color.FromHex("#50C878"); // Emerald Green
                ProgressRatio = (double)(userPoints - pointsBase) / rankThreshold;
                PointsAwayText = $"{rankThreshold - (userPoints - pointsBase)} points away from Crimson!";
            }
            // Diamond Rank: 3000 - 3999 points
            else if (userPoints >= 3000)
            {
                currentRank = "Diamond";
                pointsBase = 3000;
                RankColor = Color.FromHex("#00BFFF"); // Deep Sky Blue
                ProgressRatio = (double)(userPoints - pointsBase) / rankThreshold;
                PointsAwayText = $"{rankThreshold - (userPoints - pointsBase)} points away from Emerald!";
            }
            // Gold Rank: 2000 - 2999 points
            else if (userPoints >= 2000)
            {
                currentRank = "Gold";
                pointsBase = 2000;
                RankColor = Color.FromHex("#FFD700"); // Gold
                ProgressRatio = (double)(userPoints - pointsBase) / rankThreshold;
                PointsAwayText = $"{rankThreshold - (userPoints - pointsBase)} points away from Diamond!";
            }
            // Silver Rank: 1000 - 1999 points
            else if (userPoints >= 1000)
            {
                currentRank = "Silver";
                pointsBase = 1000;
                RankColor = Color.FromHex("#C0C0C0"); // Silver
                ProgressRatio = (double)(userPoints - pointsBase) / rankThreshold;
                PointsAwayText = $"{rankThreshold - (userPoints - pointsBase)} points away from Gold!";
            }
            // Bronze Rank: 0 - 999 points
            else
            {
                currentRank = "Bronze";
                pointsBase = 0;
                RankColor = Color.FromHex("#CD7F32"); // Bronze
                ProgressRatio = (double)userPoints / rankThreshold;
                PointsAwayText = $"{rankThreshold - userPoints} points away from Silver!";
            }

            // --- Update Bound Properties ---
            UserName = userName;
            Points = $"{userPoints} / {pointsBase + rankThreshold} Points";
            RankText = $"Rank: {currentRank}";
        }


        // Method to add 100 points
        private async void OnAddPointsClicked(object sender, EventArgs e)
        {
            if (_loggedInUserId <= 0)
            {
                await DisplayAlert("Error", "Please log in to add points.", "OK");
                return;
            }

            var user = await _databaseService.GetUserByIdAsync(_loggedInUserId);

            if (user != null)
            {
                const int testPoints = 100;
                user.Points += testPoints;
                await _databaseService.SaveUserAsync(user);

                // 1. Update Preferences immediately
                Preferences.Set("UserPoints", user.Points);

                // 2. Refresh UI by reloading data
                LoadUserData();

                // 3. Add new entry to recent points (MOST RECENT is inserted at index 0)
                RecentPoints.Insert(0, new PointEntry
                {
                    Points = testPoints,
                    Description = "Test points added via button."
                });

                // Limit RecentPoints collection to 4 items
                const int maxRecentItems = 4;
                if (RecentPoints.Count > maxRecentItems)
                {
                    RecentPoints.RemoveAt(RecentPoints.Count - 1);
                }

                await DisplayAlert("Points Added", $"Your score is now {user.Points}!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "User record not found in database.", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // 1. Clear user data from Preferences
            Preferences.Remove("LoggedInUserId");
            Preferences.Remove("UserName");
            Preferences.Remove("UserPoints");

            // 2. Navigate to the guest-facing Home Page
            await Shell.Current.GoToAsync("//HomeTab");
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

        public async Task GetUserLocationAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status == PermissionStatus.Granted)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        UserLocation = $"Lat: {location.Latitude}, Long: {location.Longitude}";
                        Debug.WriteLine(UserLocation);
                    }
                    else
                    {
                        UserLocation = "Location not available.";
                    }
                }
                else
                {
                    UserLocation = "Location permission denied.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting location: {ex.Message}");
                UserLocation = "Error getting location.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}