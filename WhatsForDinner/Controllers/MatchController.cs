﻿using System;
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
                        where m.GroupId == gid
                        select new AspNetUsers()
                        {
                            Id = c.Id,
                            UserName = c.UserName
                        };
            List<List<Restaurants>> groupRests = new List<List<Restaurants>>();
            foreach(AspNetUsers member in groupsUsers)
            {
                var userRests = await _context.Restaurants.Where(x => x.UserId == member.Id).ToListAsync();
                groupRests.Add(userRests);
            }

            return View(groupRests);

        }
    }
}