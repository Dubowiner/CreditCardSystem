using Microsoft.AspNetCore.Mvc;
using CreditCardSystem.Models;          // Para los modelos de datos
using CreditCardSystem.Models.Requests; // Para PagoRequest, BloqueoRequest, etc.
using CreditCardSystem.Models.Responses; // Para SaldoResponse, PagoResponse
using CreditCardSystem.Services;       // Para DataService

namespace CreditCardSystem.Controllers
{
    /// <summary>
    /// Controlador para operaciones con tarjetas de crédito
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TarjetasController : ControllerBase
    {
        private readonly DataService _dataService;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public TarjetasController(DataService dataService)
        {
            _dataService = dataService;
        }

        // =============================================
        // ENDPOINT: CONSULTAR SALDO
        // =============================================
        /// <summary>
        /// Obtiene el saldo actual de una tarjeta
        /// </summary>
        /// <param name="numero">Número de tarjeta (ej: "1234-5678-9012-3456")</param>
        /// <response code="200">Retorna el saldo y límite</response>
        /// <response code="404">Si la tarjeta no existe</response>
        [HttpGet("{numero}/saldo")]
        [ProducesResponseType(typeof(SaldoResponse), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetSaldo(string numero)
        {
            // 1. Buscar la tarjeta en el sistema
            var tarjeta = _dataService.BuscarTarjeta(numero);

            // 2. Validar existencia
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            // 3. Retornar respuesta estructurada
            return Ok(new SaldoResponse
            {
                Saldo = tarjeta.Saldo,
                Limite = tarjeta.Limite,
                FechaConsulta = DateTime.Now
            });
        }

        // =============================================
        // ENDPOINT: REALIZAR PAGO
        // =============================================
        /// <summary>
        /// Registra un pago con la tarjeta
        /// </summary>
        /// <param name="numero">Número de tarjeta</param>
        /// <param name="request">Datos del pago</param>
        /// <response code="200">Pago exitoso</response>
        /// <response code="400">Monto inválido</response>
        /// <response code="404">Tarjeta no encontrada</response>
        [HttpPost("{numero}/pagar")]
        [ProducesResponseType(typeof(PagoResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Pagar(string numero, [FromBody] PagoRequest request)
        {
            // 1. Validar existencia de tarjeta
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Error = "Tarjeta no existe" });

            // 2. Validar monto positivo
            if (request.Monto <= 0)
                return BadRequest(new { Error = "El monto debe ser positivo" });

            // 3. Procesar pago
            tarjeta.Saldo -= request.Monto;

            // 4. Registrar transacción
            _dataService.RegistrarTransaccion(numero, request.Monto, "Pago");

            // 5. Retornar confirmación
            return Ok(new PagoResponse
            {
                NuevoSaldo = tarjeta.Saldo,
                Mensaje = "Pago realizado exitosamente"
            });
        }

        // =============================================
        // ENDPOINT: CONSULTAR MOVIMIENTOS
        // =============================================
        /// <summary>
        /// Obtiene el historial de transacciones recientes
        /// </summary>
        [HttpGet("{numero}/movimientos")]
        [ProducesResponseType(typeof(List<Transaccion>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetMovimientos(string numero)
        {
            // 1. Validar existencia
            if (!_dataService.Tarjetas.Any(t => t.Numero == numero))
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            // 2. Filtrar transacciones de esta tarjeta
            var movimientos = _dataService.TransaccionesRecientes
                .Where(t => t.TarjetaNumero == numero)
                .OrderByDescending(t => t.Fecha)
                .ToList();

            return Ok(movimientos);
        }

        // =============================================
        // ENDPOINT: BLOQUEAR/DESBLOQUEAR TARJETA
        // =============================================
        /// <summary>
        /// Cambia el estado de bloqueo de una tarjeta
        /// </summary>
        [HttpPut("{numero}/bloquear")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult BloquearTarjeta(string numero, [FromBody] BloqueoRequest request)
        {
            // 1. Buscar tarjeta
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            // 2. Cambiar estado
            tarjeta.Bloqueada = request.Bloquear;

            // 3. Registrar acción
            _dataService.RegistrarTransaccion(numero, 0,
                request.Bloquear ? "Bloqueo" : "Desbloqueo");

            return Ok(new
            {
                Mensaje = $"Tarjeta {(request.Bloquear ? "bloqueada" : "desbloqueada")} correctamente",
                Bloqueada = tarjeta.Bloqueada
            });
        }

        // =============================================
        // ENDPOINT: CAMBIAR PIN
        // =============================================
        /// <summary>
        /// Actualiza el PIN de la tarjeta
        /// </summary>
        [HttpPut("{numero}/cambiar-pin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult CambiarPin(string numero, [FromBody] CambioPinRequest request)
        {
            // 1. Validar existencia
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            // 2. Validar PIN actual
            if (tarjeta.Pin != request.PinActual)
                return BadRequest(new { Error = "PIN actual incorrecto" });

            // 3. Validar formato nuevo PIN (4 dígitos)
            if (request.NuevoPin?.Length != 4 || !request.NuevoPin.All(char.IsDigit))
                return BadRequest(new { Error = "El nuevo PIN debe ser 4 dígitos numéricos" });

            // 4. Actualizar PIN
            tarjeta.Pin = request.NuevoPin;

            return Ok(new { Mensaje = "PIN actualizado correctamente" });
        }
    }
}