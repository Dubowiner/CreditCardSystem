namespace CreditCardSystem.Models
{
    public class Transaccion
{
    public string Id { get; set; }
    public string TarjetaNumero { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Tipo { get; set; } // "Compra", "Pago", etc.
}
}