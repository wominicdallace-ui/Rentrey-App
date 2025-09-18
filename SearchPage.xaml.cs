using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.IO;
using System.Threading.Tasks;
using Rentrey;
using RentreyApp.Services;
using System.Collections.Generic;
using System.Linq;

namespace Rentrey.Maui
{
    public partial class SearchPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Property> _allProperties;
        private ObservableCollection<Property> _newlyAddedProperties;
        private string _searchText;

        public ObservableCollection<Property> NewlyAddedProperties
        {
            get => _newlyAddedProperties;
            set
            {
                if (_newlyAddedProperties != value)
                {
                    _newlyAddedProperties = value;
                    OnPropertyChanged(nameof(NewlyAddedProperties));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    PerformSearch(value);
                }
            }
        }

        public ICommand NavigateToPropertyCommand { get; }

        public SearchPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            
            _allProperties = new ObservableCollection<Property>
            {
                new Property { ImageSource = "house1.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "27 Aldenham Road" },
                new Property { ImageSource = "house2.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "61 Butternut Ave" }
            };
            NewlyAddedProperties = _allProperties;

            NavigateToPropertyCommand = new Command<Property>(OnNavigateToProperty);
            this.BindingContext = this;
        }

        private void PerformSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                NewlyAddedProperties = _allProperties;
            }
            else
            {
                NewlyAddedProperties = new ObservableCollection<Property>(
                    _allProperties.Where(p => p.Address.ToLower().Contains(query.ToLower()))
                );
            }
        }
        
        private async void OnNavigateToProperty(Property property)
        {
            if (property == null)
                return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "property", property }
            };

            await Shell.Current.GoToAsync("PropertyPage", navigationParameter);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
