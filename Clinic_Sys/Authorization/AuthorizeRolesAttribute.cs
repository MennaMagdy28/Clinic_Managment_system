using Clinic_Sys.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Clinic_Sys.Authorization
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params UserRole[] roles)
        {
            // Convert enum values to strings and join them with commas
            var roleNames = roles.Select(r => r.ToString()).ToList();
            Roles = string.Join(",", roleNames);
        }
    }
} 