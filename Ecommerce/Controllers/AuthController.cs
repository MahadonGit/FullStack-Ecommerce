using Ecommerce.Dtos.Auth;
using Ecommerce.Infrastructure.Constants;
using Ecommerce.Models;
using Ecommerce.Services.Implementations;
using Ecommerce.Services.Interfaces;
using ECommerce.Dtos.Auth;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        //Register service

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var response =
                await _authService.RegisterAsync(dto);

            return Ok(response);
        }

        //Login service

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var response =
                await _authService.LoginAsync(dto);

            return Ok(response);
        }

        //Change password

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _authService.ChangePasswordAsync(
                userId,
                dto);

            return Ok(new
            {
                Message = "Password changed successfully."
            });


        }

        //Forgot password
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto);

            return Ok(new
            {
                Message = "If an account with that email exists, a password reset link has been sent."
            });
        }

        //Reset password
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);

            return Ok(new
            {
                Message = "Password has been reset successfully."
            });
        }

        //confirm the email;
        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto dto)
        {
            await _authService.ConfirmEmailAsync(dto);

            return Ok(new
            {
                Message = "Email confirmed successfully."
            });
        }

        //Resend Confirmation
        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmation(
    ResendConfirmationDto dto)
        {
            await _authService.ResendConfirmationAsync(dto);

            return Ok(new
            {
                Message = "If the account exists and is not already confirmed, a confirmation email has been sent."
            });
        }

        //Assign role 

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(
    AssignRoleDto dto)
        {
            await _authService.AssignRoleAsync(dto);

            return Ok(new
            {
                Message = "Role assigned successfully."
            });
        }



        //End of the program
    }


}
