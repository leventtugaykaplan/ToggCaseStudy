using UserPortal.Entities;

namespace UserPortal.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
