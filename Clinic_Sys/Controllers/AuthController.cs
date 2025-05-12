using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clinic_Sys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Clinic_Sys.Authorization;
using Clinic_Sys.Enums;
using Microsoft.EntityFrameworkCore;
using Clinic_Sys.Data;
using Microsoft.AspNetCore.Authorization;

namespace Clinic_Sys.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<User> userManager, 
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Set role to Patient by default
            model.Role = UserRole.Patient;

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                Role = model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add user to Patient role
                await _userManager.AddToRoleAsync(user, UserRole.Patient.ToString());

                // Create associated Patient record
                var patient = new Patient
                {
                    Id = user.Id,
                    User = user,
                    ChronicDisease = model.ChronicDisease ?? "None",
                    BirthDate = DateTime.SpecifyKind(model.BirthDate, DateTimeKind.Utc),
                    Gender = model.Gender
                };

                _context.Patients.Add(patient);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Rollback user creation if patient creation fails
                    await _userManager.DeleteAsync(user);
                    return StatusCode(500, new { message = "Failed to create patient record. User creation rolled back.", error = ex.Message });
                }

                return Ok(new { message = "Patient registered successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("register-doctor")]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorModel model)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                Role = UserRole.Doctor
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add user to Doctor role
                await _userManager.AddToRoleAsync(user, UserRole.Doctor.ToString());

                // Create associated Doctor record
                var doctor = new Doctor
                {
                    Id = user.Id,
                    User = user,
                    Specialization = model.Specialization,
                    LicenseNumber = model.LicenseNumber,
                    RoomNumber = model.RoomNumber
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Doctor registered successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var result = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!result)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var token = await GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    role = user.Role
                }
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Since JWTs are stateless, there's nothing to do on the server.
            // The client should remove the token.
            return Ok(new { message = "Logged out successfully" });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Get user roles from Identity
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public string ChronicDisease { get; set; }
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
    }

    public class RegisterDoctorModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
        public string LicenseNumber { get; set; }
        public int RoomNumber { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
} 