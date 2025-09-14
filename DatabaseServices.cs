using SQLite;
using System.Threading.Tasks;
using Rentrey.Maui;
using System.Collections.Generic;

namespace RentreyApp.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);

        _database.CreateTableAsync<Rentrey.Property>().Wait();
        _database.CreateTableAsync<ApplicationItem>().Wait();

        SeedDatabase();
    }

    public Task<int> SavePropertyAsync(Rentrey.Property property)
    {
        if (property.Id != 0)
        {
            return _database.UpdateAsync(property);
        }
        else
        {
            return _database.InsertAsync(property);
        }
    }

    public Task<int> SaveApplicationAsync(ApplicationItem application)
    {
        if (application.Id != 0)
        {
            return _database.UpdateAsync(application);
        }
        else
        {
            return _database.InsertAsync(application);
        }
    }

    public Task<List<Rentrey.Property>> GetPropertiesAsync()
    {
        return _database.Table<Rentrey.Property>().ToListAsync();
    }

    public Task<List<ApplicationItem>> GetApplicationsAsync()
    {
        return _database.Table<ApplicationItem>().ToListAsync();
    }

    private void SeedDatabase()
    {
        if (_database.Table<Rentrey.Property>().CountAsync().Result == 0)
        {
            _database.InsertAsync(new Rentrey.Property { ImageSource = "house1.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "27 Aldenham Road" });
            _database.InsertAsync(new Rentrey.Property { ImageSource = "house2.png", Details = "4 🛏️ 2 🛁 2 🚗", Address = "61 Butternut Ave" });
        }
        if (_database.Table<ApplicationItem>().CountAsync().Result == 0)
        {
            _database.InsertAsync(new ApplicationItem { PropertyAddress = "27 Aldenham Road", ApplicationDate = DateTime.Today, Status = ApplicationStatus.Pending });
            _database.InsertAsync(new ApplicationItem { PropertyAddress = "61 Butternut Ave", ApplicationDate = DateTime.Today.AddDays(-5), Status = ApplicationStatus.Approved });
            _database.InsertAsync(new ApplicationItem { PropertyAddress = "123 Main St", ApplicationDate = DateTime.Today.AddDays(-10), Status = ApplicationStatus.Denied });
        }
    }
}
