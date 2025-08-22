using BarakoCups.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BarakoCups.MyMvc.Test
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange  
            var logger = new LoggerFactory().CreateLogger<HomeController>();
            var controller = new HomeController(logger);

            // Act  
            var result = controller.Index();

            Xunit.Assert.IsType<ViewResult>(result); // ✅ Explicitly call xUnit Assert
        }
    }
}
