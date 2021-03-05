using System;
using System.Collections.Generic;
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
        public static async Task SeedUsers(UserManager<AppUser> userManager)
        {
            // Check if there is any user data. If there is, skip adding users.
            if (await userManager.Users.AnyAsync()) return;

            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData2.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            if (users == null) return;

            foreach (var user in users)
            {
                //using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
                //user.PasswordSalt = hmac.Key;

                //context.Users.Add(user);
                await userManager.CreateAsync(user, "password");
            }

            //await context.SaveChangesAsync();
        }
    }
}
