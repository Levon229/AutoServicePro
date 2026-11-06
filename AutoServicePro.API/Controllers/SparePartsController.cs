using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoServicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SparePartsController(ISparePartService sparePartService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SparePartResponseDto>> Create([FromBody] CreateSparePartDto dto)
    {
        var result = await sparePartService.CreateSparePartAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SparePartResponseDto>>> GetAll()
    {
        return Ok(await sparePartService.GetAllSparePartsAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SparePartResponseDto>> GetById(int id)
    {
        return Ok(await sparePartService.GetSparePartByIdAsync(id));
    }
}