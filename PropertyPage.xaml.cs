using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Globalization;

namespace Rentrey.Maui
{
    // C# class to represent the PropertyPage's view model
    public partial class PropertyPage : ContentPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string UserName { get; set; } = "Lachlan";
        public string Points { get; set; } = "790 / 1000 Points";
        public double ProgressRatio { get; set; } = 0.79;
        public string LastUpdated { get; set; } = "Last Updated: 02/08/25";
        
        public string PropertyImageSource { get; set; } = "house1.png";
        public string PropertyDetails { get; set; } = "4 🛏️ 2 🛁 2 🚗";
        public string PropertyAddress { get; set; } = "27 Aldenham Road";
        public string ChanceText { get; set; } = "High Chance at Successful Application";
        
        public PropertyPage()
        {
            InitializeComponent();
            this.BindingContext = this;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
