using AutoMapper;
using UserPortal.Dtos;
using UserPortal.Entities;

namespace UserPortal.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Source -> Target
            CreateMap<User, UserCreatedDto>()
                .ForMember(
                    dest => dest.Event,
                    opt => opt.Ignore());
            CreateMap<UserActivisionDto, User>();
        }
    }
}
