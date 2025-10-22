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
using System.Windows.Input;
using RentreyApp.Services;
using Rentrey;
using System.Linq;

namespace Rentrey.Maui
{
    // PropertyRating, NewReviewViewModel, and other Data Models are omitted for brevity

    [SQLite.Table("Ratings")]
    public class PropertyRating : INotifyPropertyChanged
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
        public string PropertyAddress { get; set; }
        public string UserName { get; set; } // Reviewer's name
        public int RatingScore { get; set; }
        public string Review { get; set; }
        public DateTime ReviewDate { get; set; }

        [SQLite.Ignore]
        public string RatingText => string.Concat(Enumerable.Repeat("★", RatingScore)).PadRight(5, '☆');

        [SQLite.Ignore]
        public string ShortReview => Review.Length > 100 ? Review.Substring(0, 97) + "..." : Review;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class NewReviewViewModel : INotifyPropertyChanged
    {
        private string _reviewText;
        public string ReviewText
        {
            get => _reviewText;
            set { _reviewText = value; OnPropertyChanged(nameof(ReviewText)); }
        }

        private int _rating = 5;
        public int Rating
        {
            get => _rating;
            set { _rating = value; OnPropertyChanged(nameof(Rating)); }
        }

        private Property _selectedProperty;
        public Property SelectedProperty
        {
            get => _selectedProperty;
            set { _selectedProperty = value; OnPropertyChanged(nameof(SelectedProperty)); }
        }

        public ObservableCollection<Property> AvailableProperties { get; set; } = new ObservableCollection<Property>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class RatingsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        private ObservableCollection<PropertyRating> _ratings;
        public ObservableCollection<PropertyRating> Ratings
        {
            get => _ratings;
            set { _ratings = value; OnPropertyChanged(nameof(Ratings)); }
        }

        private bool _isReviewModalVisible;
        public bool IsReviewModalVisible
        {
            get => _isReviewModalVisible;
            set { _isReviewModalVisible = value; OnPropertyChanged(nameof(IsReviewModalVisible)); }
        }

        public NewReviewViewModel NewReviewData { get; set; }

        private PropertyRating _selectedRating;
        public PropertyRating SelectedRating
        {
            get => _selectedRating;
            set { _selectedRating = value; OnPropertyChanged(nameof(SelectedRating)); }
        }

        private bool _isFullReviewModalVisible;
        public bool IsFullReviewModalVisible
        {
            get => _isFullReviewModalVisible;
            set { _isFullReviewModalVisible = value; OnPropertyChanged(nameof(IsFullReviewModalVisible)); }
        }

        public ICommand DeleteRatingCommand { get; }
        public ICommand ViewRatingCommand { get; }


        public RatingsPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;

            Ratings = new ObservableCollection<PropertyRating>();
            NewReviewData = new NewReviewViewModel();

            DeleteRatingCommand = new Command<PropertyRating>(async (item) => await OnDeleteRatingClicked(item));
            ViewRatingCommand = new Command<PropertyRating>(OnRatingItemTapped);


            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadRatings();
            _ = LoadAvailableProperties();
        }

        private async Task LoadRatings()
        {
            var ratingsList = await _databaseService.GetPropertyRatingsAsync();
            Ratings = new ObservableCollection<PropertyRating>(ratingsList);
        }

        private async Task LoadAvailableProperties()
        {
            var propertyList = await _databaseService.GetPropertiesForReviewAsync();
            NewReviewData.AvailableProperties.Clear();
            foreach (var prop in propertyList)
            {
                NewReviewData.AvailableProperties.Add(prop);
            }
        }

        private void OnRatingItemTapped(PropertyRating rating)
        {
            if (rating != null)
            {
                SelectedRating = rating;
                IsFullReviewModalVisible = true;
            }
        }

        private async Task OnDeleteRatingClicked(PropertyRating rating)
        {
            if (rating == null)
                return;

            // 1. Ask for confirmation
            bool confirm = await DisplayAlert("Confirm Deletion",
                                              $"Are you sure you want to delete your review for {rating.PropertyAddress}?",
                                              "Yes, Delete", "Cancel");

            if (confirm)
            {
                try
                {
                    // 2. Delete from database
                    await _databaseService.DeleteRatingAsync(rating);

                    // 3. Remove from ObservableCollection to update UI instantly
                    Ratings.Remove(rating);

                    await DisplayAlert("Success", "Review deleted.", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete review: {ex.Message}", "OK");
                }
            }
        }


        // --- Existing Handlers ---
        private void OnReviewButtonClicked(object sender, EventArgs e)
        {
            // Reset modal data for new review
            NewReviewData.ReviewText = "";
            NewReviewData.Rating = 5;
            NewReviewData.SelectedProperty = NewReviewData.AvailableProperties.FirstOrDefault();

            IsReviewModalVisible = true;
        }

        private async void OnSubmitReviewClicked(object sender, EventArgs e)
        {
            if (NewReviewData.SelectedProperty == null || string.IsNullOrWhiteSpace(NewReviewData.ReviewText))
            {
                await DisplayAlert("Error", "Please select a property and write a review.", "OK");
                return;
            }

            // Check if user is logged in
            int userId = Preferences.Get("LoggedInUserId", 0);
            if (userId <= 0)
            {
                await DisplayAlert("Error", "You must be logged in to submit a review.", "OK");
                return;
            }

            // Get reviewer details
            string userName = Preferences.Get("UserName", "Guest User");
            const int reviewPointsReward = 50; // ⭐ Reward amount

            // --- 1. Save Rating ---
            var newRating = new PropertyRating
            {
                PropertyAddress = NewReviewData.SelectedProperty.Address,
                UserName = userName,
                RatingScore = NewReviewData.Rating,
                Review = NewReviewData.ReviewText,
                ReviewDate = DateTime.Now
            };
            await _databaseService.SaveRatingAsync(newRating);

            // --- 2. Reward User ---
            var user = await _databaseService.GetUserByIdAsync(userId);
            if (user != null)
            {
                user.Points += reviewPointsReward;
                await _databaseService.SaveUserAsync(user);

                // Update session data
                Preferences.Set("UserPoints", user.Points);

                // ⭐ Send message to AccountPage to update Recent Points and Score
                MessagingCenter.Send(this, "ApplicationSubmitted", // Reusing the message key for consistency
                    new PointEntry { Points = reviewPointsReward, Description = "You earned 50 points for submitting a review!" });
            }

            // --- 3. Refresh UI and Close Modal ---
            await LoadRatings(); // Refresh the list of displayed ratings

            IsReviewModalVisible = false;
            await DisplayAlert("Review Submitted", $"Thank you for your feedback! (+{reviewPointsReward} points)", "OK");
        }

        private void OnCancelReviewClicked(object sender, EventArgs e)
        {
            IsReviewModalVisible = false;
        }

        private void OnCloseFullReviewModalClicked(object sender, EventArgs e)
        {
            IsFullReviewModalVisible = false;
            SelectedRating = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}