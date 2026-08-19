using Ecommerce.Dtos.Auth;
using ECommerce.Dtos;
using Microsoft.AspNetCore.Identity;
using Ecommerce.Models;
using ECommerce.Dtos.Auth;

namespace ECommerce.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync( 
            RegisterDto dto);

        Task<LoginResponseDto> LoginAsync(
            LoginDto dto);

        Task ChangePasswordAsync(
            string userId,
            ChangePasswordDto dto);

        Task ForgotPasswordAsync(
            ForgotPasswordDto dto);

        Task ResetPasswordAsync(
            ResetPasswordDto dto);

        Task ConfirmEmailAsync(
            ConfirmEmailDto dto);

        Task ResendConfirmationAsync(
            ResendConfirmationDto dto);

        Task AssignRoleAsync(
            AssignRoleDto dto);

        Task<UserResponseDto> GetCurrentUserAsync(
            string userId);
    }
}