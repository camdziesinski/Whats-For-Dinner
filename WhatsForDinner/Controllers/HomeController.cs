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
            //sets default to detroit if user hasn't sumbitted a zip code
            if (location == null)
            {
                location = "48226";
            }
            ViewBag.location = location;

            //API call
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.yelp.com/v3/businesses/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {YelpAPI}");
            var response = await client.GetAsync($"search?location={location}");
            var result = await response.Content.ReadAsAsync<YelpData>();

            string user = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            //Takes in API list and user list, removes the same restaurants
            var list1 = result.businesses.ToList();
            var list2 = _context.Restaurants.Where(x => x.UserId == user).ToList();
            var list3 = list1.Where(p => !list2.Any(x => x.PlaceId == p.id)).ToList();

            return View(list3);
        }



        public IActionResult GroupList(Guid id)
        {
            var users = _context.UserGroups.Where(x => x.GroupId == id).ToList();
            var restaurants = new List<Restaurants>();

            for (int i = 0; i < users.Count - 1; i++)
            {
                var user = users[i].UserId;
                var user2 = users[i + 1].UserId;

                var list2 = _context.Restaurants.Where(x => x.UserId == user2).ToList();
                if (i == 0)
                {
                    restaurants = _context.Restaurants.Where(x => x.UserId == user).ToList();
                }
                restaurants = restaurants.Where(p => list2.Any(x => x.PlaceId == p.PlaceId)).ToList();

            }
            
            return View(restaurants);
        }



        // Taking in phone number to draw data for the exact restaurant. user rating, note, and liked is to fill constructor.
        //rest is filled from the data pulled from the API call.
        public IActionResult AddRestaurant(string location, string id, string name, string zip, int userRating, string note, bool liked)
        {
            string user = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            Restaurants save = new Restaurants(user, id, name, userRating, note, zip, liked);

            //Exclude a restaurant that already exist in restaurants table
            for (int i = 0; i < _context.Restaurants.ToList().Count; i++)
            {
                if (_context.Restaurants.ToList()[i].PlaceId == id && _context.Restaurants.ToList()[i].UserId == user)
                {
                    return RedirectToAction("Discover", new { location });

                }
            }

            //Save those changes
            _context.Restaurants.Add(save);
            _context.SaveChanges();

            return RedirectToAction("Discover", new { location });
        }

        public IActionResult Favorites()
        {
            string user = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var list = _context.Restaurants.Where(x => x.UserId == user).ToList();
            var fav = _context.Restaurants.Where(x => x.Liked == true).ToList();
            return View(fav);
        }

        // Removes restaurant from your list
        public IActionResult RemoveRestaurant(int id)
        {
            Restaurants findFavorite = _context.Restaurants.Find(id);
            if (findFavorite != null)
            {
                _context.Restaurants.Remove(findFavorite);
                _context.SaveChanges();
            }

            return RedirectToAction("Favorites");
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