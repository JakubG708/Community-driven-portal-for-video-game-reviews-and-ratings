using GamesPlatform.Data;
using GamesPlatform.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Games_Platform_Tests
{
    public class UserServiceTests
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
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var factory = CreateDbContextFactory();
            string userId;

            using (var db = await factory.CreateDbContextAsync())
            {
                var user = new IdentityUser { UserName = "testuser", Email = "test@example.com" };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                userId = user.Id;
            }

            var service = new UserService(factory);

            var result = await service.GetUserByIdAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("testuser", result.UserName);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new UserService(factory);

            var result = await service.GetUserByIdAsync("nonexistentUserId");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnAllUsers_WhenNoQueryProvided()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "alice", Email = "alice@example.com" },
                    new IdentityUser { UserName = "bob", Email = "bob@example.com" },
                    new IdentityUser { UserName = "charlie", Email = "charlie@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            var factory = CreateDbContextFactory();
            var service = new UserService(factory);

            var result = await service.GetUsersAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldFilterByUsername()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "alice", Email = "alice@example.com" },
                    new IdentityUser { UserName = "bob", Email = "bob@example.com" },
                    new IdentityUser { UserName = "alicia", Email = "alicia@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("ali", "username");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.UserName == "alice");
            Assert.Contains(result, u => u.UserName == "alicia");
            Assert.DoesNotContain(result, u => u.UserName == "bob");
        }

        [Fact]
        public async Task GetUsersAsync_ShouldFilterByEmail()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "alice", Email = "alice@example.com" },
                    new IdentityUser { UserName = "bob", Email = "bob@test.com" },
                    new IdentityUser { UserName = "charlie", Email = "charlie@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("example", "email");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Email == "alice@example.com");
            Assert.Contains(result, u => u.Email == "charlie@example.com");
            Assert.DoesNotContain(result, u => u.Email == "bob@test.com");
        }

        [Fact]
        public async Task GetUsersAsync_ShouldDefaultToUsernameFilter_WhenFilterByIsNull()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "alice", Email = "alice@example.com" },
                    new IdentityUser { UserName = "bob", Email = "bob@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("alice", null);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("alice", result.First().UserName);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldBeCaseInsensitive()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "Alice", Email = "ALICE@EXAMPLE.COM" },
                    new IdentityUser { UserName = "bob", Email = "bob@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var resultUsername = await service.GetUsersAsync("alice", "username");
            var resultEmail = await service.GetUsersAsync("alice@example.com", "email");

            Assert.Single(resultUsername);
            Assert.Equal("Alice", resultUsername.First().UserName);
            Assert.Single(resultEmail);
            Assert.Equal("ALICE@EXAMPLE.COM", resultEmail.First().Email);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnEmptyList_WhenNoMatchesFound()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "alice", Email = "alice@example.com" },
                    new IdentityUser { UserName = "bob", Email = "bob@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("nonexistent", "username");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldHandlePartialMatches()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "alexander", Email = "alexander@example.com" },
                    new IdentityUser { UserName = "alex", Email = "alex@example.com" },
                    new IdentityUser { UserName = "bob", Email = "bob@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("alex", "username");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.UserName == "alexander");
            Assert.Contains(result, u => u.UserName == "alex");
        }

        [Fact]
        public async Task GetUsersAsync_ShouldHandleNullUsername()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.Add(new IdentityUser { UserName = null, Email = "test@example.com" });
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("test", "username");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldHandleNullEmail()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.Add(new IdentityUser { UserName = "testuser", Email = null });
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("test", "email");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnAllUsers_WhenQueryIsEmpty()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "alice", Email = "alice@example.com" },
                    new IdentityUser { UserName = "bob", Email = "bob@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("", "username");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnAllUsers_WhenQueryIsWhitespace()
        {
            var factory = CreateDbContextFactory();

            using (var db = await factory.CreateDbContextAsync())
            {
                db.Users.AddRange(
                    new IdentityUser { UserName = "alice", Email = "alice@example.com" },
                    new IdentityUser { UserName = "bob", Email = "bob@example.com" }
                );
                await db.SaveChangesAsync();
            }

            var service = new UserService(factory);

            var result = await service.GetUsersAsync("   ", "username");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}