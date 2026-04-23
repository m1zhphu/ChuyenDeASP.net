using SmartGarage.DTOs;

namespace SmartGarage.Interface
{
    public interface ICheckInService
    {
        Task<CheckInResponseDTO> ProcessCheckInAsync(string licensePlate);
        Task<CheckInResponseDTO> QuickOnboardAsync(QuickOnboardRequestDTO request);
    }
}