using Xunit;

namespace MyTitlePawnCompany.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void SimpleMath_Addition_IsCorrect()
        {
            // Arrange
            int a = 2;
            int b = 3;

            // Act
            int result = a + b;

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void SimpleMath_Subtraction_IsCorrect()
        {
            // Arrange
            int a = 10;
            int b = 3;

            // Act
            int result = a - b;

            // Assert
            Assert.Equal(7, result);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 3, 5)]
        [InlineData(5, 5, 10)]
        public void SimpleMath_AdditionTheory_IsCorrect(int a, int b, int expected)
        {
            // Act
            int result = a + b;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
