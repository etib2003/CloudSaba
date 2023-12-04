// Services/ApiServices.cs

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using CloudSaba.Models;
using Microsoft.AspNetCore.Mvc;
using Weather = CloudSaba.Models.Weather;


namespace CloudSaba.Services
{
    public class ApiServices
    {
        public async Task<bool> CheckImage(string imageURL)
        {
            var apiUrl = $"http://localhost:5050/ImaggaApi/CheckImage?imageUrl={imageURL}";

            // Create an instance of HttpClient
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                // Send a GET request to the other project's endpoint
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the response content manually
                    var result = JsonConvert.DeserializeObject<bool?>(content);

                    // Use the result directly in the if statement
                    return result ?? false; // If result is null, default to false
                }
                else
                {
                    // Handle the error
                    return false; // Return false or handle the error accordingly
                }
            }
        }

        public async Task<bool> CheckAddressExistence(string city, string street)
        {
            var apiUrl = $"http://localhost:5050/api/Address/check?city={city}&street={street}";

            // Create an instance of HttpClient
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                // Send a GET request to the other project's endpoint
                var response = await httpClient.GetAsync(apiUrl);


                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the response content manually
                    var result = JsonConvert.DeserializeObject<bool?>(content);

                    // Use the result directly in the if statement
                    return result ?? false; // If result is null, default to false
                }
                else
                {
                    // Handle the error
                    return false; // Return false or handle the error accordingly
                }
            }
        }
        
        public async Task<Weather> FindWeatherAsync(string city)
        {
            // Construct the URL to the API Gateway's GetWeather endpoint
            string apiUrl = $"http://localhost:5050/Weather?city={city}";

            try
            {
                // Create an instance of HttpClient
                using (var httpClient = new HttpClient())
                {
                    // Send a GET request to the API Gateway's GetWeather endpoint
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Deserialize the JSON response into a Weather object
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        var weather = JsonConvert.DeserializeObject<Weather>(jsonContent);
                        return weather;
                    }
                    else
                    {
                        // Handle errors here
                        return new Weather();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return new Weather();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> IsItHoliday()
        {
            // Construct the URL of the other project's endpoint
            string apiUrl = $"http://localhost:5050/Get";

            try
            {
                // Create an instance of HttpClient
                using (var httpClient = new HttpClient())
                {
                    // Send a GET request to the other project's endpoint
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response content as a boolean value
                        bool isHoliday = bool.Parse(await response.Content.ReadAsStringAsync());

                        return isHoliday;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
