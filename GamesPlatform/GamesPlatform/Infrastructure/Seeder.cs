using GamesPlatform.Data;
using GamesPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;

namespace GamesPlatform.Infrastructure
{
    public class Seeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (dbContext.Database.GetPendingMigrations().Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string adminEmail = "admin@example.com";
            string adminPassword = "Admin123!";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            string userEmail = "user@example.com";
            string userPassword = "User123!";
            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = "user",
                    Email = userEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, userPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "User");
            }

            if(!dbContext.Platforms.Any())
            {
                var platform = new Platform
                {
                    Name = "PC"
                };
                await dbContext.Platforms.AddAsync(platform);
                await dbContext.SaveChangesAsync();
            }
            var firstPlatform = await dbContext.Platforms.ToListAsync();

            if (!dbContext.Games.Any())
            {
                var game = new Game
                {
                    Title = "The Witcher 3: Wild Hunt",
                    Tag = Enums.Tags.RPG,
                    ReleaseYear = new DateTime(2015, 5, 18, 0, 0, 0, DateTimeKind.Utc),
                    Developer = "CD Projekt Red",
                    Publisher = "CD Projekt",
                    Platforms = firstPlatform,
                    Description = "Kultowa gra RPG o przygodach Geralta z Rivii.",
                    ImageUrl = "",
                    ThumbNailUrl = ""
                };
                await dbContext.Games.AddAsync(game);
                await dbContext.SaveChangesAsync();
            }

            var firstGame = await dbContext.Games.FirstAsync();

            if (!dbContext.Ratings.Any())
            {
                var rating = new Rating
                {
                    UserId = user.Id,
                    GameId = firstGame.GameId,
                    Gameplay = 9,
                    Graphics = 10,
                    Optimization = 8,
                    Story = 9
                };
                await dbContext.Ratings.AddAsync(rating);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.Reviews.Any())
            {
                var review = new Review
                {
                    UserId = user.Id,
                    GameId = firstGame.GameId,
                    Description = "Świetna gra z niesamowitym klimatem!"
                };
                await dbContext.Reviews.AddAsync(review);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.Libraries.Any())
            {
                var library = new GamesPlatform.Models.Library
                {
                    UserId = user.Id,
                    GameId = firstGame.GameId,
                    Status = Enums.Status.Completed,
                    AddedAt = DateTime.UtcNow
                };
                await dbContext.Libraries.AddAsync(library);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
