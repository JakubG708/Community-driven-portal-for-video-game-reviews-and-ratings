using GamesPlatform.Data;
using GamesPlatform.DTOs;
using GamesPlatform.Enums;
using GamesPlatform.Models;
using GamesPlatform.Services.Games;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Games_Platform_Tests
{
    public class GamesServiceTests
    {
        private IDbContextFactory<ApplicationDbContext> CreateDbContextFactory()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(options);
            return factory;
        }

        [Fact]
        public async Task AddGameAsync_ShouldAddGameToDatabase()
        {
            var factory = CreateDbContextFactory();
            var service = new GamesService(factory);
            var gameDTO = new GameDTO
            {
                Title = "Test Game",
                Tag = Tags.Action,
                ReleaseYear = new DateTime(2023, 1, 1),
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Description = "Test Description",
                ImageUrl = "https://test.com/image.jpg",
                ThumbNailUrl = "https://test.com/thumb.jpg",
                Platforms = new List<Platforms> { Platforms.PC, Platforms.PlayStation }
            };

            await service.AddGameAsync(gameDTO);

            using var db = await factory.CreateDbContextAsync();
            var game = await db.Games.FirstOrDefaultAsync(g => g.Title == "Test Game");
            Assert.NotNull(game);
            Assert.Equal("Test Game", game.Title);
            Assert.Equal(Tags.Action, game.Tag);
            Assert.Equal("Test Developer", game.Developer);
            Assert.Equal(2, game.Platforms.Count);
        }

        [Fact]
        public async Task GetGamesAsync_ShouldReturnAllGames()
        {
            var factory = CreateDbContextFactory();
            using (var db = await factory.CreateDbContextAsync())
            {
                db.Games.AddRange(
                    new Game { Title = "Game 1", Tag = Tags.Action, Developer = "Dev1", Publisher = "Pub1", Description = "Desc1", ReleaseYear = DateTime.UtcNow },
                    new Game { Title = "Game 2", Tag = Tags.RPG, Developer = "Dev2", Publisher = "Pub2", Description = "Desc2", ReleaseYear = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
            }
            var service = new GamesService(factory);

            var games = await service.GetGamesAsync();

            Assert.NotNull(games);
            Assert.Equal(2, games.Count);
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldReturnGameWithCorrectData()
        {
            var factory = CreateDbContextFactory();
            int gameId;
            using (var db = await factory.CreateDbContextAsync())
            {
                var game = new Game
                {
                    Title = "Test Game",
                    Tag = Tags.Adventure,
                    Developer = "Test Dev",
                    Publisher = "Test Pub",
                    Description = "Test Desc",
                    ReleaseYear = DateTime.UtcNow,
                    Platforms = new List<Platforms> { Platforms.Xbox }
                };
                db.Games.Add(game);
                await db.SaveChangesAsync();
                gameId = game.GameId;
            }
            var service = new GamesService(factory);

            var result = await service.GetGameByIdAsync(gameId);

            Assert.NotNull(result);
            Assert.Equal("Test Game", result.Title);
            Assert.Equal(Tags.Adventure, result.Tag);
            Assert.Equal("Test Dev", result.Developer);
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldCalculateAverageRatings()
        {
            var factory = CreateDbContextFactory();
            int gameId;
            using (var db = await factory.CreateDbContextAsync())
            {
                var game = new Game
                {
                    Title = "Test Game",
                    Tag = Tags.Action,
                    Developer = "Dev",
                    Publisher = "Pub",
                    Description = "Desc",
                    ReleaseYear = DateTime.UtcNow
                };
                db.Games.Add(game);
                await db.SaveChangesAsync();
                gameId = game.GameId;

                db.Ratings.AddRange(
                    new Rating { GameId = gameId, UserId = "user1", Gameplay = 8, Graphics = 9, Optimization = 7, Story = 10 },
                    new Rating { GameId = gameId, UserId = "user2", Gameplay = 6, Graphics = 7, Optimization = 9, Story = 8 }
                );
                await db.SaveChangesAsync();
            }
            var service = new GamesService(factory);

            var result = await service.GetGameByIdAsync(gameId);

            Assert.Equal(7, result.AvgGameplayRating);
            Assert.Equal(8, result.AvgGraphicsRating); 
            Assert.Equal(8, result.AvgOptimizationRating);
            Assert.Equal(9, result.AvgStoryRating);
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldReturnZeroRatings_WhenNoRatingsExist()
        {
           
            var factory = CreateDbContextFactory();
            int gameId;
            using (var db = await factory.CreateDbContextAsync())
            {
                var game = new Game
                {
                    Title = "Test Game",
                    Tag = Tags.Action,
                    Developer = "Dev",
                    Publisher = "Pub",
                    Description = "Desc",
                    ReleaseYear = DateTime.UtcNow
                };
                db.Games.Add(game);
                await db.SaveChangesAsync();
                gameId = game.GameId;
            }
            var service = new GamesService(factory);

            var result = await service.GetGameByIdAsync(gameId);

            Assert.Equal(0, result.AvgGameplayRating);
            Assert.Equal(0, result.AvgGraphicsRating);
            Assert.Equal(0, result.AvgOptimizationRating);
            Assert.Equal(0, result.AvgStoryRating);
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldThrowException_WhenGameNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new GamesService(factory);

            await Assert.ThrowsAsync<Exception>(async () => await service.GetGameByIdAsync(999));
        }

        [Fact]
        public async Task EditGameAsync_ShouldUpdateGameProperties()
        {
            var factory = CreateDbContextFactory();
            int gameId;
            using (var db = await factory.CreateDbContextAsync())
            {
                var game = new Game
                {
                    Title = "Original Title",
                    Tag = Tags.Action,
                    Developer = "Original Dev",
                    Publisher = "Original Pub",
                    Description = "Original Desc",
                    ReleaseYear = DateTime.UtcNow,
                    Platforms = new List<Platforms> { Platforms.PC }
                };
                db.Games.Add(game);
                await db.SaveChangesAsync();
                gameId = game.GameId;
            }
            var service = new GamesService(factory);
            var editDTO = new EditGameDTO
            {
                Title = "Updated Title",
                Tag = Tags.RPG,
                Developer = "Updated Dev",
                Publisher = "Updated Pub",
                Description = "Updated Desc",
                ReleaseYear = new DateTime(2024, 1, 1),
                ImageUrl = "https://updated.com/image.jpg",
                ThumbNailUrl = "https://updated.com/thumb.jpg",
                Platforms = new List<Platforms> { Platforms.PlayStation, Platforms.Xbox }
            };

            await service.EditGameAsync(gameId, editDTO);

            using (var db = await factory.CreateDbContextAsync())
            {
                var updatedGame = await db.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
                Assert.NotNull(updatedGame);
                Assert.Equal("Updated Title", updatedGame.Title);
                Assert.Equal(Tags.RPG, updatedGame.Tag);
                Assert.Equal("Updated Dev", updatedGame.Developer);
                Assert.Equal(2, updatedGame.Platforms.Count);
            }
        }

        [Fact]
        public async Task EditGameAsync_ShouldThrowException_WhenGameNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new GamesService(factory);
            var editDTO = new EditGameDTO
            {
                Title = "Test",
                Tag = Tags.Action,
                Developer = "Dev",
                Publisher = "Pub",
                Description = "Desc",
                ReleaseYear = DateTime.UtcNow,
                Platforms = new List<Platforms> { Platforms.PC }
            };

            await Assert.ThrowsAsync<Exception>(async () => await service.EditGameAsync(999, editDTO));
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldIncludeReviewsAndRatings()
        {
            var factory = CreateDbContextFactory();
            int gameId;
            using (var db = await factory.CreateDbContextAsync())
            {
                var game = new Game
                {
                    Title = "Test Game",
                    Tag = Tags.Action,
                    Developer = "Dev",
                    Publisher = "Pub",
                    Description = "Desc",
                    ReleaseYear = DateTime.UtcNow
                };
                db.Games.Add(game);
                await db.SaveChangesAsync();
                gameId = game.GameId;

                var user = new IdentityUser
                {
                    UserName = "user1"
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                var userId = user.Id;


                db.Reviews.Add(new Review { GameId = gameId, UserId = userId, Description = "Great game!" });
                await db.SaveChangesAsync();

                var review =db.Reviews.First();
            }
            var service = new GamesService(factory);

            

            var result = await service.GetGameByIdAsync(gameId);

            Assert.NotNull(result.Reviews);
            Assert.Single(result.Reviews);
            Assert.Equal("Great game!", result.Reviews.First().Description);
        }
    }

    public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
        {
            _options = options;
        }

        public ApplicationDbContext CreateDbContext()
        {
            return new ApplicationDbContext(_options);
        }

        public async Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new ApplicationDbContext(_options));
        }
    }
}