using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Programmin2_classroom.GoogleAuth.Data.Entities;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Programmin2_classroom.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Programmin2_classroom.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignInGoogleController : ControllerBase
    {
        [Route("signin-google")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return BadRequest(); // Handle error response

            var claims = authenticateResult.Principal.Identities.FirstOrDefault().Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Add to database if not exists
            AddUserToDatabase(email, name);

            return LocalRedirect("/");
        }

        private readonly ApplicationDbContext _context;
        public SignInGoogleController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void AddUserToDatabase(string email, string name)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmailAddress == email);
            if (user == null)
            {
                user = new User { EmailAddress = email, FirstName = name };
                _context.Users.Add(user);
            }
            else
            {
                user.FirstName = name;
            }
            _context.SaveChanges();
        }

        [HttpPost("signin-google")]
        public async Task<IActionResult> SignInWithGoogle(User userProfile)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == userProfile.EmailAddress);
            if (user == null)
            {
                user = new User { EmailAddress = userProfile.EmailAddress, FirstName = userProfile.FirstName };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok("User created");
            }
            else
            {
                user.FirstName = userProfile.FirstName; // Update any details that might have changed
                await _context.SaveChangesAsync();
                return Ok("User updated");
            }
        }

    }
}
