using AutoMapper;
using Application.Utilities.Models;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models.Contractors;

namespace Application.Utilities.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<ModelBase<T>, BaseMapperViewModel<T>> MapGlobalProperties<T>(
            this IMappingExpression<ModelBase<T>, BaseMapperViewModel<T>> mappingExpression)
        {
            return mappingExpression
                .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedBy.Id))
                .ForMember(dest => dest.CreatedByFullName, opt => opt.MapFrom(src => src.CreatedBy.Name))
                .ForMember(dest => dest.ModifiedById, opt => opt.MapFrom(src => src.ModifiedBy.Id))
                .ForMember(dest => dest.ModifiedByFullName, opt => opt.MapFrom(src => src.ModifiedBy.Name));
        }
    }
}
