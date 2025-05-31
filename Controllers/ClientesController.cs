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

    // GET: api/clientes
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_dataService.Clientes.Values.ToList());
    }

    // GET api/clientes/1
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        if (_dataService.Clientes.TryGetValue(id, out var cliente))
            return Ok(cliente);

        return NotFound();
    }

    // POST api/clientes
    [HttpPost]
    public IActionResult Create([FromBody] Cliente cliente)
    {
        _dataService.Clientes.Add(cliente.Id, cliente);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }
}