using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ModelLayer.DTO;
using RepositoryLayer.Entity;

namespace ModelLayer.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AddressBookEntry, AddressBookEntryDTO>().ReverseMap();
        }
    }

}
