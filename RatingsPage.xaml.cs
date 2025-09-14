using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;

namespace Rentrey.Maui
{
    public class PropertyRating
    {
        public string PropertyAddress { get; set; }
        public int Rating { get; set; }
        public string Review { get; set; }
        public string RatingText => $"{Rating} Stars";
    }

    public partial class RatingsPage : ContentPage
    {
        public ObservableCollection<PropertyRating> Ratings { get; set; }

        public RatingsPage()
        {
            InitializeComponent();

            Ratings = new ObservableCollection<PropertyRating>
            {
                new PropertyRating { PropertyAddress = "27 Aldenham Road", Rating = 5, Review = "Great landlord, great location. Highly recommend!" },
                new PropertyRating { PropertyAddress = "61 Butternut Ave", Rating = 4, Review = "Nice property, but the maintenance was a bit slow." },
                new PropertyRating { PropertyAddress = "123 Main St", Rating = 3, Review = "An okay place to live. Nothing special." }
            };

            BindingContext = this;
        }
    }
}
