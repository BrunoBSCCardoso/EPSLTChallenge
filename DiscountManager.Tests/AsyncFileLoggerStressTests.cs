using DiscountManager.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountManager.Tests
{
    public class AsyncFileLoggerStressTests
    {
        [Fact]
        public void Logger_Writes_Parallel_Messages()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var tempFile = Path.Combine(tempDir, "log.txt");

            using (var logger = new AsyncFileLogger(tempFile))
            {
                int threads = 20;
                int linesPerThread = 100;
                Parallel.For(0, threads, i =>
                {
                    for (int j = 0; j < linesPerThread; j++)
                        logger.Info($"Message {i}-{j}");
                });
            }

            // Wait a little for flush (should be done at Dispose, but just in case)
            Thread.Sleep(100);

            var lines = File.ReadAllLines(tempFile);
            Assert.Equal(2000, lines.Length); // 20*100

            Directory.Delete(tempDir, true);
        }
    }
}
