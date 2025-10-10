using SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Rentrey;
using Rentrey.Maui;

namespace RentreyApp.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

            // Create tables if they don't exist
            _database.CreateTableAsync<Property>().Wait();
            _database.CreateTableAsync<ApplicationItem>().Wait();

            SeedProperties().ConfigureAwait(false);
        }

        #region Property Methods

        // Seed some initial property data
        private async Task SeedProperties()
        {
            try
            {
                var existingProperties = await GetPropertiesAsync();
                if (existingProperties == null || existingProperties.Count == 0)
                {
                    Debug.WriteLine("Seeding property data...");

                    var properties = new List<Property>
                    {
                        new Property {
                            ListingId = "11026031",
                            ImageSource = "p12maplewyoming.jpg",
                            Details = "3 🛏️ 2 🛁 2 🚗",
                            Address = "12 Maple St, Wyoming",
                            Price = 750,
                            Latitude = -33.40037495651697,
                            Longitude = 151.35753124645237,
                            MinimumRating = 1000,
                            PreferredRating = 2000,
                            ScrapedAt = new DateTime(2025, 9, 20)
                        },
                        new Property {
                            ListingId = "2072679238",
                            ImageSource = "p1oleandercanton.jpg",
                            Details = "4 🛏️ 3 🛁 2 🚗",
                            Address = "44 Oleander St, Canton Beach",
                            Price = 1000,
                            Latitude = -33.27438911103409,
                            Longitude = 151.54706085009724,
                            MinimumRating = 500,
                            PreferredRating = 1000,
                            ScrapedAt = new DateTime(2025, 9, 21)
                        },
                        new Property {
                            ListingId = "2059101425",
                            ImageSource = "p789pinetreeterrigal.jpg",
                            Details = "2 🛏️ 1 🛁 1 🚗",
                            Address = "789 Pine Tree Lane, Terrigal",
                            Price = 500,
                            Latitude = -33.448449907055746,
                            Longitude = 151.44528761024011,
                            MinimumRating = 1500,
                            PreferredRating = 2500,
                            ScrapedAt = new DateTime(2025, 9, 22)
                        },
                        new Property {
                            ListingId = "11026045",
                            ImageSource = "p101cedarrd.jpg",
                            Details = "5 🛏️ 4 🛁 3 🚗",
                            Address = "101 Cedar Rd, Redbank Plains",
                            Price = 1500,
                            Latitude = -27.648273688064506,
                            Longitude = 152.86058891172047,
                            MinimumRating = 750,
                            PreferredRating = 1500,
                            ScrapedAt = new DateTime(2025, 9, 23)
                        },
                        new Property {
                            ListingId = "448967054",
                            ImageSource = "p14silverbirchmardi.jpg",
                            Details = "3 🛏️ 2.5 🛁 2 🚗",
                            Address = "14 Silverbirch Ave, Mardi",
                            Price = 750,
                            Latitude = -33.29260646956398,
                            Longitude = 151.4126676709335,
                            MinimumRating = 1000,
                            PreferredRating = 2000,
                            ScrapedAt = new DateTime(2025, 9, 24)
                        },
                        new Property {
                            ListingId = "338575355",
                            ImageSource = "p15elmnarara.jpg",
                            Details = "4 🛏️ 2 🛁 1 🚗",
                            Address = "15 Elm St, Narara",
                            Price = 1000,
                            Latitude = -33.390642003537806,
                            Longitude = 151.35446442953696,
                            MinimumRating = 1250,
                            PreferredRating = 2250,
                            ScrapedAt = new DateTime(2025, 9, 25)
                        },
                        new Property {
                            ListingId = "63471549",
                            ImageSource = "p7sprucehamlyn.jpg",
                            Details = "1 🛏️ 1 🛁 1 🚗",
                            Address = "7 Spruce Cl, Hamlyn Terrace",
                            Price = 500,
                            Latitude = -33.248230908468784,
                            Longitude = 151.47099685322402,
                            MinimumRating = 500,
                            PreferredRating = 1500,
                            ScrapedAt = new DateTime(2025, 9, 26)
                        },
                        new Property {
                            ListingId = "p11026047",
                            ImageSource = "6willowtreewyong.jpg",
                            Details = "3 🛏️ 3 🛁 2 🚗",
                            Address = "6 Willow Tree Rd, Wyong",
                            Price = 1000,
                            Latitude = -33.26507860064559,
                            Longitude = 151.44661759984368,
                            MinimumRating = 1000,
                            PreferredRating = 2000,
                            ScrapedAt = new DateTime(2025, 9, 27)
                        },
                        new Property {
                            ListingId = "333814282",
                            ImageSource = "p11aspenterrigal.jpg",
                            Details = "2 🛏️ 2 🛁 2 🚗",
                            Address = "11 Aspen Ave, Terrigal",
                            Price = 750,
                            Latitude = -33.43420713643496,
                            Longitude = 151.43295651759433,
                            MinimumRating = 750,
                            PreferredRating = 1750,
                            ScrapedAt = new DateTime(2025, 9, 28)
                        },
                        new Property {
                            ListingId = "11026032",
                            ImageSource = "p38mistflychisholm.jpg",
                            Details = "5 🛏️ 5 🛁 3 🚗",
                            Address = "38 Mistfly St, Chisholm",
                            Price = 1500,
                            Latitude = -32.7545194149791,
                            Longitude = 151.6305670996335,
                            MinimumRating = 1500,
                            PreferredRating = 2500,
                            ScrapedAt = new DateTime(2025, 9, 29)
                        }
                    };

                    await _database.InsertAllAsync(properties);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error seeding properties: {ex.Message}");
            }
        }

        // Get all properties
        public Task<List<Property>> GetPropertiesAsync()
            => _database.Table<Property>().ToListAsync();

        // Save or update a property
        public Task<int> SavePropertyAsync(Property property)
            => property.Id != 0 ? _database.UpdateAsync(property) : _database.InsertAsync(property);

        #endregion

        #region Application Methods

        // ✅ Get all tenancy applications
        public Task<List<ApplicationItem>> GetApplicationsAsync()
            => _database.Table<ApplicationItem>()
                        .OrderByDescending(a => a.ApplicationDate)
                        .ToListAsync();

        // ✅ Insert a new application
        public Task<int> AddApplicationAsync(ApplicationItem application)
            => _database.InsertAsync(application);

        // ✅ Update an existing application
        public Task<int> UpdateApplicationAsync(ApplicationItem application)
            => _database.UpdateAsync(application);

        // ✅ Get applications for a specific property
        public Task<List<ApplicationItem>> GetApplicationsByPropertyIdAsync(int propertyId)
            => _database.Table<ApplicationItem>()
                        .Where(a => a.PropertyId == propertyId)
                        .OrderByDescending(a => a.ApplicationDate)
                        .ToListAsync();

        #endregion
    }
}
