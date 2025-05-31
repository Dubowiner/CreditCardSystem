using System.ComponentModel.DataAnnotations;

namespace CreditCardSystem.Models.Requests
{
    public class CambioPinRequest
    {
        [Required(ErrorMessage = "El PIN actual es requerido")]
        public string PinActual { get; set; }

        [Required(ErrorMessage = "El nuevo PIN es requerido")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "El PIN debe tener 4 dígitos")]
        public string NuevoPin { get; set; }
    }
}