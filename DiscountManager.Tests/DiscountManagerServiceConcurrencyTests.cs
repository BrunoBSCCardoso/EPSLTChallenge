using DiscountManager.Application.Services;
using DiscountManager.Domain.Interfaces;
using Moq;
using System.Collections.Concurrent;

namespace DiscountManager.Tests
{
    public class DiscountManagerServiceConcurrencyTests
    {
        [Fact]
        public void GenerateCodesAsync_Is_ThreadSafe()
        {
            var repoMock = new Mock<IDiscountRepository>();
            repoMock.Setup(r => r.GetAll()).Returns(Array.Empty<string>());
            var service = new DiscountCodeService(repoMock.Object);

            int concurrentTasks = 10;
            int codesPerTask = 200;
            var allCodes = new ConcurrentBag<string>();

            Parallel.For(0, concurrentTasks, _ =>
            {
                var codes = service.GenerateCodes(codesPerTask, 8);
                foreach (var c in codes)
                    allCodes.Add(c);
            });

            // No duplicate codes, all are unique
            Assert.Equal(concurrentTasks * codesPerTask, allCodes.Distinct().Count());
        }
    }
}
