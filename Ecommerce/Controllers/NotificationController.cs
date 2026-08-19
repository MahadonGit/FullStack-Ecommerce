using Ecommerce.Dtos.Notification;
using Ecommerce.Infrastructure.Constants;
using Ecommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        //--------------------------------------------------------------------
        // Create Notification (Admin Only)
        //--------------------------------------------------------------------

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> CreateNotification(CreateNotificationDto dto)
        {
            var result = await _notificationService.CreateNotificationAsync(dto);

            return Ok(result);
        }

        //--------------------------------------------------------------------
        // Get Logged-in User Notifications
        //--------------------------------------------------------------------

        [HttpGet("my")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _notificationService.GetUserNotificationsAsync(userId);

            return Ok(result);
        }

        //--------------------------------------------------------------------
        // Get Notification By Id
        //--------------------------------------------------------------------

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            var result = await _notificationService.GetNotificationByIdAsync(id);

            return Ok(result);
        }

        //--------------------------------------------------------------------
        // Mark Notification Read/Unread
        //--------------------------------------------------------------------

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(
            int id,
            UpdateNotificationDto dto)
        {
            await _notificationService.UpdateNotificationAsync(id, dto);

            return Ok(new
            {
                Message = "Notification updated successfully."
            });
        }

        //--------------------------------------------------------------------
        // Delete Notification
        //--------------------------------------------------------------------

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);

            return Ok(new
            {
                Message = "Notification deleted successfully."
            });
        }


























        //End

    }
}
