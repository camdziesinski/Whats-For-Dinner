using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WhatsForDinner.Models;
using System.Security.Claims;
using System.Net.Http.Headers;

namespace WhatsForDinner.Controllers
{
    [Authorize]

    public class HomeController : Controller
    {
        private readonly DinnerDbContext _context;

        private readonly string YelpAPI;



        public HomeController(IConfiguration configuration, DinnerDbContext context)
        {
            _context = context;
            YelpAPI = configuration.GetSection("APIKeys")["YelpAPI"];
        }

        public IActionResult Index()
        {
            return View();
        }

        //Takes in the location and fills in the field to make API call
        public async Task<IActionResult> Discover(string location)
        {
            ViewBag.location = location;

            if (location == null)
            {
                location = "48226";
            }

            //API call
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.yelp.com/v3/businesses/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {YelpAPI}");
            var response = await client.GetAsync($"search?location={location}");
            var result = await response.Content.ReadAsAsync<YelpData>();

            //Takes in API list and user list, removes the same restaurants
            var list1 = result.businesses.ToList();
            var list2 = _context.Restaurants.ToList();
            var list3 = list1.Where(p => !list2.Any(x => x.PlaceId == p.id)).ToList();

            return View(list3);
        }



        // Taking in phone number to draw data for the exact restaurant. user rating, note, and liked is to fill constructor.
        //rest is filled from the data pulled from the API call.
        public async Task<IActionResult> AddRestaurant(string phone, int userRating, string note, bool liked)
        {
            //API call
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.yelp.com/v3/businesses/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {YelpAPI}");
            var response = await client.GetAsync($"phone?phone={phone}");
            var result = await response.Content.ReadAsAsync<Business>();

            //Makes the new restaurant to store into Db
            Restaurants save = new Restaurants(User.FindFirst(ClaimTypes.NameIdentifier).Value, result.id, result.name, userRating, note, result.location.zip_code, liked);

            //Exclude a restaurant that already exist in restaurants table
            for (int i = 0; i < _context.Restaurants.ToList().Count; i++)
            {
                if (_context.Restaurants.ToList()[i].PlaceId == result.id)
                {
                    return RedirectToAction("Discover");

                }
            }

            //Save those changes
            _context.Restaurants.Add(save);
            _context.SaveChanges();

            return View("Discover");
        }

        #region PrivacyErrorActions
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}