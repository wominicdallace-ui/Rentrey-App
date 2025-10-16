using SQLite;

namespace RentreyApp.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string TaxFileNumber { get; set; }
        public int Points { get; set; } = 0;
    }
}
