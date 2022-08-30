using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eTickets.Data.Static
{
    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string User = "user";
    }
}
