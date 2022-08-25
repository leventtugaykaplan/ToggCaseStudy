using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using UserPortal.Data.Repositories;
using UserPortal.Dtos;
using UserPortal.Entities;
using UserPortal.Enums;
using UserPortal.Services.AsyncDataServices.Interfaces;

namespace UserPortal.Services.AsyncDataServices.Services
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(
            IServiceScopeFactory scopeFactory,
            IMapper mapper)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            switch (eventType)
            {
                case SubscriberEventType.UserActivision:
                    ChangeUserActivisionStatus(message);
                    break;
                default:
                    break;
            }
        }

        private SubscriberEventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("--> Determine Event");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (eventType.Event)
            {
                case "User_Activision":
                    Console.WriteLine("--> User Activision Event Detected");
                    return SubscriberEventType.UserActivision;
                default:
                    Console.WriteLine("--> Could not determine the event type");
                    return SubscriberEventType.Undetermined;
            }
        }

        private void ChangeUserActivisionStatus(string userActivisionMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<UserPortalRepository<User>>();

                var userActivisionDto = JsonSerializer.Deserialize<UserActivisionDto>(userActivisionMessage);

                try
                {
                    var user = userRepository.FindById(userActivisionDto.Id);
                    if (user is not null)
                    {
                        user.IsActive = userActivisionDto.IsActive;

                        userRepository.Update(user);
                        userRepository.SaveChanges();
                        Console.WriteLine("--> User activision changed!");
                    }
                    else
                    {
                        Console.WriteLine("--> User could not find...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not change activision status : {ex.Message}");
                }
            }
        }
    }
}
