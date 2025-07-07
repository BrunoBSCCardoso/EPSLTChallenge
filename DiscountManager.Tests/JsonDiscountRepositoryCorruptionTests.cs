using DiscountManager.Infrastructure.Persistence;

namespace DiscountManager.Tests
{
    public class JsonDiscountRepositoryCorruptionTests
    {
        [Fact]
        public void GetAll_Returns_Empty_On_Corrupt_File()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "not-a-json!");

            var logger = new NullLogger();
            var repo = new JsonDiscountRepository(tempFile, logger);

            var codes = repo.GetAll();

            Assert.Empty(codes);

            File.Delete(tempFile);
        }
    }
}
