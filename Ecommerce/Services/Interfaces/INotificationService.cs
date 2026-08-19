using Ecommerce.Dtos.Notification;

namespace Ecommerce.Services.Interfaces
{
    public interface INotificationService
    {
        // Create a notification
        Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto dto);

        // Get all notifications of a user
        Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(string userId);

        // Get notification by Id
        Task<NotificationResponseDto> GetNotificationByIdAsync(int id);

        // Mark notification as Read/Unread
        Task UpdateNotificationAsync(int id, UpdateNotificationDto dto);

        // Delete notification
        Task DeleteNotificationAsync(int id);
    }
}
