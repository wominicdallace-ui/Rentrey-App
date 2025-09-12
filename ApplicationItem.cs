using SQLite;
using System;
using Microsoft.Maui.Graphics;
using System.ComponentModel;

namespace Rentrey.Maui
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
        public int PropertyId { get; set; } // Foreign key to link to a Property
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
}
