using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Rentrey.Maui
{
    // Data model for the recent updates section
    public class RecentUpdate
    {
        public string IconSource { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    // Data model for the newly added properties section
    public class Property
    {
        public string ImageSource { get; set; }
        public string Details { get; set; }
        public string Address { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        public string UserName { get; set; }
        public string Points { get; set; }
        public double ProgressRatio { get; set; }
        public string LastUpdated { get; set; }

        public ObservableCollection<RecentUpdate> RecentUpdates { get; set; }
        public ObservableCollection<Property> NewlyAddedProperties { get; set; }

        public MainPage()
        {
            InitializeComponent();

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

            // Initialize the Newly Added Properties collection
            NewlyAddedProperties = new ObservableCollection<Property>
            {
                new Property { ImageSource = "house1.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "27 Aldenham Road" },
                new Property { ImageSource = "house2.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "61 Butternut Ave" }
            };

            // Set the BindingContext
            BindingContext = this;
        }
    }
}
