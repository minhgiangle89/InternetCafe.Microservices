using AccountService.Application.DTOs.Account;
using AccountService.Application.DTOs.Transaction;
using AccountService.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Account mappings
            CreateMap<Account, AccountDTO>();
            CreateMap<Account, AccountDetailsDTO>();

            // Transaction mappings
            CreateMap<Transaction, TransactionDTO>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
                .ForMember(dest => dest.PaymentMethod, opt =>
                    opt.MapFrom(src => src.PaymentMethod.HasValue ? (int)src.PaymentMethod.Value : (int?)null))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Creation_Timestamp));
        }
    }
}
