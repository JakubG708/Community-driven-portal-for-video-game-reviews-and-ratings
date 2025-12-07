using GamesPlatform.Data;
using GamesPlatform.DTOs;
using GamesPlatform.Enums;
using GamesPlatform.Models;
using GamesPlatform.Services.Reviews;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Games_Platform_Tests
{
    public class ReviewServiceTests
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
        public async Task AddReviewAsync_ShouldAddReviewSuccessfully()
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

            var service = new ReviewsService(factory);

            await service.AddReviewAsync(gameId, userId, "Great game!");

            using (var db = await factory.CreateDbContextAsync())
            {
                var review = await db.Reviews.FirstOrDefaultAsync(r => r.GameId == gameId && r.UserId == userId);
                Assert.NotNull(review);
                Assert.Equal("Great game!", review.Description);
            }
        }

        [Fact]
        public async Task EditReviewAsync_ShouldUpdateReviewSuccessfully()
        {
            var factory = CreateDbContextFactory();
            int reviewId;

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

                var review = new Review
                {
                    GameId = game.GameId,
                    UserId = user.Id,
                    Description = "Original content"
                };
                db.Reviews.Add(review);
                await db.SaveChangesAsync();
                reviewId = review.ReviewId;
            }

            var service = new ReviewsService(factory);

            await service.EditReviewAsync(reviewId, "Updated content");

            using (var db = await factory.CreateDbContextAsync())
            {
                var review = await db.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
                Assert.NotNull(review);
                Assert.Equal("Updated content", review.Description);
            }
        }

        [Fact]
        public async Task EditReviewAsync_ShouldThrowException_WhenReviewNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new ReviewsService(factory);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.EditReviewAsync(999, "Updated content")
            );
            Assert.Equal("Review not found", exception.Message);
        }

        [Fact]
        public async Task DeleteReviewAsync_ShouldRemoveReviewSuccessfully()
        {
            var factory = CreateDbContextFactory();
            int reviewId;

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

                var review = new Review
                {
                    GameId = game.GameId,
                    UserId = user.Id,
                    Description = "Test review"
                };
                db.Reviews.Add(review);
                await db.SaveChangesAsync();
                reviewId = review.ReviewId;
            }

            var service = new ReviewsService(factory);

            await service.DeleteReviewAsync(reviewId);

            using (var db = await factory.CreateDbContextAsync())
            {
                var review = await db.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
                Assert.Null(review);
            }
        }

        [Fact]
        public async Task DeleteReviewAsync_ShouldThrowException_WhenReviewNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new ReviewsService(factory);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.DeleteReviewAsync(999)
            );
            Assert.Equal("Review not found", exception.Message);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ShouldReturnReviewWithDetails()
        {
            var factory = CreateDbContextFactory();
            int reviewId;

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

                var review = new Review
                {
                    GameId = game.GameId,
                    UserId = user.Id,
                    Description = "Test review"
                };
                db.Reviews.Add(review);
                await db.SaveChangesAsync();
                reviewId = review.ReviewId;
            }

            var service = new ReviewsService(factory);

            var result = await service.GetReviewByIdAsync(reviewId);

            Assert.NotNull(result);
            Assert.Equal("Test review", result.Description);
            Assert.NotNull(result.Game);
            Assert.Equal("Test Game", result.Game.Title);
            Assert.NotNull(result.User);
            Assert.Equal("testuser", result.User.UserName);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ShouldThrowException_WhenReviewNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new ReviewsService(factory);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.GetReviewByIdAsync(999)
            );
            Assert.Equal("Review not found", exception.Message);
        }

        [Fact]
        public async Task GetReviewsAsync_ShouldReturnAllReviews()
        {
            var factory = CreateDbContextFactory();

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

                var user1 = new IdentityUser { UserName = "user1" };
                var user2 = new IdentityUser { UserName = "user2" };
                db.Users.AddRange(user1, user2);
                await db.SaveChangesAsync();

                db.Reviews.AddRange(
                    new Review { GameId = game.GameId, UserId = user1.Id, Description = "Review 1" },
                    new Review { GameId = game.GameId, UserId = user2.Id, Description = "Review 2" }
                );
                await db.SaveChangesAsync();
            }

            var service = new ReviewsService(factory);

            var result = await service.GetReviewsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetReviewsAsync_ShouldReturnEmptyList_WhenNoReviewsExist()
        {
            var factory = CreateDbContextFactory();
            var service = new ReviewsService(factory);

            var result = await service.GetReviewsAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserReviewsAsync_ShouldReturnAllUserReviews()
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

                db.Reviews.AddRange(
                    new Review { GameId = game1.GameId, UserId = userId, Description = "Review 1" },
                    new Review { GameId = game2.GameId, UserId = userId, Description = "Review 2" }
                );
                await db.SaveChangesAsync();
            }

            var service = new ReviewsService(factory);

            var result = await service.GetUserReviewsAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Game.Title == "Game 1");
            Assert.Contains(result, r => r.Game.Title == "Game 2");
        }

        [Fact]
        public async Task GetUserReviewsAsync_ShouldReturnEmptyList_WhenUserHasNoReviews()
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

            var service = new ReviewsService(factory);

            var result = await service.GetUserReviewsAsync(userId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetReviewDTOsAsync_ShouldReturnAllReviewDTOs_WhenNoQueryProvided()
        {
            var factory = CreateDbContextFactory();

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

                var user = new IdentityUser { UserName = "testuser", Email = "test@example.com" };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                db.Reviews.Add(new Review
                {
                    GameId = game.GameId,
                    UserId = user.Id,
                    Description = "Test review"
                });
                await db.SaveChangesAsync();
            }

            var service = new ReviewsService(factory);

            var result = await service.GetReviewDTOsAsync();

            Assert.NotNull(result);
            Assert.Single(result);
            var dto = result.First();
            Assert.Equal("testuser", dto.UserName);
            Assert.Equal("Test Game", dto.GameTitle);
            Assert.Equal("Test review", dto.Description);
        }

        [Fact]
        public async Task GetReviewDTOsAsync_ShouldFilterByUsername()
        {
            var factory = CreateDbContextFactory();

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

                var user1 = new IdentityUser { UserName = "alice", Email = "alice@example.com" };
                var user2 = new IdentityUser { UserName = "bob", Email = "bob@example.com" };
                db.Users.AddRange(user1, user2);
                await db.SaveChangesAsync();

                db.Reviews.AddRange(
                    new Review { GameId = game.GameId, UserId = user1.Id, Description = "Alice review" },
                    new Review { GameId = game.GameId, UserId = user2.Id, Description = "Bob review" }
                );
                await db.SaveChangesAsync();
            }

            var service = new ReviewsService(factory);

            var result = await service.GetReviewDTOsAsync("alice", "username");

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("alice", result.First().UserName);
        }

        [Fact]
        public async Task GetReviewDTOsAsync_ShouldFilterByEmail()
        {
            var factory = CreateDbContextFactory();

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

                var user1 = new IdentityUser { UserName = "alice", Email = "alice@example.com" };
                var user2 = new IdentityUser { UserName = "bob", Email = "bob@example.com" };
                db.Users.AddRange(user1, user2);
                await db.SaveChangesAsync();

                db.Reviews.AddRange(
                    new Review { GameId = game.GameId, UserId = user1.Id, Description = "Alice review" },
                    new Review { GameId = game.GameId, UserId = user2.Id, Description = "Bob review" }
                );
                await db.SaveChangesAsync();
            }

            var service = new ReviewsService(factory);

            var result = await service.GetReviewDTOsAsync("alice@example.com", "email");

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("alice", result.First().UserName);
        }

        [Fact]
        public async Task GetReviewDTOsAsync_ShouldFilterByGameTitle()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                var game1 = new Game
                {
                    Title = "Action Game",
                    Tag = Tags.Action,
                    Developer = "Dev1",
                    Publisher = "Pub1",
                    Description = "Desc1",
                    ReleaseYear = DateTime.UtcNow
                };
                var game2 = new Game
                {
                    Title = "RPG Game",
                    Tag = Tags.RPG,
                    Developer = "Dev2",
                    Publisher = "Pub2",
                    Description = "Desc2",
                    ReleaseYear = DateTime.UtcNow
                };
                db.Games.AddRange(game1, game2);

                var user = new IdentityUser { UserName = "testuser", Email = "test@example.com" };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                db.Reviews.AddRange(
                    new Review { GameId = game1.GameId, UserId = user.Id, Description = "Review 1" },
                    new Review { GameId = game2.GameId, UserId = user.Id, Description = "Review 2" }
                );
                await db.SaveChangesAsync();
            }

            var service = new ReviewsService(factory);

            var result = await service.GetReviewDTOsAsync("Action", "game");

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Action Game", result.First().GameTitle);
        }

        [Fact]
        public async Task GetReviewDTOsAsync_ShouldReturnEmptyList_WhenNoMatchesFound()
        {
            var factory = CreateDbContextFactory();

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

                var user = new IdentityUser { UserName = "testuser", Email = "test@example.com" };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                db.Reviews.Add(new Review
                {
                    GameId = game.GameId,
                    UserId = user.Id,
                    Description = "Test review"
                });
                await db.SaveChangesAsync();
            }

            var service = new ReviewsService(factory);

            var result = await service.GetReviewDTOsAsync("nonexistent", "username");

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}