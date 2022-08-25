namespace UserPortal.Services.AsyncDataServices.Consts
{
    internal static class RabbitMqConst
    {
        internal static readonly string createUser = "create_user_queue";
        internal static readonly string userActivision = "user_activision_queue";
        internal static readonly string userCreateExchange = "user_create_exchange";
    }
}
