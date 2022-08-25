using UserPortal.Dtos;

namespace UserPortal.Services.AsyncDataServices.Interfaces
{
    public interface IMessageBusClient
    {
        void PublishNewUser(UserCreatedDto userCreatedDto);
    }
}
