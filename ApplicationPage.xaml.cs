using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using RentreyApp.Services;
using RentreyApp.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

            BindingContext = this;
            LoadPendingApplications();
        }

        private async void LoadPendingApplications()
        {
            var applications = await _databaseService.GetApplicationsAsync();

            // ✅ Filter only pending applications
            var pendingApplications = applications
                .Where(a => a.Status == ApplicationStatus.Pending)
                .OrderByDescending(a => a.ApplicationDate)
                .ToList();

            Applications = new ObservableCollection<ApplicationItem>(pendingApplications);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
