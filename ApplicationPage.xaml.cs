using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using RentreyApp.Services;
using RentreyApp.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Input; // ⭐ ADDED for ICommand

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

        // ⭐ NEW: Command for deleting an application
        public ICommand DeleteApplicationCommand { get; }

        public ApplicationPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;

            // ⭐ Initialize the delete command
            DeleteApplicationCommand = new Command<ApplicationItem>(async (item) => await OnDeleteApplicationClicked(item));

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadApplications();
        }

        private async void LoadApplications()
        {
            var applications = await _databaseService.GetApplicationsAsync();

            var allApplications = applications
                .OrderByDescending(a => a.ApplicationDate)
                .ToList();

            Applications = new ObservableCollection<ApplicationItem>(allApplications);
        }

        // ⭐ NEW: Logic to handle the delete button click
        private async Task OnDeleteApplicationClicked(ApplicationItem application)
        {
            if (application == null)
                return;

            // 1. Ask for confirmation
            bool confirm = await DisplayAlert("Confirm Deletion",
                                              $"Are you sure you want to withdraw and delete the application for {application.PropertyAddress}?",
                                              "Yes, Delete", "Cancel");

            if (confirm)
            {
                try
                {
                    // 2. Delete from database
                    await _databaseService.DeleteApplicationAsync(application);

                    // 3. Remove from ObservableCollection to update UI instantly
                    Applications.Remove(application);

                    await DisplayAlert("Success", "Application deleted.", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete application: {ex.Message}", "OK");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}