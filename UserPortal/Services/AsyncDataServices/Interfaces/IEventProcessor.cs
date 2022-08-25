namespace UserPortal.Services.AsyncDataServices.Interfaces
{
    public interface IEventProcessor
    {
        void ProcessEvent(string message);
    }
}
