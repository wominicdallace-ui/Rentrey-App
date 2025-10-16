using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using RentreyApp.Services;
using System.Threading.Tasks;
using Rentrey;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Rentrey.Maui
{
    public partial class PropertyPage : ContentPage, IQueryAttributable, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        private int _userScore = 0; // Field to hold the dynamic user score

        private Property _property;

        // Color of the application status bar
        private Color _applicationColor = Color.FromHex("#E0E0E0");
        public Color ApplicationColor
        {
            get => _applicationColor;
            set
            {
                if (_applicationColor != value)
                {
                    _applicationColor = value;
                    OnPropertyChanged(nameof(ApplicationColor));
                }
            }
        }

        // Text inside the application status bar
        private string _applicationStatusText = "Checking eligibility...";
        public string ApplicationStatusText
        {
            get => _applicationStatusText;
            set
            {
                if (_applicationStatusText != value)
                {
                    _applicationStatusText = value;
                    OnPropertyChanged(nameof(ApplicationStatusText));
                }
            }
        }

        // Main property model used for data binding in XAML
        public Property Property
        {
            get => _property;
            set
            {
                if (_property != value)
                {
                    _property = value;
                    OnPropertyChanged(nameof(Property));

                    // Update the eligibility bar whenever new property data is loaded
                    if (value != null)
                    {
                        CalculateApplicationChance(value);
                    }
                }
            }
        }

        // Header UI data
        public string UserName { get; set; } = "Lachlan";
        public string Points { get; set; } = "790 / 1000 Points";
        public double ProgressRatio { get; set; } = 0.79;
        public string LastUpdated { get; set; } = "Last Updated: 02/08/25";
        public string ProfileImageSource { get; set; } = "profile_icon.png";

        // ⭐ NEW: Color property for points text and progress bar
        private Color _rankColor;
        public Color RankColor
        {
            get => _rankColor;
            set { if (_rankColor != value) { _rankColor = value; OnPropertyChanged(nameof(RankColor)); } }
        }

        public PropertyPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;

            LoadUserData(); // Load user data upon initialization

            this.BindingContext = this;
        }

        // ⭐ REFACTORED: Loads user data, calculates rank, color, and progress
        private void LoadUserData()
        {
            // Retrieve data from Preferences (guaranteed to be consistent with HomePage/AccountPage)
            int userPoints = Preferences.Get("UserPoints", 0);
            string userName = Preferences.Get("UserName", "Guest");

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
            _userScore = userPoints; // Used for eligibility check (CalculateApplicationChance)
            UserName = userName;
            // Points display format (e.g., 790 / 1000 Points)
            Points = $"{userPoints} / {pointsBase + rankThreshold} Points";

            // Trigger property updates for data binding
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(Points));
            OnPropertyChanged(nameof(ProgressRatio));
        }


        // Calculates application chance and updates the color/text accordingly
        private void CalculateApplicationChance(Property property)
        {
            if (property == null) return;

            // UPDATED: Use the dynamic _userScore instead of the constant UserScore
            if (_userScore >= property.PreferredRating)
            {
                ApplicationStatusText = "Great Chance at Successful Application";
                ApplicationColor = Color.FromArgb("#2E7D32"); // Green
            }
            else if (_userScore >= property.MinimumRating)
            {
                ApplicationStatusText = "Good Chance at Successful Application";
                ApplicationColor = Color.FromArgb("#FFC107"); // Yellow/Amber
            }
            else
            {
                ApplicationStatusText = "Low Chance at Successful Application";
                ApplicationColor = Color.FromArgb("#D32F2F"); // Red
            }
        }

        // Called when navigation passes query parameters
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("propertyId") && query["propertyId"] is string propertyId)
            {
                await GetPropertyDetailsFromDatabase(propertyId);
            }
        }

        // Fetch the property details from the database using the propertyId passed via query
        private async Task GetPropertyDetailsFromDatabase(string listingId)
        {
            try
            {
                var allProperties = await _databaseService.GetPropertiesAsync();
                var detailedProperty = allProperties.FirstOrDefault(p => p.ListingId == listingId);

                if (detailedProperty != null)
                {
                    Property = detailedProperty; // This triggers OnPropertyChanged and updates bindings
                }
                else
                {
                    Debug.WriteLine($"No property found with ID: {listingId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get property details from database: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (Property == null) return;

            var query = new Dictionary<string, object>
                {
                    { "propertyId", Property.ListingId }
                };

            await Shell.Current.GoToAsync("///TenancyApplicationPage", query);
        }
    }
}