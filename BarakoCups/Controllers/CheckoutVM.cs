using BarakoCups.Models;
using System.ComponentModel.DataAnnotations;
using static BarakoCups.Models.Product;

public class CheckoutVM
{
    [Required]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}