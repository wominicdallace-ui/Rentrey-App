using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System;
using RentreyApp.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Rentrey;
using Microsoft.Maui.Graphics;
using SQLite;

namespace Rentrey.Maui
{
    [Table("Applications")]
    public class ApplicationItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyAddress { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }

        [Ignore]
        public Color StatusColor => GetColorForStatus(Status);
        [Ignore]
        public string StatusText => $"Status: {Status}";

        private Color GetColorForStatus(ApplicationStatus status)
        {
            switch (status)
            {
                case ApplicationStatus.Pending:
                    return Color.FromArgb("#FFA726");
                case ApplicationStatus.Approved:
                    return Color.FromArgb("#4CAF50");
                case ApplicationStatus.Denied:
                    return Color.FromArgb("#EF5350");
                default:
                    return Color.FromArgb("#333333");
            }
        }
    }

    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Denied
    }

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