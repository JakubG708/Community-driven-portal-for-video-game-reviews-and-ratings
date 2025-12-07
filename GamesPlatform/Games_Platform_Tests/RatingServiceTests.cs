using GamesPlatform.Data;
using GamesPlatform.DTOs;
using GamesPlatform.Enums;
using GamesPlatform.Models;
using GamesPlatform.Services.Ratings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Games_Platform_Tests
{
    public class RatingServiceTests
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
        public async Task AddRatingAsync_ShouldAddRatingSuccessfully()
        {

            var factory = CreateDbContextFactory();
            int gameId;
            string userId;

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

                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);

                await db.SaveChangesAsync();
                gameId = game.GameId;
                userId = user.Id;
            }

            var service = new RatingService(factory);
            var ratingDTO = new RatingDTO
            {
                Gameplay = 8,
                Graphics = 9,
                Optimization = 7,
                Story = 10
            };

            
            await service.AddRatingAsync(gameId, userId, ratingDTO);

            using (var db = await factory.CreateDbContextAsync())
            {
                var rating = await db.Ratings.FirstOrDefaultAsync(r => r.GameId == gameId && r.UserId == userId);
                Assert.NotNull(rating);
                Assert.Equal(8, rating.Gameplay);
                Assert.Equal(9, rating.Graphics);
                Assert.Equal(7, rating.Optimization);
                Assert.Equal(10, rating.Story);
            }
        }

        [Fact]
        public async Task AddRatingAsync_ShouldThrowException_WhenGameNotFound()
        {
            var factory = CreateDbContextFactory();
            string userId;

            using (var db = await factory.CreateDbContextAsync())
            {
                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                userId = user.Id;
            }

            var service = new RatingService(factory);
            var ratingDTO = new RatingDTO
            {
                Gameplay = 8,
                Graphics = 9,
                Optimization = 7,
                Story = 10
            };

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.AddRatingAsync(999, userId, ratingDTO)
            );
            Assert.Equal("Game not found", exception.Message);
        }

        [Fact]
        public async Task AddRatingAsync_ShouldThrowException_WhenUserNotFound()
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

            var service = new RatingService(factory);
            var ratingDTO = new RatingDTO
            {
                Gameplay = 8,
                Graphics = 9,
                Optimization = 7,
                Story = 10
            };

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.AddRatingAsync(gameId, "nonexistentUser", ratingDTO)
            );
            Assert.Equal("User not found", exception.Message);
        }

        [Fact]
        public async Task EditRatingAsync_ShouldUpdateRatingSuccessfully()
        {
            var factory = CreateDbContextFactory();
            int ratingId;

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

                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                var rating = new Rating
                {
                    GameId = game.GameId,
                    UserId = user.Id,
                    Gameplay = 5,
                    Graphics = 5,
                    Optimization = 5,
                    Story = 5
                };
                db.Ratings.Add(rating);
                await db.SaveChangesAsync();
                ratingId = rating.RatingId;
            }

            var service = new RatingService(factory);
            var updatedRatingDTO = new RatingDTO
            {
                Gameplay = 9,
                Graphics = 8,
                Optimization = 10,
                Story = 7
            };

            await service.EditRatingAsync(ratingId, updatedRatingDTO);

            using (var db = await factory.CreateDbContextAsync())
            {
                var rating = await db.Ratings.FirstOrDefaultAsync(r => r.RatingId == ratingId);
                Assert.NotNull(rating);
                Assert.Equal(9, rating.Gameplay);
                Assert.Equal(8, rating.Graphics);
                Assert.Equal(10, rating.Optimization);
                Assert.Equal(7, rating.Story);
            }
        }

        [Fact]
        public async Task EditRatingAsync_ShouldThrowException_WhenRatingNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new RatingService(factory);
            var ratingDTO = new RatingDTO
            {
                Gameplay = 9,
                Graphics = 8,
                Optimization = 10,
                Story = 7
            };

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.EditRatingAsync(999, ratingDTO)
            );
            Assert.Equal("Rating not found", exception.Message);
        }

        [Fact]
        public async Task DeleteRatingAsync_ShouldRemoveRatingSuccessfully()
        {
            var factory = CreateDbContextFactory();
            int ratingId;

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

                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                var rating = new Rating
                {
                    GameId = game.GameId,
                    UserId = user.Id,
                    Gameplay = 8,
                    Graphics = 9,
                    Optimization = 7,
                    Story = 10
                };
                db.Ratings.Add(rating);
                await db.SaveChangesAsync();
                ratingId = rating.RatingId;
            }

            var service = new RatingService(factory);

            await service.DeleteRatingAsync(ratingId);

            using (var db = await factory.CreateDbContextAsync())
            {
                var rating = await db.Ratings.FirstOrDefaultAsync(r => r.RatingId == ratingId);
                Assert.Null(rating);
            }
        }

        [Fact]
        public async Task DeleteRatingAsync_ShouldThrowException_WhenRatingNotFound()
        {
            
            var factory = CreateDbContextFactory();
            var service = new RatingService(factory);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.DeleteRatingAsync(999)
            );
            Assert.Equal("Rating not found", exception.Message);
        }

        [Fact]
        public async Task GetUserRatingAsync_ShouldReturnCorrectRating()
        {
            var factory = CreateDbContextFactory();
            int gameId;
            string userId;

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

                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                gameId = game.GameId;
                userId = user.Id;

                var rating = new Rating
                {
                    GameId = gameId,
                    UserId = userId,
                    Gameplay = 8,
                    Graphics = 9,
                    Optimization = 7,
                    Story = 10
                };
                db.Ratings.Add(rating);
                await db.SaveChangesAsync();
            }

            var service = new RatingService(factory);


            var result = await service.GetUserRatingAsync(gameId, userId);


            Assert.NotNull(result);
            Assert.Equal(8, result.Gameplay);
            Assert.Equal(9, result.Graphics);
            Assert.Equal(7, result.Optimization);
            Assert.Equal(10, result.Story);
        }

        [Fact]
        public async Task GetUserRatingAsync_ShouldReturnNull_WhenRatingNotFound()
        {

            var factory = CreateDbContextFactory();
            var service = new RatingService(factory);


            var result = await service.GetUserRatingAsync(999, "nonexistentUser");


            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserRatingsAsync_ShouldReturnAllUserRatings()
        {
            
            var factory = CreateDbContextFactory();
            string userId;

            using (var db = await factory.CreateDbContextAsync())
            {
                var game1 = new Game
                {
                    Title = "Game 1",
                    Tag = Tags.Action,
                    Developer = "Dev1",
                    Publisher = "Pub1",
                    Description = "Desc1",
                    ReleaseYear = DateTime.UtcNow
                };
                var game2 = new Game
                {
                    Title = "Game 2",
                    Tag = Tags.RPG,
                    Developer = "Dev2",
                    Publisher = "Pub2",
                    Description = "Desc2",
                    ReleaseYear = DateTime.UtcNow
                };
                db.Games.AddRange(game1, game2);

                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                userId = user.Id;

                db.Ratings.AddRange(
                    new Rating { GameId = game1.GameId, UserId = userId, Gameplay = 8, Graphics = 9, Optimization = 7, Story = 10 },
                    new Rating { GameId = game2.GameId, UserId = userId, Gameplay = 6, Graphics = 7, Optimization = 8, Story = 9 }
                );
                await db.SaveChangesAsync();
            }

            var service = new RatingService(factory);


            var result = await service.GetUserRatingsAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Game.Title == "Game 1");
            Assert.Contains(result, r => r.Game.Title == "Game 2");
        }

        [Fact]
        public async Task GetUserRatingsAsync_ShouldReturnEmptyList_WhenUserHasNoRatings()
        {
            var factory = CreateDbContextFactory();
            string userId;

            using (var db = await factory.CreateDbContextAsync())
            {
                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                userId = user.Id;
            }

            var service = new RatingService(factory);

            var result = await service.GetUserRatingsAsync(userId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllRatingsAsync_ShouldReturnAllRatings()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                var game1 = new Game
                {
                    Title = "Game 1",
                    Tag = Tags.Action,
                    Developer = "Dev1",
                    Publisher = "Pub1",
                    Description = "Desc1",
                    ReleaseYear = DateTime.UtcNow
                };
                var game2 = new Game
                {
                    Title = "Game 2",
                    Tag = Tags.RPG,
                    Developer = "Dev2",
                    Publisher = "Pub2",
                    Description = "Desc2",
                    ReleaseYear = DateTime.UtcNow
                };
                db.Games.AddRange(game1, game2);

                var user1 = new IdentityUser { UserName = "user1" };
                var user2 = new IdentityUser { UserName = "user2" };
                db.Users.AddRange(user1, user2);
                await db.SaveChangesAsync();

                db.Ratings.AddRange(
                    new Rating { GameId = game1.GameId, UserId = user1.Id, Gameplay = 8, Graphics = 9, Optimization = 7, Story = 10 },
                    new Rating { GameId = game2.GameId, UserId = user1.Id, Gameplay = 6, Graphics = 7, Optimization = 8, Story = 9 },
                    new Rating { GameId = game1.GameId, UserId = user2.Id, Gameplay = 9, Graphics = 8, Optimization = 10, Story = 7 }
                );
                await db.SaveChangesAsync();
            }

            var service = new RatingService(factory);

            var result = await service.GetAllRatingsAsync();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, r => Assert.NotNull(r.Game));
        }

        [Fact]
        public async Task GetAllRatingsAsync_ShouldReturnEmptyList_WhenNoRatingsExist()
        {
            var factory = CreateDbContextFactory();
            var service = new RatingService(factory);

            var result = await service.GetAllRatingsAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddRatingAsync_ShouldSupportAllRatingRanges()
        {
            
            var factory = CreateDbContextFactory();
            int gameId;
            string userId;

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

                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);

                await db.SaveChangesAsync();
                gameId = game.GameId;
                userId = user.Id;
            }

            var service = new RatingService(factory);
            var ratingDTO = new RatingDTO
            {
                Gameplay = 1,
                Graphics = 10,
                Optimization = 5,
                Story = 7
            };

            
            await service.AddRatingAsync(gameId, userId, ratingDTO);

            
            using (var db = await factory.CreateDbContextAsync())
            {
                var rating = await db.Ratings.FirstOrDefaultAsync(r => r.GameId == gameId && r.UserId == userId);
                Assert.NotNull(rating);
                Assert.Equal(1, rating.Gameplay);
                Assert.Equal(10, rating.Graphics);
                Assert.Equal(5, rating.Optimization);
                Assert.Equal(7, rating.Story);
            }
        }

        [Fact]
        public async Task GetUserRatingsAsync_ShouldIncludeGameDetails()
        {
           
            var factory = CreateDbContextFactory();
            string userId;

            using (var db = await factory.CreateDbContextAsync())
            {
                var game = new Game
                {
                    Title = "Test Game",
                    Tag = Tags.Action,
                    Developer = "Test Dev",
                    Publisher = "Test Pub",
                    Description = "Test Desc",
                    ReleaseYear = DateTime.UtcNow
                };
                db.Games.Add(game);

                var user = new IdentityUser { UserName = "testuser" };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                userId = user.Id;

                db.Ratings.Add(new Rating
                {
                    GameId = game.GameId,
                    UserId = userId,
                    Gameplay = 8,
                    Graphics = 9,
                    Optimization = 7,
                    Story = 10
                });
                await db.SaveChangesAsync();
            }

            var service = new RatingService(factory);

            
            var result = await service.GetUserRatingsAsync(userId);

            
            Assert.NotNull(result);
            Assert.Single(result);
            var rating = result.First();
            Assert.NotNull(rating.Game);
            Assert.Equal("Test Game", rating.Game.Title);
            Assert.Equal("Test Dev", rating.Game.Developer);
        }
    }
}