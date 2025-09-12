using SQLite;

namespace Rentrey;

public class Property
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string ImageSource { get; set; }
    public string Details { get; set; }
    public string Address { get; set; }
}
