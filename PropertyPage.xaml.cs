using Microsoft.Maui.Controls;
using Rentrey;
using System.ComponentModel;
using System.Globalization;

namespace Rentrey.Maui
{
    // A simple converter to handle the progress bar ratio


    public partial class PropertyPage : ContentPage
    {
        public PropertyPage()
        {
            InitializeComponent();

            // Create a placeholder Property object with hardcoded data for house1
            var placeholderProperty = new Property
            {
                ImageSource = "house1.png",
                Details = "4 🛏️ 2 🛁 2 🚗",
                Address = "27 Aldenham Road"
            };

            // Set the BindingContext of the page to the placeholder object
            this.BindingContext = placeholderProperty;
        }
    }
}
