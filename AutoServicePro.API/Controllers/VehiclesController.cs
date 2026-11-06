using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoServicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<VehicleResponseDto>> Create([FromBody] CreateVehicleDto dto)
    {
        var result = await vehicleService.CreateVehicleAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleResponseDto>>> GetAll()
    {
        return Ok(await vehicleService.GetAllVehiclesAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleResponseDto>> GetById(int id)
    {
        return Ok(await vehicleService.GetVehicleByIdAsync(id));
    }
}