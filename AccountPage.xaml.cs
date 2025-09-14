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

namespace Rentrey.Maui
{
    public class RatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double ratio)
            {
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

        public string UserName { get; set; }
        public string Points { get; set; }
        public string LastUpdated { get; set; }
        public string RankText { get; set; }
        public string PointsAwayText { get; set; }
        public double ProgressRatio { get; set; }

        public ObservableCollection<PointEntry> RecentPoints { get; set; }
        public ObservableCollection<Badge> EarnedBadges { get; set; }

        public AccountPage()
        {
            InitializeComponent();

            UserName = "Lachlan";
            Points = "790 / 1000 Points";
            LastUpdated = "Last Updated: 02/08/25";
            RankText = "Rank: Bronze";
            PointsAwayText = "210 points away from Silver!";
            ProgressRatio = 0.79; // 790/1000

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

            ProfileImageSource = "profilepicture.png";

            _ = GetUserLocationAsync();

            this.BindingContext = this;
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
