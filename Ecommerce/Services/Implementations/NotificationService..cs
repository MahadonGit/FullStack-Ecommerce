using Ecommerce.Dtos.Notification;
using Ecommerce.Models;
using Ecommerce.Services.Interfaces;
using ECommerce.Data;
using Microsoft.EntityFrameworkCore;
namespace Ecommerce.Services.Implementations
{
    public class NotificationService :INotificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IEmailService _emailService;

        public NotificationService(
            AppDbContext context,
            ILogger<NotificationService> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        //Create the noticfiactuon method

        public async Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto dto)
        {
            _logger.LogInformation(
                "Creating notification for user {UserId}",
                dto.UserId);

            var notification = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                IsRead = false
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Notification {NotificationId} created successfully.",
                notification.Id);

            return new NotificationResponseDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        //second user notifuication 

        public async Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(string userId)
        {
            _logger.LogInformation(
                "Fetching notifications for user {UserId}",
                userId);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(notification => new NotificationResponseDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            });
        }

        //By id

        public async Task<NotificationResponseDto> GetNotificationByIdAsync(int id)
        {
            _logger.LogInformation(
                "Fetching notification with Id {NotificationId}",
                id);

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                _logger.LogWarning(
                    "Notification with Id {NotificationId} was not found.",
                    id);

                throw new Exception("Notification not found.");
            }

            return new NotificationResponseDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
        //update notification if users read

        public async Task UpdateNotificationAsync(int id, UpdateNotificationDto dto)
        {
            _logger.LogInformation(
                "Updating notification with Id {NotificationId}",
                id);

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                _logger.LogWarning(
                    "Notification with Id {NotificationId} not found.",
                    id);

                throw new Exception("Notification not found.");
            }

            notification.IsRead = dto.IsRead;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Notification with Id {NotificationId} updated successfully.",
                id);
        }

        //Delete
        public async Task DeleteNotificationAsync(int id)
        {
            _logger.LogInformation(
                "Deleting notification with Id {NotificationId}",
                id);

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                _logger.LogWarning(
                    "Notification with Id {NotificationId} not found.",
                    id);

                throw new Exception("Notification not found.");
            }

            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Notification with Id {NotificationId} deleted successfully.",
                id);
        }



        //End
    }
}
