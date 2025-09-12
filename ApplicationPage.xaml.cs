using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System;
using RentreyApp.Services;

namespace Rentrey.Maui
{
    public partial class ApplicationPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        private ObservableCollection<ApplicationItem> _applications;
        public ObservableCollection<ApplicationItem> Applications
        {
            get => _applications;
            set
            {
                if (_applications != value)
                {
                    _applications = value;
                    OnPropertyChanged(nameof(Applications));
                }
            }
        }

        public ApplicationPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;

            LoadApplications();

            BindingContext = this;
        }

        private async void LoadApplications()
        {
            var applications = await _databaseService.GetApplicationsAsync();
            Applications = new ObservableCollection<ApplicationItem>(applications);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
