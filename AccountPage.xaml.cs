using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace Rentrey
{
    // progress bar
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

    // data models
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

    // The ViewModel for the page, implementing INotifyPropertyChanged
    public partial class AccountPage : ContentPage
    {
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

            Resources.Add("RatioConverter", new RatioConverter());

            UserName = "User";
            Points = "790 / 1000 Points";
            LastUpdated = "Last Updated: 02/08/25";
            RankText = "Rank: Bronze";
            PointsAwayText = "210 points away from Silver!";
            ProgressRatio = 0.79; // 790/1000

            // Initialise the collections
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

            
            this.BindingContext = this;
        }
    }
}
