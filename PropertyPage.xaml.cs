using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using RentreyApp.Services;
using System.Threading.Tasks;

namespace Rentrey.Maui
{
    public partial class PropertyPage : ContentPage, IQueryAttributable, INotifyPropertyChanged
    {
        private readonly ProptrackService _proptrackService;
        private Property _property;

        public Property Property
        {
            get => _property;
            set
            {
                _property = value;
                OnPropertyChanged(nameof(Property));
            }
        }

        // Inject the ProptrackService into the constructor
        public PropertyPage(ProptrackService proptrackService)
        {
            InitializeComponent();
            _proptrackService = proptrackService;
            BindingContext = this;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("property") && query["property"] is Property receivedProperty)
            {
                this.Property = receivedProperty;
                
                // Call the API to get more detailed information using the property's ID
                await GetPropertyDetailsFromApi(this.Property.Id.ToString());
            }
        }
        
        // New method to fetch property details from the API
        private async Task GetPropertyDetailsFromApi(string propertyId)
        {
            try
            {
                // Call the new GetListingAsync method in your service
                var detailedProperty = await _proptrackService.GetListingAsync(propertyId);
                
                // Update the Property object with the new, detailed information
                if (detailedProperty != null)
                {
                    this.Property = detailedProperty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get property details from API: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
