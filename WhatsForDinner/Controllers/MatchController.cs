using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WhatsForDinner.Models;

namespace WhatsForDinner.Controllers
{
    public class MatchController : Controller
    {
        private readonly DinnerDbContext _context;

        private readonly string APIKEYVARIABLE;
        public MatchController(IConfiguration configuration, DinnerDbContext context)
        {
            _context = context;
            APIKEYVARIABLE = configuration.GetSection("APIKeys")["YelpAPI"];
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GroupRestLists(Guid gid)
        {
            var groupsUsers = from m in _context.UserGroups
                              from c in _context.AspNetUsers
                              where m.GroupId == gid && m.UserId == c.Id
                              select new AspNetUsers()
                              {
                                  Id = c.Id,
                                  UserName = c.UserName
                              };
            var newGroupsUsers = await groupsUsers.ToListAsync();
            List<List<Restaurants>> groupRests = new List<List<Restaurants>>();
            foreach (AspNetUsers member in newGroupsUsers)
            {
                var userRests = await _context.Restaurants.Where(x => x.UserId == member.Id && x.Liked == true).ToListAsync();
                groupRests.Add(userRests);
            }

            return RedirectToAction("WhatsForDinner", new { groupRests });

        }

        //public async Task<IActionResult> WhatsForDinner(List<List<Restaurants>> matchUp)
        //{
        //    foreach (List<Restaurants> usersList in Model)
        //    {
                
                   

        //    }
        //}
    }
}