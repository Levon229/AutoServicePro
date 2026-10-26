using AutoServicePro.Application.DTOs;

namespace AutoServicePro.Application.Interfaces;

public interface IWorkOrderService
{
    Task<WorkOrderResponseDto> CreateWorkOrderAsync(CreateWorkOrderDto dto);
    Task<IEnumerable<WorkOrderResponseDto>> GetAllWorkOrdersAsync();
    Task<WorkOrderResponseDto> CompleteWorkOrderAsync(int workOrderId);
}