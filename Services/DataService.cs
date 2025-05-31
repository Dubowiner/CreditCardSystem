using System.Text.Json;
using CreditCardSystem.Models;

namespace CreditCardSystem.Services
{
    public class DataService
    {
        // Estructuras de datos (requeridas en el proyecto)
        public Dictionary<string, Cliente> Clientes { get; } = new(); // Tabla Hash
        public List<TarjetaCredito> Tarjetas { get; } = new(); // Lista enlazada
        public Stack<Transaccion> TransaccionesRecientes { get; } = new(); // Pila (últimas transacciones)
        public Queue<Transaccion> TransaccionesPendientes { get; } = new(); // Cola (procesamiento batch)

        public DataService()
        {
            // Limpia estructuras antes de cargar
            Clientes.Clear();
            Tarjetas.Clear();
            TransaccionesRecientes.Clear();
            TransaccionesPendientes.Clear();

            CargarDatosIniciales(); // Carga datos al iniciar
        }

        // Método principal de carga de datos
        private void CargarDatosIniciales()
        {
            try
            {
                // 1. Carga clientes desde JSON con validación de duplicados
                if (File.Exists("Data/clientes.json"))
                {
                    string jsonClientes = File.ReadAllText("Data/clientes.json");
                    var clientes = JsonSerializer.Deserialize<List<Cliente>>(jsonClientes);

                    foreach (var c in clientes)
                    {
                        if (!Clientes.ContainsKey(c.Id)) // Evita duplicados
                        {
                            Clientes.Add(c.Id, c);
                        }
                        else
                        {
                            Console.WriteLine($"Advertencia: Cliente duplicado omitido (ID: {c.Id})");
                        }
                    }
                }

                // 2. Carga tarjetas desde JSON con validación
                if (File.Exists("Data/tarjetas.json"))
                {
                    string jsonTarjetas = File.ReadAllText("Data/tarjetas.json");
                    var tarjetas = JsonSerializer.Deserialize<List<TarjetaCredito>>(jsonTarjetas);

                    foreach (var t in tarjetas)
                    {
                        if (!Tarjetas.Any(tarj => tarj.Numero == t.Numero)) // Evita duplicados
                        {
                            Tarjetas.Add(t);
                        }
                        else
                        {
                            Console.WriteLine($"Advertencia: Tarjeta duplicada omitida (Número: {t.Numero})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando datos: {ex.Message}");
                CargarDatosDemo(); // Si hay error, carga datos de prueba
            }
        }

        // Datos de prueba (por si falla la carga de JSON)
        private void CargarDatosDemo()
        {
            try
            {
                // Cliente demo (solo si no existe)
                if (!Clientes.ContainsKey("1"))
                {
                    var clienteDemo = new Cliente
                    {
                        Id = "1",
                        Nombre = "Cliente Demo",
                        Email = "demo@banco.com",
                        Telefono = "555-1234"
                    };
                    Clientes.Add(clienteDemo.Id, clienteDemo);
                }

                // Tarjeta demo (solo si no existe)
                if (!Tarjetas.Any(t => t.Numero == "1234-5678-9012-3456"))
                {
                    Tarjetas.Add(new TarjetaCredito
                    {
                        Numero = "1234-5678-9012-3456",
                        ClienteId = "1",
                        Saldo = 5000,
                        Limite = 10000,
                        Pin = "1234",
                        Bloqueada = false
                    });
                }

                // Transacción demo
                RegistrarTransaccion("1234-5678-9012-3456", 1000, "Carga inicial");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando datos demo: {ex.Message}");
            }
        }

        // ---- Métodos útiles para los controllers ----
        public TarjetaCredito BuscarTarjeta(string numeroTarjeta)
        {
            return Tarjetas.FirstOrDefault(t => t.Numero == numeroTarjeta);
        }

        public void RegistrarTransaccion(string numeroTarjeta, decimal monto, string tipo)
        {
            var transaccion = new Transaccion
            {
                Id = Guid.NewGuid().ToString(),
                TarjetaNumero = numeroTarjeta,
                Monto = monto,
                Tipo = tipo,
                Fecha = DateTime.Now
            };

            // Agrega a la pila (límite: 10)
            if (TransaccionesRecientes.Count >= 10)
            {
                TransaccionesRecientes.Pop();
            }
            TransaccionesRecientes.Push(transaccion);

            // Agrega a la cola
            TransaccionesPendientes.Enqueue(transaccion);
        }

        // Método adicional para obtener cliente por ID
        public Cliente BuscarCliente(string id)
        {
            return Clientes.TryGetValue(id, out var cliente) ? cliente : null;
        }
    }
}