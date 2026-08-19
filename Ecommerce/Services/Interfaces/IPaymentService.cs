using Ecommerce.Dtos.Payment;
namespace Ecommerce.Services.Interfaces
{
    public interface IPaymentService
    {

        Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto);

        Task<PaymentResponseDto> GetPaymentByIdAsync(int id);

        Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync();

        Task UpdatePaymentStatusAsync(int id, UpdatePaymentStatusDto dto);

        Task DeletePaymentAsync(int id);
    }
}
