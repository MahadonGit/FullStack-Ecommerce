using Ecommerce.Dtos.Auth;
using Ecommerce.Infrastructure.Constants;
using Ecommerce.Infrastructure.Exceptions;
using Ecommerce.Models;
using Ecommerce.Services.Interfaces;
using ECommerce.Data;
using ECommerce.Dtos.Auth;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace ECommerce.Services.Implementations
{
    public class AuthService : IAuthService

    {

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ITokenService _tokenService;

        private readonly IEmailService _emailService;

        private readonly ILogger<AuthService> _logger;
        
        private readonly AppDbContext _context;

        //cONSTRUCTOR FOR THE DEPENDENCY INJECTION
        public AuthService(
     UserManager<ApplicationUser> userManager,
     SignInManager<ApplicationUser> signInManager,
     RoleManager<IdentityRole> roleManager,
     ITokenService tokenService , IEmailService emailService , ILogger<AuthService> logger ,AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _logger = logger;
            _context = context;
        }

        //-------------------------------------------------------------------------------------------------------------------

        //Assinging the role
        public async Task AssignRoleAsync(AssignRoleDto dto)
        {

            //Find the user
            var user =await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception("User not found.");
            }
            //Check the Role exists or not 
            bool Exists = await _roleManager.RoleExistsAsync(dto.Role);
            if(!Exists)
            {
                throw new Exception("The role doesnt exists");
            }

            //Check if the role you are assiing is already assigned to the user 
            if (await _userManager.IsInRoleAsync(user, dto.Role))
            {
                throw new Exception("User already has this role.");
            }

            //If not assign role

            IdentityResult result = await _userManager.AddToRoleAsync(user, dto.Role);
            //Check result;
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }



        }

        //-------------------------------------------------------------------------------------------------------------------
        //Changing the password
        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            //Find the user

            var user = await _userManager.FindByNameAsync(userId);
            if (user == null)
            {
                throw new Exception("User does not found");
            }


            //now chagne pass using identity 

            IdentityResult result = await _userManager.ChangePasswordAsync(user,dto.CurrentPassword,dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors =
                    result.Errors
                          .Select(e => e.Description);

                throw new Exception(
                    string.Join(", ", errors));
            }

        }
        //-------------------------------------------------------------------------------------------------------------------
        public async Task ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new Exception("Please confirm your email before logging in.");
            }

            //Decode the token 
            string token = WebUtility.UrlDecode(dto.Token);
            //now confoim the mail 

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join(", ",
                        result.Errors.Select(e => e.Description)));
            }

        }
        //-------------------------------------------------------------------------------------------------------------------

        //Forgot password;
        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {

            //Find the user
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) { return; };  //this is user enumeration to save from hackers

            //generate the password reset token
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //we cannot senr dthetokeb by email directly but we encode it here
            token = WebUtility.UrlEncode(token);

            //we will build the reset link to show on frontend,,imaginary link is craeted 
            string resetLink = $"https://localhost:3000/reset-password?email={user.Email}&token={token}";

            //we are creating the mail body template to be send

               string body = $@"
                  <h2>Password Reset</h2>

                  <p>You requested a password reset.</p>

                  <p>
                     Click the link below to reset your password:
                  </p>

                  <a href='{resetLink}'>
                      Reset Password
                  </a>

                  <p>
                        If you didn't request this,
                        please ignore this email.
                  </p>";

            //now send the email

            await _emailService.SendEmailAsync(user.Email!, "Reset Password", body);
            //here we use email! beacuse we use the ? in email dto means we know and confirm our email is not null

        }

        //-------------------------------------------------------------------------------------------------------------------
        public Task<UserResponseDto> GetCurrentUserAsync(string userId)
        {
            throw new NotImplementedException();
        }


        //-------------------------------------------------------------------------------------------------------------------
        //Login Module:
        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            //Confirm the email 
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception("Invalid email or password.");

            }

            //Confirm the passwiord

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
            {
                throw new Exception("Invalid email or password.");
            }

            //Go to the roels and check the roles

            var roles = await _userManager.GetRolesAsync(user);

            //Now the genration of jwt token

            string token = await _tokenService.CreateTokenAsync(user);


            //Returen the response
            return new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Roles = roles
            };


        }

        //-------------------------------------------------------------------------------------------------------------------
        // Regiister Module
        // ------------------------------------------------------------
        // Register Module
        // ------------------------------------------------------------
        public async Task<UserResponseDto> RegisterAsync(RegisterDto dto)
        {
            // --------------------------------------------------------
            // 1. Log registration attempt
            // --------------------------------------------------------
            _logger.LogInformation(
                "Registration attempt for {Email}",
                dto.Email);


            // --------------------------------------------------------
            // 2. Check whether user already exists
            // --------------------------------------------------------
            var existingUser =
                await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                _logger.LogWarning(
                    "Registration failed. User with email {Email} already exists.",
                    dto.Email);

                throw new BadRequestException(
                    "User already exists.");
            }


            // --------------------------------------------------------
            // 3. Create Identity User
            // --------------------------------------------------------
            var user = new ApplicationUser
            {
                Email = dto.Email,
                FullName = dto.FullName,
                UserName = dto.Email
            };


            // --------------------------------------------------------
            // 4. Save Identity User
            // --------------------------------------------------------
            IdentityResult result =
                await _userManager.CreateAsync(
                    user,
                    dto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => e.Description);

                _logger.LogWarning(
                    "Registration failed for {Email}: {Errors}",
                    dto.Email,
                    string.Join(", ", errors));

                throw new BadRequestException(
                    string.Join(", ", errors));
            }


            // --------------------------------------------------------
            // 5. Assign Customer Role
            // --------------------------------------------------------
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    Roles.Customer);

            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors
                    .Select(e => e.Description);

                _logger.LogError(
                    "Failed to assign Customer role to {Email}: {Errors}",
                    dto.Email,
                    string.Join(", ", errors));

                throw new Exception(
                    string.Join(", ", errors));
            }


            // --------------------------------------------------------
            // 6. Create Customer record
            // --------------------------------------------------------
            var customer = new Customer
            {
                Name = user.FullName,
                ApplicationUserId = user.Id
            };


            // --------------------------------------------------------
            // 7. Save Customer record
            // --------------------------------------------------------
            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();


            // --------------------------------------------------------
            // 8. Generate Email Confirmation Token
            // --------------------------------------------------------
            string emailToken =
                await _userManager
                    .GenerateEmailConfirmationTokenAsync(user);


            // --------------------------------------------------------
            // 9. Log successful registration
            // --------------------------------------------------------
            _logger.LogInformation(
                "User {Email} registered successfully.",
                dto.Email);


            // --------------------------------------------------------
            // 10. Return User Response
            // --------------------------------------------------------
            return new UserResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = new List<string>
        {
            Roles.Customer
        }
            };
        }
           


        

        //-------------------------------------------------------------------------------------------------------------------
        public async Task ResendConfirmationAsync(ResendConfirmationDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return;
            }

            //check if it is already confirmed
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new Exception("Email is already confirmed.");
            }
            //now regengarte confirmantion token 

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            //now encode token;
            token = WebUtility.UrlEncode(token);

            //now send frontend confirmation link
            string confirmationLink =$"https://localhost:3000/confirm-email?email={user.Email}&token={token}";

            //Email Body to be send
            string body = $@"
                         <h2>Email Confirmation</h2>

                         <p>Please click the link below to confirm your email.</p>

                         <a href='{confirmationLink}'>
                         Confirm Email
                         </a>";


            //Send the email 
            await _emailService.SendEmailAsync( user.Email!, "Confirm Your Email", body);


        }

        //-------------------------------------------------------------------------------------------------------------------
        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            //Now decode the token

            string decodedToken = WebUtility.UrlDecode(dto.Token);
            //Now resetPasss

            IdentityResult result = await _userManager.ResetPasswordAsync(user, decodedToken,dto.NewPassword);

            //checking the identity results 
            if (!result.Succeeded)
            {
                var errors =
                    result.Errors
                          .Select(e => e.Description);

                throw new Exception(
                    string.Join(", ", errors));
            }




        }
        //-------------------------------------------------------------------------------------------------------------------
    }
}