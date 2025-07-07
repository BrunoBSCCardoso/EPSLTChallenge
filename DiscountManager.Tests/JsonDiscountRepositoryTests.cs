using DiscountManager.Infrastructure.Persistence;

namespace DiscountManager.Tests
{
    public class JsonDiscountRepositoryTests
    {
        [Fact]
        public void SaveAndGetAll_Works()
        {
            // Arrange
            var tempPath = Path.GetTempFileName();
            var logger = new NullLogger();
            var repo = new JsonDiscountRepository(tempPath, logger);

            var codes = new[] { "AAA1111", "BBB2222" };

            // Act
            repo.SaveAll(codes);
            var loaded = repo.GetAll();

            // Assert
            Assert.Equal(2, loaded.Count());
            Assert.Contains("AAA1111", loaded);
            Assert.Contains("BBB2222", loaded);

            File.Delete(tempPath);
        }
    }

    public class NullLogger : DiscountManager.Domain.Interfaces.IFileLogger
    {
        public bool Info(string msg, string caller = "") => true;
        public bool Warn(string msg, string caller = "") => true;
        public bool Error(string msg, Exception? ex = null, string caller = "") => true;
    }
}
