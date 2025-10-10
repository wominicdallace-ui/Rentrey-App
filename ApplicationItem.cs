using SQLite;
using Microsoft.Maui.Graphics;
using System;

namespace RentreyApp.Models
{
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Denied
    }

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

        private static Color GetColorForStatus(ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Pending => Color.FromArgb("#FFA726"),
                ApplicationStatus.Approved => Color.FromArgb("#4CAF50"),
                ApplicationStatus.Denied => Color.FromArgb("#EF5350"),
                _ => Color.FromArgb("#333333"),
            };
        }
    }
}
