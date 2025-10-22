using SQLite;
using System;

namespace Rentrey;

[Table("Properties")]
public class Property
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string ListingId { get; set; }
    public string ImageSource { get; set; }
    public string Details { get; set; }
    public string Address { get; set; }
    public int Price { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public int MinimumRating { get; set; }
    public int PreferredRating { get; set; }
    public DateTime ScrapedAt { get; set; }
    public bool IsUserAdded { get; set; } = false;
}
