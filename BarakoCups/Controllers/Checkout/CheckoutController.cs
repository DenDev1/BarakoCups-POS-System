using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarakoCups.Controllers.Checkout
{
    public class CheckoutController : Controller
    {
        // Already existing methods...
        [Authorize]
        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Failed()
        {
            return View();
        }


    }

}
