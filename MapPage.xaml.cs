using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using RentreyApp.Services;
using System.Collections.Generic;
using System.Windows.Input;

namespace Rentrey.Maui
{
    public partial class MapPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Property> _properties;
        private string _searchText;
        private Property _selectedPinProperty;
        private double _popupStartY;
        private bool _isDragging = false;

        public ObservableCollection<Property> Properties
        {
            get => _properties;
            set { _properties = value; OnPropertyChanged(nameof(Properties)); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); FilterProperties(); }
        }

        // Command for CollectionView item taps
        public ICommand PropertyTappedCommand { get; }

        public Command SearchCommand { get; }

        public MapPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            BindingContext = this;

            SearchCommand = new Command(FilterProperties);

            // Initialize the item tap command
            PropertyTappedCommand = new Command<Property>(OnPropertyTapped);

            LoadProperties();
        }

        private async void LoadProperties()
        {
            var properties = await _databaseService.GetPropertiesAsync();
            Properties = new ObservableCollection<Property>(properties);

            AddPinsToMap();
            CenterMapOnProperties();
        }

        private void AddPinsToMap()
        {
            PropertyMap.Pins.Clear();

            foreach (var property in Properties)
            {
                var pin = new Pin
                {
                    Label = property.Address,
                    Address = property.Details,
                    Type = PinType.Place,
                    Location = new Location(property.Latitude, property.Longitude)
                };

                pin.MarkerClicked += (s, args) =>
                {
                    args.HideInfoWindow = true;
                    ShowPopup(property);
                };

                PropertyMap.Pins.Add(pin);
            }
        }

        private void CenterMapOnProperties()
        {
            if (!Properties.Any()) return;

            var centerLat = Properties.Average(p => p.Latitude);
            var centerLon = Properties.Average(p => p.Longitude);
            var centerLocation = new Location(centerLat, centerLon);

            var minLat = Properties.Min(p => p.Latitude);
            var maxLat = Properties.Max(p => p.Latitude);
            var minLon = Properties.Min(p => p.Longitude);
            var maxLon = Properties.Max(p => p.Longitude);

            PropertyMap.MoveToRegion(new MapSpan(centerLocation, maxLat - minLat, maxLon - minLon));
        }

        private async void FilterProperties()
        {
            var allProperties = await _databaseService.GetPropertiesAsync();

            if (string.IsNullOrWhiteSpace(SearchText))
                Properties = new ObservableCollection<Property>(allProperties);
            else
                Properties = new ObservableCollection<Property>(
                    allProperties.Where(p => p.Address.ToLower().Contains(SearchText.ToLower()))
                );

            AddPinsToMap();
            CenterMapOnProperties();
        }

        private async void ShowPopup(Property property)
        {
            _selectedPinProperty = property;

            PopupAddress.Text = property.Address;
            PopupDetails.Text = property.Details;
            PopupPrice.Text = $"${property.Price} p/w";

            if (!string.IsNullOrEmpty(property.ImageSource))
                PopupImage.Source = property.ImageSource.StartsWith("http")
                                    ? ImageSource.FromUri(new Uri(property.ImageSource))
                                    : ImageSource.FromFile(property.ImageSource);
            else
                PopupImage.Source = "placeholder_house.png";

            PopupCard.TranslationY = 200;
            PopupCard.Opacity = 0;
            PopupCard.IsVisible = true;

            PopupOverlay.IsVisible = true;
            PopupOverlay.Opacity = 0;

            // Animate popup and overlay
            await Task.WhenAll(
                PopupCard.TranslateTo(0, 0, 250, Easing.CubicOut),
                PopupCard.FadeTo(1, 250, Easing.CubicIn),
                PopupOverlay.FadeTo(1, 250, Easing.CubicIn)
            );

            PropertyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(property.Latitude, property.Longitude),
                Distance.FromMeters(500)
            ));
        }

        private async Task HidePopupAsync()
        {
            if (!PopupCard.IsVisible) return;

            await Task.WhenAll(
                PopupCard.TranslateTo(0, 200, 200, Easing.CubicIn),
                PopupCard.FadeTo(0, 200, Easing.CubicOut),
                PopupOverlay.FadeTo(0, 200, Easing.CubicOut)
            );

            PopupCard.IsVisible = false;
            PopupOverlay.IsVisible = false;
        }

        private void OnPopupPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _popupStartY = PopupCard.TranslationY;
                    _isDragging = true;
                    break;

                case GestureStatus.Running:
                    if (_isDragging)
                        PopupCard.TranslationY = _popupStartY + e.TotalY;
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    _isDragging = false;
                    if (PopupCard.TranslationY > 100)
                        _ = HidePopupAsync();
                    else
                        PopupCard.TranslateTo(0, 0, 150, Easing.CubicOut);
                    break;
            }
        }

        private async void OnPopupViewDetailsClicked(object sender, EventArgs e)
        {
            if (_selectedPinProperty != null)
                NavigateToProperty(_selectedPinProperty);
        }

        private async void OnPropertyTapped(Property property)
        {
            if (property != null)
            {
                NavigateToProperty(property);
            }
        }

        private async void NavigateToProperty(Property property)
        {
            if (property == null) return;

            var navParam = new Dictionary<string, object> { { "propertyId", property.ListingId } };
            await Shell.Current.GoToAsync("PropertyPage", navParam);
        }

        private async void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            await HidePopupAsync();
        }

        private async void OnOverlayTapped(object sender, EventArgs e)
        {
            await HidePopupAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
