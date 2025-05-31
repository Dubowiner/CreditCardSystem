namespace CreditCardSystem.Models
{
    public class TarjetaCredito
{
    public string Numero { get; set; } // Ej: "1234-5678-9012-3456"
    public string ClienteId { get; set; } // Relación con Cliente
    public decimal Saldo { get; set; }
    public decimal Limite { get; set; }
    public string Pin { get; set; } // Ej: "1234"
    public bool Bloqueada { get; set; } = false;
        public DateTime FechaVencimiento { get; set; }
    }
}