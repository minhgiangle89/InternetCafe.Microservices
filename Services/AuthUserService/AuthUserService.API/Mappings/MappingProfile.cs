using AuthUserService.Application.DTOs.User;
using AuthUserService.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AuthUserService.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity to DTO
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.Creation_Timestamp));

            // DTO to Entity
            CreateMap<CreateUserDTO, User>();
            CreateMap<UpdateUserDTO, User>();
        }
    }
}
