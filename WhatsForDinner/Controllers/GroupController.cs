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

        public IActionResult GroupMembers(Guid id)
        {
            TempData["group"] = _context.Groups.Find(id).Name;
            
            var members = from u in _context.AspNetUsers
                          from g in _context.UserGroups
                          where g.GroupId == id && g.UserId == u.Id
                          select new AspNetUsers()
                          {
                              Id = u.Id,
                              UserName = u.UserName,
                              Email = u.Email
                          };

            return View(members.ToList());
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
            var validemail = validUser(email);


            if (validemail == true)
            {
                AspNetUsers found = _context.AspNetUsers.Where(x => x.Email == email).First();
                bool member = inGroup(found.Id, (Guid)TempData["groupId"]);
                if(member == false)
                {
                    GroupInvite newinvite = new GroupInvite();
                    var tempUser = await _context.AspNetUsers.Where(x => x.Email == email).FirstAsync();
                    newinvite.UserId = tempUser.Id;
                    newinvite.GroupId = (Guid)TempData["groupId"];
                    await _context.GroupInvite.AddAsync(newinvite);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ListGroups");
                }
                else
                {
                    TempData["inGroup"] = true;
                    return RedirectToAction("InviteToGroup", TempData["groupId"]);
                }
            }
            else
            {
                TempData["exists"] = true;
                return RedirectToAction("InviteToGroup", TempData["groupId"]);
            }

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
            // using group id from invite, creates UserGroups entry, adding user to group
            string uid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserGroups newUG = new UserGroups();
            newUG.GroupId = id;
            newUG.UserId = uid;
            await _context.UserGroups.AddAsync(newUG);
            await _context.SaveChangesAsync();

            return RedirectToAction("DeleteInvite", new { newUG.GroupId });
        }

        public async Task<IActionResult> DeleteInvite(Guid groupId)
        {
            // deletes entry from GroupInvites table
            string uid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var tempInvite = await _context.GroupInvite.Where(x => x.GroupId == groupId && x.UserId == uid).ToListAsync();
            _context.Remove(tempInvite[0]);
            await _context.SaveChangesAsync();
            return RedirectToAction("ListGroups");
        }

        public async Task<IActionResult> LeaveGroup(Guid id)
        {
            string uid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            //get entry in UserGroups table by comparing to users id and passed goup id
            var userGroup = from m in _context.UserGroups
                            where m.GroupId == id && m.UserId == uid
                            select new UserGroups()
                            {
                                Id = m.Id,
                                UserId = m.UserId,
                                GroupId = m.GroupId,
                            };
            List<UserGroups> newlist = userGroup.ToList();
            if (newlist[0] != null)
            {
                _context.Remove(newlist[0]);
                await _context.SaveChangesAsync();
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

            await _context.SaveChangesAsync();

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

        public bool inGroup(string uId, Guid gId)
        {
            var member = _context.UserGroups.Where(x => x.GroupId == gId).Where(y => y.UserId == uId).ToList();
            if(member.Count() != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool validUser(string email)
        {
            var usr = _context.AspNetUsers.Where(x => x.Email == email).ToList();
            if(usr.Count() != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}