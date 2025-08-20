using BarakoCups.Models;
using System.ComponentModel.DataAnnotations;
using static BarakoCups.Models.Product;

namespace BarakoCups.ViewModels
{
    public class CheckoutVm
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }
}
