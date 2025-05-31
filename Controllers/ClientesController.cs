using CreditCardSystem.Models;
using CreditCardSystem.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly DataService _dataService;

    public ClientesController(DataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// Obtiene todos los clientes registrados
    /// </summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_dataService.Clientes.Values.ToList());
    }

    /// <summary>
    /// Obtiene un cliente específico por su ID
    /// </summary>
    /// <param name="id">ID del cliente</param>
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        return _dataService.Clientes.TryGetValue(id, out var cliente)
            ? Ok(cliente)
            : NotFound(new { Mensaje = $"Cliente con ID {id} no encontrado" });
    }

    /// <summary>
    /// Crea un nuevo cliente
    /// </summary>
    /// <param name="cliente">Datos del cliente a crear</param>
    [HttpPost]
    public IActionResult Create([FromBody] Cliente cliente)
    {
        if (_dataService.Clientes.ContainsKey(cliente.Id))
            return Conflict(new { Error = $"El cliente con ID {cliente.Id} ya existe" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _dataService.Clientes.Add(cliente.Id, cliente);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }

    /// <summary>
    /// Actualiza los datos de un cliente existente
    /// </summary>
    /// <param name="id">ID del cliente a actualizar</param>
    /// <param name="clienteActualizado">Nuevos datos del cliente</param>
    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] Cliente clienteActualizado)
    {
        if (id != clienteActualizado.Id)
            return BadRequest(new { Error = "El ID en la URL no coincide con el cuerpo de la solicitud" });

        if (!_dataService.Clientes.ContainsKey(id))
            return NotFound(new { Mensaje = $"Cliente con ID {id} no encontrado" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _dataService.Clientes[id] = clienteActualizado;
        return NoContent();
    }

    /// <summary>
    /// Elimina un cliente existente
    /// </summary>
    /// <param name="id">ID del cliente a eliminar</param>
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (!_dataService.Clientes.ContainsKey(id))
            return NotFound(new { Mensaje = $"Cliente con ID {id} no encontrado" });

        _dataService.Clientes.Remove(id);
        return NoContent();
    }
}