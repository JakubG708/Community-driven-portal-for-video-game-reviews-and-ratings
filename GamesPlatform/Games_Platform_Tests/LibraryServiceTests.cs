using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamesPlatform.Data;
using GamesPlatform.Enums;
using GamesPlatform.Models;
using GamesPlatform.Services.Libraries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Games_Platform_Tests
{
    public class LibraryServiceTests
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
        public async Task CreateLibraryAsync_ShouldCreateLibraryForUser()
        {
            var factory = CreateDbContextFactory();
            var service = new LibraryService(factory);
            var userId = "testUser123";

            await service.CreateLibraryAsync(userId);

            using var db = await factory.CreateDbContextAsync();
            var library = await db.Libraries.FirstOrDefaultAsync(l => l.UserId == userId);
            Assert.NotNull(library);
            Assert.Equal(userId, library.UserId);
            Assert.Empty(library.LibGames);
        }

        [Fact]
        public async Task CreateLibraryAsync_ShouldThrowException_WhenLibraryAlreadyExists()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            using (var db = await factory.CreateDbContextAsync())
            {
                db.Libraries.Add(new Library { UserId = userId });
                await db.SaveChangesAsync();
            }
            var service = new LibraryService(factory);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.CreateLibraryAsync(userId)
            );
            Assert.Contains("Library already exists", exception.Message);
        }

        [Fact]
        public async Task GetLibraryId_ShouldReturnCorrectLibraryId()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            int expectedLibraryId;
            using (var db = await factory.CreateDbContextAsync())
            {
                var library = new Library { UserId = userId };
                db.Libraries.Add(library);
                await db.SaveChangesAsync();
                expectedLibraryId = library.LibraryId;
            }
            var service = new LibraryService(factory);

            var libraryId = await service.GetLibraryId(userId);

            Assert.Equal(expectedLibraryId, libraryId);
        }

        [Fact]
        public async Task GetLibraryId_ShouldThrowException_WhenLibraryNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new LibraryService(factory);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.GetLibraryId("nonexistentUser")
            );
            Assert.Contains("Library not found", exception.Message);
        }

        [Fact]
        public async Task AddGameToLibraryAsync_ShouldAddGameSuccessfully()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            int gameId;
            int libraryId;
            
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
                
                var library = new Library { UserId = userId };
                db.Libraries.Add(library);
                
                await db.SaveChangesAsync();
                gameId = game.GameId;
                libraryId = library.LibraryId;
            }
            
            var service = new LibraryService(factory);

            await service.AddGameToLibraryAsync(userId, gameId, Status.InProgress);

            using (var db = await factory.CreateDbContextAsync())
            {
                var libGame = await db.LibGames
                    .FirstOrDefaultAsync(lg => lg.LibraryId == libraryId && lg.GameId == gameId);
                
                Assert.NotNull(libGame);
                Assert.Equal(Status.InProgress, libGame.Status);
                Assert.True((DateTime.UtcNow - libGame.AddedAt).TotalSeconds < 5);
            }
        }

        [Fact]
        public async Task AddGameToLibraryAsync_ShouldThrowException_WhenGameNotFound()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            
            using (var db = await factory.CreateDbContextAsync())
            {
                db.Libraries.Add(new Library { UserId = userId });
                await db.SaveChangesAsync();
            }
            
            var service = new LibraryService(factory);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.AddGameToLibraryAsync(userId, 999, Status.InProgress)
            );
            Assert.Equal("Game not found", exception.Message);
        }

        [Fact]
        public async Task AddGameToLibraryAsync_ShouldThrowException_WhenLibraryNotFound()
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
            
            var service = new LibraryService(factory);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.AddGameToLibraryAsync("nonexistentUser", gameId, Status.InProgress)
            );
            Assert.Contains("Library not found", exception.Message);
        }

        [Fact]
        public async Task AddGameToLibraryAsync_ShouldThrowException_WhenGameAlreadyInLibrary()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
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
                
                var library = new Library { UserId = userId };
                db.Libraries.Add(library);
                await db.SaveChangesAsync();
                
                gameId = game.GameId;
                
                var libGame = new LibGame
                {
                    LibraryId = library.LibraryId,
                    GameId = gameId,
                    Status = Status.InProgress,
                    AddedAt = DateTime.UtcNow
                };
                db.LibGames.Add(libGame);
                await db.SaveChangesAsync();
            }
            
            var service = new LibraryService(factory);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await service.AddGameToLibraryAsync(userId, gameId, Status.Completed)
            );
            Assert.Equal("Game already in library", exception.Message);
        }

        [Fact]
        public async Task GetUserLibraryAsync_ShouldReturnAllGamesInLibrary()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            
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
                
                var library = new Library { UserId = userId };
                db.Libraries.Add(library);
                await db.SaveChangesAsync();
                
                db.LibGames.AddRange(
                    new LibGame { LibraryId = library.LibraryId, GameId = game1.GameId, Status = Status.Completed, AddedAt = DateTime.UtcNow },
                    new LibGame { LibraryId = library.LibraryId, GameId = game2.GameId, Status = Status.InProgress, AddedAt = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
            }
            
            var service = new LibraryService(factory);

            var libGames = await service.GetUserLibraryAsync(userId);

            Assert.NotNull(libGames);
            Assert.Equal(2, libGames.Count);
            Assert.Contains(libGames, lg => lg.Game.Title == "Game 1" && lg.Status == Status.Completed);
            Assert.Contains(libGames, lg => lg.Game.Title == "Game 2" && lg.Status == Status.InProgress);
        }

        [Fact]
        public async Task GetUserLibraryAsync_ShouldReturnEmptyList_WhenLibraryIsEmpty()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            
            using (var db = await factory.CreateDbContextAsync())
            {
                db.Libraries.Add(new Library { UserId = userId });
                await db.SaveChangesAsync();
            }
            
            var service = new LibraryService(factory);

           
            var libGames = await service.GetUserLibraryAsync(userId);

            Assert.NotNull(libGames);
            Assert.Empty(libGames);
        }

        [Fact]
        public async Task GetUserLibraryAsync_ShouldThrowException_WhenLibraryNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new LibraryService(factory);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.GetUserLibraryAsync("nonexistentUser")
            );
            Assert.Contains("Library not found", exception.Message);
        }

        [Fact]
        public async Task RemoveGameFromLibraryAsync_ShouldRemoveGameSuccessfully()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            int gameId;
            int libraryId;
            
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
                
                var library = new Library { UserId = userId };
                db.Libraries.Add(library);
                await db.SaveChangesAsync();
                
                gameId = game.GameId;
                libraryId = library.LibraryId;
                
                db.LibGames.Add(new LibGame
                {
                    LibraryId = libraryId,
                    GameId = gameId,
                    Status = Status.InProgress,
                    AddedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            
            var service = new LibraryService(factory);

            await service.RemoveGameFromLibraryAsync(userId, gameId);

            using (var db = await factory.CreateDbContextAsync())
            {
                var libGame = await db.LibGames
                    .FirstOrDefaultAsync(lg => lg.LibraryId == libraryId && lg.GameId == gameId);
                Assert.Null(libGame);
            }
        }

        [Fact]
        public async Task RemoveGameFromLibraryAsync_ShouldThrowException_WhenLibraryNotFound()
        {
            var factory = CreateDbContextFactory();
            var service = new LibraryService(factory);

           
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.RemoveGameFromLibraryAsync("nonexistentUser", 1)
            );
            Assert.Contains("Library not found", exception.Message);
        }

        [Fact]
        public async Task RemoveGameFromLibraryAsync_ShouldThrowException_WhenGameNotInLibrary()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            
            using (var db = await factory.CreateDbContextAsync())
            {
                db.Libraries.Add(new Library { UserId = userId });
                await db.SaveChangesAsync();
            }
            
            var service = new LibraryService(factory);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.RemoveGameFromLibraryAsync(userId, 999)
            );
            Assert.Contains("not found in user's library", exception.Message);
        }

        [Fact]
        public async Task AddGameToLibraryAsync_ShouldSupportAllStatusTypes()
        {
            var factory = CreateDbContextFactory();
            var userId = "testUser123";
            
            using (var db = await factory.CreateDbContextAsync())
            {
                var games = new[]
                {
                    new Game { Title = "Game 1", Tag = Tags.Action, Developer = "Dev", Publisher = "Pub", Description = "Desc", ReleaseYear = DateTime.UtcNow },
                    new Game { Title = "Game 2", Tag = Tags.RPG, Developer = "Dev", Publisher = "Pub", Description = "Desc", ReleaseYear = DateTime.UtcNow },
                    new Game { Title = "Game 3", Tag = Tags.Adventure, Developer = "Dev", Publisher = "Pub", Description = "Desc", ReleaseYear = DateTime.UtcNow }
                };
                db.Games.AddRange(games);
                db.Libraries.Add(new Library { UserId = userId });
                await db.SaveChangesAsync();
            }
            
            var service = new LibraryService(factory);

            
            using (var db = await factory.CreateDbContextAsync())
            {
                var gameIds = await db.Games.Select(g => g.GameId).ToListAsync();
                await service.AddGameToLibraryAsync(userId, gameIds[0], Status.Completed);
                await service.AddGameToLibraryAsync(userId, gameIds[1], Status.InProgress);
                await service.AddGameToLibraryAsync(userId, gameIds[2], Status.Dropped);
            }

            var library = await service.GetUserLibraryAsync(userId);
            Assert.Equal(3, library.Count);
            Assert.Contains(library, lg => lg.Status == Status.Completed);
            Assert.Contains(library, lg => lg.Status == Status.InProgress);
            Assert.Contains(library, lg => lg.Status == Status.Dropped);
        }
    }
}
