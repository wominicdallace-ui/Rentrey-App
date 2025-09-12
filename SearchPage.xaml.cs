using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.IO;
using System.Threading.Tasks;
using Rentrey;

namespace Rentrey.Maui
{
    public partial class SearchPage : ContentPage, INotifyPropertyChanged
    {
        public ObservableCollection<Property> NewlyAddedProperties { get; set; }
        public ICommand NavigateToPropertyCommand { get; }

        public SearchPage()
        {
            InitializeComponent();

            // Initialize the properties collection with placeholder data
            NewlyAddedProperties = new ObservableCollection<Property>
            {
                new Property { ImageSource = "house1.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "27 Aldenham Road" },
                new Property { ImageSource = "house2.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "61 Butternut Ave" }
            };

            // Initialize the command for navigation
            NavigateToPropertyCommand = new Command<Property>(OnNavigateToProperty);

            // Set the BindingContext
            this.BindingContext = this;
        }

        private async void OnNavigateToProperty(Property property)
        {
            if (property == null)
                return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "property", property }
            };

            // Use an absolute path with the tab and child page route names
            await Shell.Current.GoToAsync($"//HomePage/PropertyPage", navigationParameter);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
