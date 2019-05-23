using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiz.Data;
using Qwiz.Models;
using Group = Qwiz.Models.Group;

namespace Qwiz.Controllers.Api
{
    [Route("api/group")]
    [ApiController]
    public class ApiGroupController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        
        public ApiGroupController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }

        [HttpGet("getList/{type}")]
        public async Task<IActionResult> GetGroupsList(string type, int? id, int page, int size, string orderBy, string username, string search)
        {
            if (page < 1 || size < 1 || type == null) return BadRequest("Invalid request!");
            
            switch (type)
            {
                case "all":
                {
                    var groups = await _db.Groups.Include(g => g.Members).ToListAsync();

                    if (orderBy == "members") groups = groups.OrderByDescending(g => g.Members.Count).ToList();
                    if (orderBy == "created") groups = groups.OrderByDescending(g => g.CreationDate).ToList();

                    if (search != null)
                    {
                        var searchArr        = Regex.Split(search.ToLower(), @"\s+").Where(s => s != string.Empty);
                        var queryName   = groups.Where(q => searchArr.Any(q.Name.ToLower().Contains)).ToList();
                        var queryOwner  = groups.Where(q => searchArr.Any(q.OwnerUsername.ToLower().Contains)).ToList();
                        var queryRegion = groups.Where(q => searchArr.Any(q.Region.ToLower().Contains)).ToList();

                        var searchList = new List<Group>();
                        searchList.AddRange(queryName);
                        searchList.AddRange(queryOwner);
                        searchList.AddRange(queryRegion);
                        groups = searchList.Distinct().ToList();
                    }

                    var query = groups.Select(g => new
                    {
                        g.Id,
                        g.Name,
                        g.Region,
                        g.IsPublic,
                        g.OwnerUsername,
                        members = g.Members.Count,
                        creationDate = g.CreationDate.ToString("dd/MM/yyyy HH:mm")
                    }).ToList();
                    
                    var entries = query.Skip((page - 1) * size).Take(size).ToList();
                    var pages   = (int) Math.Ceiling(decimal.Divide(query.Count, size));
    
                    return Ok(new {entries, pages});
                }
                case "myGroups":
                {
                    var user = await _um.Users
                        .Where(u => u.UserName == username)
                        .Include(u => u.MyGroups)
                        .ThenInclude(g => g.Members).FirstOrDefaultAsync();
                    if (user == null) return BadRequest("You need to be logged in!");
                    
                    var query = user.MyGroups
                        .Select(g => new
                        {
                            g.Id,
                            g.Name,
                            g.Region,
                            g.IsPublic,
                            g.OwnerUsername,
                            members = g.Members.Count,
                            creationDate = g.CreationDate.ToString("dd/MM/yyyy HH:mm")
                        }).ToList();

                    var entries = query.Skip((page - 1) * size).Take(size).ToList();
                    var pages   = (int) Math.Ceiling(decimal.Divide(query.Count, size));
    
                    return Ok(new {entries, pages});
                }
                case "members" when id != null && id > 0:
                {
                    var group = await _db.Groups
                        .Where(m => m.Id == id)
                        .Include(g => g.Members)
                        .ThenInclude(m => m.Member)
                        .FirstOrDefaultAsync();
                    if (group == null) return BadRequest("Could not find group!");

                    var isMember = group.Members.Exists(m => m.Username == _um.GetUserName(User));
                    if (!group.IsPublic && !isMember) return BadRequest("Not authorized!");
                    
                    var query = group.Members
                        .Select(m => new
                        {
                            m.Role,
                            m.Username,
                            m.RoleText,
                            m.Member.Level,
                            joinDate = m.JoinDate.ToString("dd/MM/yyyy HH:mm")
                        }).ToList();
                    
                    if      (orderBy == "role")   query = query.OrderBy(m => m.Role).ToList();
                    else if (orderBy == "level")  query = query.OrderByDescending(m => m.Level).ToList();
                    else if (orderBy == "joined") query = query.OrderByDescending(m => m.joinDate).ToList();
                    
                    var entries = query.Skip((page - 1) * size).Take(size).ToList();
                    var pages   = (int) Math.Ceiling(decimal.Divide(query.Count, size));
    
                    return Ok(new {entries, pages});
                }

                case "pending" when id != null && id > 0:
                {
                    var authorizedUsername = _um.GetUserName(User);
                    if (authorizedUsername == null) return BadRequest("You need to be logged in!");

                    var query = await _db.Groups
                        .Where(g => g.Id == id)
                        .Include(g => g.Members)
                        .Include(g => g.PendingInvites).FirstOrDefaultAsync();
                    if (query == null) return BadRequest("Could not find group!");

                    var member = query.Members.Find(m => m.Username == authorizedUsername);
                    if (member == null) return BadRequest("You are not a member!");
                    if (member.Role == 2) return BadRequest("Not authorized!");
                
                    var entries = query.PendingInvites.Skip((page - 1) * size).Take(size).ToList();
                    var pages   = (int) Math.Ceiling(decimal.Divide(query.PendingInvites.Count, size));
                    
                    return Ok(new {entries, pages});
                }
                
                default:
                    return BadRequest("Id not present or type not found!");
            }
        }

        [HttpPut("request")]
        [Authorize]
        public async Task<IActionResult> RequestGroup(string type, int? id, string username, int? role)
        {
            if (id == null) return BadRequest("Invalid group ID!");
            
            switch (type)
            {
                case "leave":
                {
                    var group = await _db.Groups
                        .Where(g => g.Id == id)
                        .Include(g => g.Members)
                        .FirstOrDefaultAsync();
                    if (group == null) return BadRequest("Group not found!");
                
                    var authorizedUsername = _um.GetUserName(User);
                    if (group.OwnerUsername == authorizedUsername) return BadRequest("Can't leave group as the owner!");
                    
                    var member = group.Members.Find(p => p.Username == authorizedUsername);
                    if (member == null) return BadRequest("You are not a member of this group!");
                    
                    group.Members.Remove(member);
                    await _db.SaveChangesAsync();
                    
                    return Ok();

                }
                case "join":
                {
                    var group = await _db.Groups
                        .Where(g => g.Id == id)
                        .Include(g => g.PendingInvites)
                        .FirstOrDefaultAsync();
                    if (group == null) return BadRequest("Group not found!");
                
                    var user = await _um.GetUserAsync(User);
                    if (group.PendingInvites.Exists(p => p.Username == user.UserName))
                        return BadRequest("Already pending request to join group!");
                    
                    group.PendingInvites.Add(new PendingMember(user));
                    await _db.SaveChangesAsync();
                    
                    return Ok();
                }

                case "accept":
                case "deny" when username != null:
                {
                    var group = await _db.Groups
                        .Where(g => g.Id == id)
                        .Include(g => g.Members)
                        .Include(g => g.PendingInvites)
                        .ThenInclude(g => g.User)
                        .FirstOrDefaultAsync();
                    if (group == null) return BadRequest("Group not found!");

                    var canAccept = group.Members
                        .Exists(m => m.Username == _um.GetUserName(User) && m.Role < 2);
                    if (!canAccept) return BadRequest("Not authorized to add members!");
                
                    var pendingMember = group.PendingInvites.Find(p => p.Username == username);
                    if (pendingMember == null) return BadRequest("User not found!");
                
                    group.PendingInvites.Remove(pendingMember);

                    if (type == "accept")
                    {
                        group.Members.Add(new GroupMember(2, pendingMember.User, group));
                    }
                
                    await _db.SaveChangesAsync();

                    return Ok();
                }

                case "change":
                case "remove" when username != null:
                {
                    var group = await _db.Groups
                        .Where(g => g.Id == id)
                        .Include(g => g.Members)
                        .FirstOrDefaultAsync();
                    if (group == null) return BadRequest("Group not found!");
                    if (group.OwnerUsername == username) BadRequest("Cannot do that to owner!");
                
                    var authorizedMember = group.Members.Find(m => m.Username == _um.GetUserName(User));
                    if (authorizedMember == null) return BadRequest("You are not a member!");
                    if (authorizedMember.Username == username) return BadRequest("Cannot do that to owner!");
                
                    var member = group.Members.Find(p => p.Username == username);
                    if (member == null) return BadRequest("User not found!");

                    if (type == "remove") {
                        if (authorizedMember.Role == 0) group.Members.Remove(member);
                        else if (authorizedMember.Role == 1 && member.Role == 2) group.Members.Remove(member);
                        else return BadRequest("You are not authorized to do that!");
                    }
                    else if (type == "leave")
                    {
                        
                    }
                    else if (type == "change")
                    {
                        if (role == null) return BadRequest("Role not present!");
                        
                        if (authorizedMember.Role == 0 && group.OwnerUsername != username)
                        {
                            if (role == 1)
                            {
                                member.Role = 1;
                                member.RoleText = "Admin";
                            }
                            else
                            {
                                member.Role = 2;
                                member.RoleText = "Member";
                            }
                        }
                        else
                        {
                            return BadRequest("Not authorized!");
                        }
                    }

                    await _db.SaveChangesAsync();
                    return Ok();
                }

                default:
                    return BadRequest("Type not found!");
            }
        }

        [HttpDelete("remove")]
        [Authorize]
        public async Task<IActionResult> RemoveGroup(int? id)
        {
            if (id == null) return BadRequest("Id not present!");
            
            var group = await _db.Groups
                .Where(m => m.Id == id)
                .Include(g => g.PendingInvites)
                .Include(g => g.Members)
                .ThenInclude(m => m.Member)
                .FirstOrDefaultAsync();
            if (group == null) return BadRequest("Could not find group!");

            var username = _um.GetUserName(User);
            if (group.OwnerUsername != username) return BadRequest("You need to be owner to do that!");

            foreach (var member in group.Members)
                member.Member.MyGroups.Remove(group);
            
            group.PendingInvites.Clear();
            _db.GroupMembers.RemoveRange(group.Members);
            _db.Groups.Remove(group);
            
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}