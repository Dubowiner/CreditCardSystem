using System.ComponentModel.DataAnnotations;

namespace CreditCardSystem.Models.Requests
{
    public class PagoRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser positivo")]
        public decimal Monto { get; set; }
    }
}