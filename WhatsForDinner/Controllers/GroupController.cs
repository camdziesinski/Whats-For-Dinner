using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WhatsForDinner.Models;


namespace WhatsForDinner.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly DinnerDbContext _context;

        private readonly string APIKEYVARIABLE;
        public GroupController(IConfiguration configuration, DinnerDbContext context)
        {
            _context = context;
            APIKEYVARIABLE = configuration.GetSection("APIKeys")["YelpAPI"];
        }


        public IActionResult NewGroup()
        {
            return View();
        }

        [HttpGet]
        public IActionResult InviteToGroup(Guid id)
        {
            TempData["groupId"] = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InviteToGroup(string email)
        {
            GroupInvite newinvite = new GroupInvite();
            List<AspNetUsers> tempUser = await _context.AspNetUsers.Where(x => x.Email == email).ToListAsync();
            newinvite.UserId = tempUser[0].Id;
            newinvite.GroupId = (Guid)TempData["groupId"];
            await _context.GroupInvite.AddAsync(newinvite);
            await _context.SaveChangesAsync();
            return RedirectToAction("ListGroups");
        }

        public IActionResult ListInvites()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var query = from m in _context.GroupInvite
                            from c in _context.Groups
                            where m.GroupId == c.Id && m.UserId == id
                            select new Groups()
                            {
                                Id = c.Id,
                                Name = c.Name,
                                Type = c.Type,
                            };

            return View(query.ToList());
        }


        public async Task<IActionResult> JoinGroup(Guid id)
        {
            string uid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserGroups newUG = new UserGroups();
            newUG.GroupId = id;
            newUG.UserId = uid;
            await _context.UserGroups.AddAsync(newUG);
            _context.SaveChanges();

            return RedirectToAction("DeleteInvite", new { newUG.GroupId });
        }

        public async Task<IActionResult> DeleteInvite(Guid groupId)
        {
            string uid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var tempInvite = await _context.GroupInvite.Where(x => x.GroupId == groupId && x.UserId == uid).ToListAsync();
            _context.Remove(tempInvite[0]);
            _context.SaveChanges();
            return RedirectToAction("ListGroups");
        }

        public async Task<IActionResult> LeaveGroup(Guid id)
        {
            string uid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var tg = from m in _context.UserGroups
                     where m.GroupId == id && m.UserId == uid
                     select new UserGroups()
                     {
                         Id = m.Id,
                         UserId = m.UserId,
                         GroupId = m.GroupId,
                     };
            List<UserGroups> newlist = tg.ToList();
            if (newlist[0] != null)
            {
                _context.Remove(newlist[0]);
                _context.SaveChanges();
            }
            return RedirectToAction("ListGroups");
        }
        public async Task<IActionResult> CreateGroup(Groups newgroup)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            Guid gid = Guid.NewGuid();
            newgroup.Id = gid;

            UserGroups newUG = new UserGroups();
            newUG.GroupId = gid;
            newUG.UserId = id;

            await _context.UserGroups.AddAsync(newUG);
            await _context.Groups.AddAsync(newgroup);

            _context.SaveChanges();

            return RedirectToAction("ListGroups");
        }

        //public async Task<IActionResult> ListGroups()
        //{
        //    string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    List<UserGroups> usersgroups = await _context.UserGroups.Where(x => x.UserId == id).ToListAsync();
        //    List<Groups> groups = new List<Groups>();
        //    foreach (UserGroups group in usersgroups)
        //    {
        //        groups.Add((Groups)_context.Groups.Where(x => x.Id == group.GroupId));
        //    }

        //    return View(groups);
        //}

        public IActionResult ListGroups()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var query = from m in _context.UserGroups
                        from c in _context.Groups
                        where m.GroupId == c.Id && m.UserId == id
                        select new Groups()
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Type = c.Type,
                        };

            return View(query.ToList());
        }
    }
}