using ComputerSessionService.Application.DTOs.Computer;
using ComputerSessionService.Domain.Entities;
using AutoMapper;
using ComputerSessionService.Application.DTOs.Session;
using ComputerSessionService.Application.DTOs.Transaction;


namespace ComputerSessionService.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Computer mappings
            CreateMap<Computer, ComputerDTO>();
            CreateMap<Computer, ComputerDetailsDTO>();

            // Session mappings
            CreateMap<Session, SessionDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<Session, SessionDetailsDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<Session, SessionSummaryDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));
        }
    }
}
