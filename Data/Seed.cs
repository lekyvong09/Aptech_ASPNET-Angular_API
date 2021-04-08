using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static void SeedFantasy(DataContext context)
        {
            // Check if there is any data. If there is, skip adding.
            if (context.FantasiesDbSet.Any()) return;

            var fantasies = new List<Fantasy>
            {
                new Fantasy {
                    FantasyId = "romatic",
                    Description = "I love romatic"
                },
                new Fantasy {
                    FantasyId = "desire",
                    Description = "My dark desire is..."
                }
            };
            foreach (var fantasy in fantasies)
            {
                context.FantasiesDbSet.Add(fantasy);
            }
            context.SaveChanges();
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            // Check if there is any user data. If there is, skip adding users.
            if (await userManager.Users.AnyAsync()) return;

            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData2.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            if (users == null) return;

            var roles = new List<AppRole>
            {
                new AppRole { Name = "Member"},
                new AppRole { Name = "Admin"},
                new AppRole { Name = "Moderator"},
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users)
            {
                //using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
                //user.PasswordSalt = hmac.Key;

                //context.Users.Add(user);
                await userManager.CreateAsync(user, "password");
                await userManager.AddToRoleAsync(user, "Member");
            }

            //await context.SaveChangesAsync();

            var admin = new AppUser
            {
                UserName = "admin"
            };

            await userManager.CreateAsync(admin, "password");
            await userManager.AddToRoleAsync(admin, "Admin");
            await userManager.AddToRoleAsync(admin, "Moderator");
        }
    }
}
