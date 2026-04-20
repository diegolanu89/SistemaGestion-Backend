using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/potencial-clients")]
public class PotencialClientsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PotencialClientsController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/potencial-clients
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _db.PotencialClients
            .OrderBy(c => c.Name)
            .Select(c => new PotencialClientDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return Ok(clients);
    }

    // GET api/potencial-clients/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var client = await _db.PotencialClients.FindAsync(id);

        if (client == null)
            return NotFound(new { message = "Cliente potencial no encontrado" });

        return Ok(new PotencialClientDto
        {
            Id = client.Id,
            Name = client.Name,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        });
    }

    // POST api/potencial-clients
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PotencialClientRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return UnprocessableEntity(new { message = "El nombre es requerido" });

        var client = new PotencialClient
        {
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.PotencialClients.Add(client);
        await _db.SaveChangesAsync();

        return StatusCode(201, new PotencialClientDto
        {
            Id = client.Id,
            Name = client.Name,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        });
    }

    // PUT api/potencial-clients/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] PotencialClientRequestDto dto)
    {
        var client = await _db.PotencialClients.FindAsync(id);

        if (client == null)
            return NotFound(new { message = "Cliente potencial no encontrado" });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return UnprocessableEntity(new { message = "El nombre es requerido" });

        client.Name = dto.Name;
        client.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new PotencialClientDto
        {
            Id = client.Id,
            Name = client.Name,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        });
    }

    // DELETE api/potencial-clients/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var client = await _db.PotencialClients.FindAsync(id);

        if (client == null)
            return NotFound(new { message = "Cliente potencial no encontrado" });

        _db.PotencialClients.Remove(client);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Cliente potencial eliminado" });
    }
}
