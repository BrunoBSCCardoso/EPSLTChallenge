using DiscountManager.Application.Services;
using DiscountManager.Domain.Interfaces;
using Moq;
using Xunit;

namespace DiscountManager.Tests
{
    public class DiscountManagerServiceTests
    {
        [Theory]
        [InlineData(0, 7)]
        [InlineData(-5, 7)]
        [InlineData(2001, 7)]
        [InlineData(1, 6)]
        [InlineData(1, 9)]
        public void GenerateCodes_Invalid_Params_ThrowsArgumentException(int count, int length)
        {
            var repoMock = new Mock<IDiscountRepository>();
            repoMock.Setup(r => r.GetAll()).Returns(Array.Empty<string>());
            var service = new DiscountCodeService(repoMock.Object);

            Assert.Throws<ArgumentException>(() => service.GenerateCodes(count, length));
        }

        [Fact]
        public void GenerateCodes_Generates_Correct_Number_And_Length()
        {
            // Arrange
            var repoMock = new Mock<IDiscountRepository>();
            repoMock.Setup(r => r.GetAll()).Returns(Array.Empty<string>());
            var service = new DiscountCodeService(repoMock.Object);

            // Act
            var codes = service.GenerateCodes(5, 8);

            // Assert
            Assert.Equal(5, codes.Count());
            Assert.All(codes, code => Assert.Equal(8, code.Length));
            Assert.Equal(5, codes.Distinct().Count());
        }

        [Fact]
        public void GenerateCodes_Does_Not_Repeat_Codes()
        {
            // Arrange
            var repoMock = new Mock<IDiscountRepository>();
            repoMock.Setup(r => r.GetAll()).Returns(new[] { "ABCDEFGH" });
            var service = new DiscountCodeService(repoMock.Object);

            // Act
            var codes = service.GenerateCodes(10, 8);

            // Assert
            Assert.DoesNotContain("ABCDEFGH", codes);
            Assert.Equal(10, codes.Count());
        }

        [Fact]
        public void UseCode_Removes_Code_When_Exists()
        {
            // Arrange
            var repoMock = new Mock<IDiscountRepository>();
            repoMock.Setup(r => r.GetAll()).Returns(new[] { "A1B2C3D4" });
            var service = new DiscountCodeService(repoMock.Object);

            // Act
            var result = service.UseCode("A1B2C3D4");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseCode_Returns_False_If_Code_Does_Not_Exist()
        {
            // Arrange
            var repoMock = new Mock<IDiscountRepository>();
            repoMock.Setup(r => r.GetAll()).Returns(Array.Empty<string>());
            var service = new DiscountCodeService(repoMock.Object);

            // Act
            var result = service.UseCode("ZZZZZZZZ");

            // Assert
            Assert.False(result);
        }
    }
}
