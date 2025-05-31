using System.Text.Json;
using CreditCardSystem.Models;

namespace CreditCardSystem.Services
{
    public class DataService
    {
        // 1. Estructuras de datos (actualizadas)
        public Dictionary<string, Cliente> Clientes { get; } = new();
        public Dictionary<string, TarjetaCredito> Tarjetas { get; } = new(); // Tabla Hash 
        public Stack<Transaccion> TransaccionesRecientes { get; } = new();
        public Queue<Transaccion> TransaccionesPendientes { get; } = new();

        public DataService()
        {
            CargarDatosIniciales();
        }

        private void CargarDatosIniciales()
        {
            try
            {
                // Carga de clientes
                if (File.Exists("Data/clientes.json"))
                {
                    var jsonClientes = File.ReadAllText("Data/clientes.json");
                    var clientes = JsonSerializer.Deserialize<List<Cliente>>(jsonClientes);
                    foreach (var c in clientes.Where(c => !Clientes.ContainsKey(c.Id)))
                    {
                        Clientes.Add(c.Id, c);
                    }
                }

                // Carga de tarjetas (usando Dictionary temporal)
                if (File.Exists("Data/tarjetas.json"))
                {
                    var jsonTarjetas = File.ReadAllText("Data/tarjetas.json");
                    var tarjetas = JsonSerializer.Deserialize<List<TarjetaCredito>>(jsonTarjetas);
                    foreach (var t in tarjetas.Where(t => !Tarjetas.ContainsKey(t.Numero)))
                    {
                        Tarjetas.Add(t.Numero, t);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando datos: {ex.Message}");
                CargarDatosDePrueba(); // Renombrado para evitar conflictos
            }
        }

        private void CargarDatosDePrueba()
        {
            // Cliente demo
            if (!Clientes.ContainsKey("1"))
            {
                Clientes.Add("1", new Cliente
                {
                    Id = "1",
                    Nombre = "Cliente Demo",
                    Email = "demo@banco.com",
                    Telefono = "555-1234"
                });
            }

            // Tarjeta demo
            if (!Tarjetas.ContainsKey("1234-5678-9012-3456"))
            {
                Tarjetas.Add("1234-5678-9012-3456", new TarjetaCredito
                {
                    Numero = "1234-5678-9012-3456",
                    ClienteId = "1",
                    Saldo = 5000,
                    Limite = 10000,
                    Pin = "1234",
                    Bloqueada = false
                });
            }
        }

        // Métodos clave (actualizados)
        public TarjetaCredito BuscarTarjeta(string numero)
        {
            return Tarjetas.TryGetValue(numero, out var tarjeta) ? tarjeta : null;
        }

        public void RegistrarTransaccion(string numeroTarjeta, decimal monto, string tipo)
        {
            var transaccion = new Transaccion
            {
                Id = Guid.NewGuid().ToString(), // Convertido a string
                TarjetaNumero = numeroTarjeta,
                Monto = monto,
                Tipo = tipo,
                Fecha = DateTime.Now
            };

            // Limitar historial
            if (TransaccionesRecientes.Count >= 10)
                TransaccionesRecientes.Pop();

            TransaccionesRecientes.Push(transaccion);
            TransaccionesPendientes.Enqueue(transaccion);
        }

        public Cliente BuscarCliente(string id)
        {
            return Clientes.TryGetValue(id, out var cliente) ? cliente : null;
        }
    }
}