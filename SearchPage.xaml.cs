using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Rentrey.Maui
{
    // A simple data model for the properties
    public class Property
    {
        public string ImageSource { get; set; }
        public string Details { get; set; }
        public string Address { get; set; }
    }

    public partial class SearchPage : ContentPage
    {
        public ObservableCollection<Property> NewlyAddedProperties { get; set; }

        public SearchPage()
        {
            InitializeComponent();

            // Initialize the properties collection with placeholder data
            NewlyAddedProperties = new ObservableCollection<Property>
            {
                new Property { ImageSource = "house1.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "27 Aldenham Road" },
                new Property { ImageSource = "house2.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "61 Butternut Ave" }
            };

            // Set the BindingContext for the page
            this.BindingContext = this;
        }
    }
}
