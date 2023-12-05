using CloudSaba.Data;
using CloudSaba.Models;
using CloudSaba.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;



namespace CloudSaba.Controllers
{
    public class ManagerController : Controller
    {
        private readonly CloudSabaContext _context;//ETI
        private readonly ApiServices _apiServices; // Add this line
        Random random = new Random();

        

        public ManagerController(CloudSabaContext context, ApiServices apiServices) // Add ApiServices parameter
        {
            _context = context;
            _apiServices = apiServices; // Initialize ApiServices
        }



        // Inside ManagerController.cs
        private string GetOrdersDataAsString(Order order)
        {
            //var flavors = string.Join(", ", order.Products?.Select(p => p.Flavor) ?? Enumerable.Empty<string>());//real
            //var listOfFlavors = "Chocolate, vanilla, pistachio, banana";//stam
            //var fixedFlavor = listOfFlavors.FirstOrDefault(); // Choose the desired method to select the fixed flavor//stam
            //var flavors = string.Join(", ", Enumerable.Repeat(fixedFlavor, order.Products?.Count ?? 0));//stam
            // Format order data into a string
            // יצירת מספר אקראי בין 1 ל-5
            var amount = random.Next(1, 5); //order.Products?.Count;
            var orderDataString = $@"
        City: {order.City}
        Date: {order.Date}
        FeelsLike: {order.FeelsLike}
        Humidity: {order.Humidity}
        Day: {order.Day}
    ";

            return orderDataString;
        }



        private string CallGPTAPI(string Prompt)
        {
            var gptApiKey = "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";

            var gptApiUrl = "https://api.openai.com/v1/completions";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {gptApiKey}");

                try
                {
                    var formattedPrompt = JsonConvert.SerializeObject(new
                    {
                        model = "gpt-3.5-turbo-instruct",
                        prompt =Prompt ,
                        max_tokens = 100
                    });


                    var content = new StringContent(formattedPrompt, Encoding.UTF8, "application/json");


                    var response = httpClient.PostAsync(gptApiUrl, content).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    // Console log for debugging
                    Console.WriteLine($"GPT API Response Status Code: {response.StatusCode}");
                    Console.WriteLine("GPT API Response Content: " + responseContent);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the JSON string to a C# object
                        var jsonResponse = JsonConvert.DeserializeObject<MyResponseType>(responseContent);

                        // Extract the text from the parsed object
                        var extractedText = jsonResponse.choices[0].text;

                        // Now 'extractedText' contains the desired text
                        return extractedText;
                        //return responseContent.choices[0].text; // צריך לעשות פארסינג למחרוזת כדי להוציא את החלק של הטקסט. 
                    }
                    else
                    {
                        // Parse and handle the error message from the response content
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                        var errorMessage = $"GPT API request failed with status code: {response.StatusCode}, Message: {errorResponse?.Error?.Message}";
                        throw new HttpRequestException(errorMessage);
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Log or handle the exception
                    Console.WriteLine("GPT API Exception: " + ex.Message);
                    throw new HttpRequestException($"Error making GPT API request: {ex.Message}", ex);
                }
            }
        }

        public class MyResponseType
        {
            public List<Choice> choices { get; set; }
        }

        public class Choice
        {
            public string text { get; set; }
        }

        // Add a class to represent the error response from the API
        public class ErrorResponse
        {
            [JsonProperty("error")]
            public ErrorDetail Error { get; set; }
        }

        public class ErrorDetail
        {
            [JsonProperty("message")]
            public string Message { get; set; }
        }

        // Inside ManagerController.cs//ETI
        public async Task<IActionResult> PredictIceCreamConsumption()
        {
            // Retrieve the last 10 orders
            var orders = _context.Order.OrderByDescending(o => o.Date).Take(10).ToList();

            // Format orders data into a string
            var inputText = string.Join("\n\n", orders.Select(GetOrdersDataAsString));

            var flavors = string.Join("\n\n", _context.IceCream.Select(iceCream => iceCream.Name).ToList());

            // Create a specific prompt for GPT prediction
            var SpecifyCityHere = "Tel Aviv";
            Weather wez = await _apiServices.FindWeatherAsync(SpecifyCityHere);

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var SpecifyHumidityLevelHere = wez.Humidity.ToString();
            var SpecifyFeelsLikeTemperatureHere = wez.FeelsLike.ToString();
            var IsItHoliday = await _apiServices.IsItHoliday();


            var prompt = $@"
Ice Cream Consumption Prediction:

Recent Orders:
{inputText}

The flavors available in the store:
{flavors}

Contextual Factors:
- The flavors available in the store.
- Day of the Week: {(Models.DayOfWeek)DateTime.Now.DayOfWeek}
- Humidity Level: {SpecifyHumidityLevelHere}
- Feels Like Temperature: {SpecifyFeelsLikeTemperatureHere}
- City: {SpecifyCityHere}
- Is there a holiday this week: {IsItHoliday}

Guidance for Prediction:
Given the historical orders and the Factors, predict which ice cream flavor will consume the most on {date}. Consider factors like current weather, day of the week, and past preferences. Provide a detailed forecast with qualitative and quantitative insights. Mention any observed patterns or notable considerations.
Note: Return a short answer! and accurate, a maximum answer of up to 50 words!, focusing on relevant details and insights.";


            Console.WriteLine(prompt);
            // Call GPT API with the prompt and get the response
            var gptResponse = CallGPTAPI(prompt);

            // Process the GPT response and present the results to the manager

            // Render a view with the results or return a JSON response
            ViewBag.Place = "Prediction";
            return View("PredictIceCreamResults", gptResponse);
        }



        // GET: ManagerController
        public ActionResult Index()
        {
            ViewBag.Place = "Manager";
            return View();
        }

        // GET: ManagerController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ManagerController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ManagerController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ManagerController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ManagerController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ManagerController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ManagerController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
