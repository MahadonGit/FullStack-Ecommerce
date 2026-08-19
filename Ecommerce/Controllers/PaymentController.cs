using Ecommerce.Infrastructure;
using Ecommerce.Dtos.Payment;

using Ecommerce.Infrastructure.Constants;
using Ecommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        //--------------------------------------------------------------------
        // Create Payment (Customer)
        //--------------------------------------------------------------------

        [HttpPost]
        [Authorize(Roles = Roles.Customer)]
        public async Task<IActionResult> CreatePayment(CreatePaymentDto dto)
        {
            var result = await _paymentService.CreatePaymentAsync(dto);

            return Ok(result);
        }

        //--------------------------------------------------------------------
        // Get All Payments (Admin)
        //--------------------------------------------------------------------

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllPayments()
        {
            var result = await _paymentService.GetAllPaymentsAsync();

            return Ok(result);
        }

        //--------------------------------------------------------------------
        // Get Payment By Id (Admin)
        //--------------------------------------------------------------------

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);

            return Ok(result);
        }

        //--------------------------------------------------------------------
        // Update Payment Status (Admin)
        //--------------------------------------------------------------------

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdatePaymentStatus(
            int id,
            UpdatePaymentStatusDto dto)
        {
            await _paymentService.UpdatePaymentStatusAsync(id, dto);

            return Ok(new
            {
                Message = "Payment updated successfully."
            });
        }

        //--------------------------------------------------------------------
        // Delete Payment (Admin)
        //--------------------------------------------------------------------

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeletePayment(int id)
        {
            await _paymentService.DeletePaymentAsync(id);

            return Ok(new
            {
                Message = "Payment deleted successfully."
            });
        }
    }
}