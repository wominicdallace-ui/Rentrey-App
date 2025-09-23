using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Rentrey;

namespace RentreyApp.Services
{
    public class ProptrackService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "YOUR_API_KEY_HERE";
        private readonly string _baseUrl = "https://api.proptrack.com.au/v2";

        public ProptrackService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        }

        public async Task<List<Property>> GetListingsAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_baseUrl}/listings");
                var apiResponse = JsonSerializer.Deserialize<ListingsResponse>(response);
                
                var properties = apiResponse.Listings.Select(l => new Property
                {
                    ImageSource = l.MainImage.Url,
                    Details = l.Bedrooms + " 🛏️ " + l.Bathrooms + " 🛁 " + l.ParkingSpaces + " 🚗",
                    Address = l.Address.StreetAddress
                }).ToList();

                return properties;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return new List<Property>();
            }
        }
        
        public async Task<Property> GetListingAsync(string listingId)
        {
            try
            {
                // Construct the full API URL for a single listing
                var response = await _httpClient.GetStringAsync($"{_baseUrl}/listing/{listingId}");
                var listing = JsonSerializer.Deserialize<Listing>(response);
                
                // Map the API response to your Property model
                return new Property
                {
                    ImageSource = listing.MainImage.Url,
                    Details = listing.Bedrooms + " 🛏️ " + listing.Bathrooms + " 🛁 " + listing.ParkingSpaces + " 🚗",
                    Address = listing.Address.StreetAddress
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return null;
            }
        }
    }
    
    // Data models to match the Proptrack API's JSON response structure.
    public class ListingsResponse
    {
        public List<Listing> Listings { get; set; }
    }
    
    public class Listing
    {
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int ParkingSpaces { get; set; }
        public ListingAddress Address { get; set; }
        public ListingImage MainImage { get; set; }
    }
    
    public class ListingAddress
    {
        public string StreetAddress { get; set; }
    }
    
    public class ListingImage
    {
        public string Url { get; set; }
    }
}
