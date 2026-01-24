using Listenarr.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Listenarr.Api.Tests
{
    public class AudiobookDtoFactoryTests
    {
        [Fact]
        public async Task BuildFromEntity_MapsFieldsAndFiles_AndComputesWanted()
        {
            var options = new DbContextOptionsBuilder<ListenArrDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new ListenArrDbContext(options);

            var book = new Audiobook
            {
                Title = "Factory Book",
                Authors = new System.Collections.Generic.List<string> { "Author One" },
                BasePath = "C:\\test\\book",
                Monitored = true
            };

            db.Audiobooks.Add(book);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var file = new AudiobookFile { AudiobookId = book.Id, Path = "C:\\test\\book\\file1.m4b", Size = 12345, CreatedAt = DateTime.UtcNow };
            db.AudiobookFiles.Add(file);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var updated = await db.Audiobooks.Include(a => a.Files).FirstOrDefaultAsync(a => a.Id == book.Id, TestContext.Current.CancellationToken);

            var dto = AudiobookDtoFactory.BuildFromEntity(db, updated);

            Assert.Equal(book.Id, dto.Id);
            Assert.Equal(book.Title, dto.Title);
            Assert.Contains("Author One", dto.Authors ?? new string[] { });
            Assert.Equal(book.BasePath, dto.BasePath);
            Assert.NotNull(dto.Files);
            Assert.Single(dto.Files);
            Assert.False(dto.Wanted == true, "With a file present, wanted should be false");
        }

        [Fact]
        public async Task BuildFromEntity_ComputesWantedWhenNoFiles()
        {
            var options = new DbContextOptionsBuilder<ListenArrDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new ListenArrDbContext(options);

            var book = new Audiobook
            {
                Title = "NoFiles Book",
                Monitored = true
            };

            db.Audiobooks.Add(book);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var updated = await db.Audiobooks.Include(a => a.Files).FirstOrDefaultAsync(a => a.Id == book.Id, TestContext.Current.CancellationToken);
            var dto = AudiobookDtoFactory.BuildFromEntity(db, updated);

            Assert.True(dto.Wanted == true);
        }
    }
}
