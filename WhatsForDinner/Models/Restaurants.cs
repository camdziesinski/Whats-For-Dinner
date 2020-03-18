using System;
using System.Collections.Generic;

namespace WhatsForDinner.Models
{
    public partial class Restaurants
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PlaceId { get; set; }
        public string Name { get; set; }
        public int? UserRating { get; set; }
        public string Notes { get; set; }
        public string ZipCode { get; set; }
        public bool Liked { get; set; }

        public virtual AspNetUsers User { get; set; }
        public Restaurants() { }
        public Restaurants(string userId, string placeId, string name, int userRating, string notes, string zipCode, bool liked)
        {
            UserId = userId;
            PlaceId = placeId;
            Name = name;
            UserRating = userRating;
            Notes = notes;
            ZipCode = zipCode;
            Liked = liked;
        }
    }
}
