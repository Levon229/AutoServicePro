using AutoServicePro.Application.DTOs;
using AutoServicePro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoServicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrdersController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrderResponseDto>> Create([FromBody] CreateWorkOrderDto dto)
    {
        var result = await _workOrderService.CreateWorkOrderAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkOrderResponseDto>> GetById(int id)
    {
        var result = await _workOrderService.GetWorkOrderByIdAsync(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrderResponseDto>>> GetAll()
    {
        var results = await _workOrderService.GetAllWorkOrdersAsync();
        return Ok(results);
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<WorkOrderResponseDto>> Complete(int id)
    {
        var result = await _workOrderService.CompleteWorkOrderAsync(id);
        return Ok(result);
    }
}