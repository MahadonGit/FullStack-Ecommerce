namespace Ecommerce.Dtos.Auth
{
    public class LoginResponseDto
    {

        public string Token { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;


        public IList<String> Roles { get; set; } = new List<string>();
    }
}
