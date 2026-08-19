using Ecommerce.Models;

namespace Ecommerce.Services.Interfaces
{
    public interface ITokenService
    {

        Task<string> CreateTokenAsync(ApplicationUser user);

    }
}
