using AutoMapper;
using TransactionsProcessor.Entities;
using TransactionsProcessor.Models;

namespace TransactionsProcessor.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TransactionRequest, Transaction>()
                .ReverseMap();
        }
    }
}
