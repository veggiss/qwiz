using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Qwiz.Data;
using Qwiz.Models;

namespace Qwiz.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class ApiUserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        private readonly IHttpContextAccessor _accessor;
        
        public ApiUserController(ApplicationDbContext db, UserManager<ApplicationUser> um, IHttpContextAccessor accessor)
        {
            _db = db;
            _um = um;
            _accessor = accessor;
        }
        
        // TODO: Change to a websocket solution?
        [HttpPut("wakeUp")]
        [Authorize]
        public async void SetLastActivity()
        {
            var user = await _um.GetUserAsync(User);
            
            Console.WriteLine("----------------------0");

            if (user != null)
            {
                Console.WriteLine("----------------------1");
                if (user.LastActivity < DateTime.Now)
                {
                    Console.WriteLine("----------------------2");
                    user.LastActivity = DateTime.Now.AddMinutes(5);
                    await _um.UpdateAsync(user);
                }
            }
        }
    
        [HttpPut("updateAvatar")]
        [Authorize]
        public async void UpdateAvatar(string path)
        {
            var user = await _um.GetUserAsync(User);
            user.ImagePath = path;
            await _um.UpdateAsync(user);
        }
        
        [HttpPost("uploadImage")]
        [Authorize]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0) return BadRequest("File not selected");
            if (!image.ContentType.Contains("image")) return BadRequest("File type not supported");
            if (image.Length > 1024 * 1024) return BadRequest("Files bigger than 1MB not allowed");
            
            var filename = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", filename);

            var username = (await _um.GetUserAsync(User)).UserName;
            var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            if (username == null) return BadRequest("You seem to not be logged in!");
            if (ip == null) return BadRequest("Cannot upload that image!");
            
            await _db.Images.AddAsync(new Image(filename, ip, username));
            await _db.SaveChangesAsync();

            using (var stream = new FileStream(path, FileMode.Create))
                await image.CopyToAsync(stream);

            return Ok("/images/" + filename);
        }
    }
}