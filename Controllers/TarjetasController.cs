using Microsoft.AspNetCore.Mvc;
using CreditCardSystem.Models;
using CreditCardSystem.Models.Requests;
using CreditCardSystem.Models.Responses;
using CreditCardSystem.Services;

namespace CreditCardSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TarjetasController : ControllerBase
    {
        private readonly DataService _dataService;

        public TarjetasController(DataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet("{numero}/saldo")]
        [ProducesResponseType(typeof(SaldoResponse), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetSaldo(string numero)
        {
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            return Ok(new SaldoResponse
            {
                Saldo = tarjeta.Saldo,
                Limite = tarjeta.Limite,
                FechaConsulta = DateTime.Now
            });
        }

        [HttpPost("{numero}/pagar")]
        [ProducesResponseType(typeof(PagoResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Pagar(string numero, [FromBody] PagoRequest request)
        {
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Error = "Tarjeta no existe" });

            if (tarjeta.Bloqueada)
                return BadRequest(new { Error = "Tarjeta bloqueada. Operación no permitida." });

            if (request.Monto <= 0)
                return BadRequest(new { Error = "El monto debe ser positivo" });

            tarjeta.Saldo -= request.Monto;
            _dataService.RegistrarTransaccion(numero, request.Monto, "Pago");

            return Ok(new PagoResponse
            {
                NuevoSaldo = tarjeta.Saldo,
                Mensaje = "Pago realizado exitosamente"
            });
        }

        [HttpGet("{numero}/movimientos")]
        [ProducesResponseType(typeof(List<Transaccion>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetMovimientos(string numero)
        {
            if (_dataService.BuscarTarjeta(numero) == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            var movimientos = _dataService.TransaccionesRecientes
                .Where(t => t.TarjetaNumero == numero)
                .OrderByDescending(t => t.Fecha)
                .ToList();

            return Ok(movimientos);
        }

        [HttpPut("{numero}/bloquear")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult BloquearTarjeta(string numero, [FromBody] BloqueoRequest request)
        {
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            tarjeta.Bloqueada = request.Bloquear;
            _dataService.RegistrarTransaccion(numero, 0, request.Bloquear ? "Bloqueo" : "Desbloqueo");

            return Ok(new
            {
                Mensaje = $"Tarjeta {(request.Bloquear ? "bloqueada" : "desbloqueada")} correctamente",
                Bloqueada = tarjeta.Bloqueada
            });
        }

        [HttpPut("{numero}/cambiar-pin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult CambiarPin(string numero, [FromBody] CambioPinRequest request)
        {
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            if (tarjeta.Bloqueada)
                return BadRequest(new { Error = "Tarjeta bloqueada. No se puede cambiar el PIN." });

            if (tarjeta.Pin != request.PinActual)
                return BadRequest(new { Error = "PIN actual incorrecto" });

            if (request.NuevoPin?.Length != 4 || !request.NuevoPin.All(char.IsDigit))
                return BadRequest(new { Error = "El nuevo PIN debe ser 4 dígitos numéricos" });

            tarjeta.Pin = request.NuevoPin;
            return Ok(new { Mensaje = "PIN actualizado correctamente" });
        }

        [HttpPut("{numero}/renovar")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult RenovarTarjeta(string numero)
        {
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            if (tarjeta.Bloqueada)
                return BadRequest(new { Error = "Tarjeta bloqueada. No se puede renovar." });

            tarjeta.FechaVencimiento = tarjeta.FechaVencimiento.AddYears(2);
            _dataService.RegistrarTransaccion(numero, 0, "Renovación");

            return Ok(new
            {
                Mensaje = "Tarjeta renovada",
                NuevaFecha = tarjeta.FechaVencimiento.ToString("yyyy-MM-dd")
            });
        }

        [HttpPut("{numero}/aumentar-limite")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult AumentarLimite(string numero, [FromBody] AumentoLimiteRequest request)
        {
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            if (tarjeta.Bloqueada)
                return BadRequest(new { Error = "Tarjeta bloqueada. No se puede modificar el límite." });

            if (request.NuevoLimite <= tarjeta.Limite)
                return BadRequest(new { Error = "El nuevo límite debe ser mayor al actual" });

            tarjeta.Limite = request.NuevoLimite;
            return Ok(new
            {
                Mensaje = "Límite actualizado",
                NuevoLimite = tarjeta.Limite
            });
        }

        [HttpPost("{numero}/consumo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult RegistrarConsumo(string numero, [FromBody] ConsumoRequest request)
        {
            var tarjeta = _dataService.BuscarTarjeta(numero);
            if (tarjeta == null)
                return NotFound(new { Mensaje = "Tarjeta no encontrada" });

            if (tarjeta.Bloqueada)
                return BadRequest(new { Error = "Tarjeta bloqueada. No se pueden registrar consumos." });

            if (request.Monto <= 0)
                return BadRequest(new { Error = "Monto inválido" });

            if (tarjeta.Saldo + request.Monto > tarjeta.Limite)
                return BadRequest(new { Error = "Límite de crédito excedido" });

            tarjeta.Saldo += request.Monto;
            _dataService.RegistrarTransaccion(numero, request.Monto, "Consumo");

            return Ok(new
            {
                Mensaje = "Consumo registrado",
                NuevoSaldo = tarjeta.Saldo
            });
        }
    }
}