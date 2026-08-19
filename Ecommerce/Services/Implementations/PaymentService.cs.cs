using Ecommerce.Dtos.Payment;
using Ecommerce.Enums;
using Ecommerce.Models;
using Ecommerce.Services.Interfaces;
using ECommerce.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Implementations
{
    public class PaymentService : IPaymentService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            AppDbContext context,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }



        //Implement CreatePaymentAsync()

        public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto)
        {
            _logger.LogInformation( "Creating payment for OrderId {OrderId}", dto.OrderId);

            var payment = new Payment
            {
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow,
                Remarks = dto.Remarks

            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();


            _logger.LogInformation("Payment {PaymentId} created successfully.", payment.Id);

            return new PaymentResponseDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId,
                PaymentDate = payment.PaymentDate,
                Remarks = payment.Remarks
            };

        }

        //GetPaymentByIdAsync()
        public async Task<PaymentResponseDto> GetPaymentByIdAsync(int id)
        {
            _logger.LogInformation( "Fetching payment with Id {PaymentId}", id);

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                _logger.LogWarning( "Payment with Id {PaymentId} was not found.", id);

                throw new Exception("Payment not found.");
            }

            return new PaymentResponseDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId,
                PaymentDate = payment.PaymentDate,
                Remarks = payment.Remarks
            };
        }


        //GetAllPaymentsAsync()

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync()
        {
            _logger.LogInformation("Fetching all payments.");

            var payments = await _context.Payments.
                OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(payment => new PaymentResponseDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId,
                PaymentDate = payment.PaymentDate,
                Remarks = payment.Remarks
            });
        }

        //UpdatePaymentStatusAsync()
        public async Task UpdatePaymentStatusAsync(int id, UpdatePaymentStatusDto dto)
        {
            _logger.LogInformation("Updating payment status for Payment Id {PaymentId}",id);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                _logger.LogWarning( "Payment with Id {PaymentId} was not found.",id);

                throw new Exception("Payment not found.");
            }

            payment.PaymentStatus = dto.PaymentStatus;
            payment.TransactionId = dto.TransactionId;
            payment.Remarks = dto.Remarks;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment Id {PaymentId} updated successfully.",id);
        }

        //Remove

        public async Task DeletePaymentAsync(int id)
        {
            _logger.LogInformation("Deleting payment with Id {PaymentId}", id);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                _logger.LogWarning("Payment with Id {PaymentId} was not found.",id);

                throw new Exception("Payment not found.");
            }

            _context.Payments.Remove(payment);

            await _context.SaveChangesAsync();

            _logger.LogInformation( "Payment with Id {PaymentId} deleted successfully.", id);
        }








        //ENd


    }
}
