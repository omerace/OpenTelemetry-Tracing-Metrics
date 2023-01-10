using App3.WebApi.Domain.Models;
using App3.WebApi.WebApi.Dtos.Category;
using AutoMapper;

namespace App3.WebApi.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Category, CategoryAddDto>().ReverseMap();
            CreateMap<Category, CategoryEditDto>().ReverseMap();
            CreateMap<Category, CategoryResultDto>().ReverseMap();           
        }
    }
}
