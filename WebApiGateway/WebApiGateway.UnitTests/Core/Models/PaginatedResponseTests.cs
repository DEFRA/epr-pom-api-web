using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Core.Models.Pagination;

namespace WebApiGateway.UnitTests.Core.Models
{
    [TestClass]
    public class PaginatedResponseTests
    {
        [TestMethod]
        public void PageCount_ShouldReturnZero_WhenPageSizeIsZero()
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<object>
            {
                PageSize = 0,
                TotalItems = 100,
                CurrentPage = 1
            };

            // Act
            var pageCount = paginatedResponse.PageCount;

            // Assert
            pageCount.Should().Be(0);
        }

        [TestMethod]
        public void PageCount_ShouldReturnCorrectPageCount_WhenPageSizeIsNonZero()
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<object>
            {
                PageSize = 10,
                TotalItems = 95,
                CurrentPage = 1
            };

            // Act
            var pageCount = paginatedResponse.PageCount;

            // Assert
            pageCount.Should().Be(10);
        }

        [TestMethod]
        public void PageCount_ShouldReturnCalculatedPageCount_WhenCurrentPageIsLessThanCalculatedPageCount()
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<object>
            {
                PageSize = 10,
                TotalItems = 95,
                CurrentPage = 5
            };

            // Act
            var pageCount = paginatedResponse.PageCount;

            // Assert
            pageCount.Should().Be(10);
        }

        [TestMethod]
        public void PageCount_ShouldReturnCalculatedPageCount_WhenCurrentPageIsGreaterThanCalculatedPageCount()
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<object>
            {
                PageSize = 10,
                TotalItems = 95,
                CurrentPage = 15
            };

            // Act
            var pageCount = paginatedResponse.PageCount;

            // Assert
            pageCount.Should().Be(10);
        }
    }
}
