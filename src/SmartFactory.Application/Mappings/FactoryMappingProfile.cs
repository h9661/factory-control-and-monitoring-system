using AutoMapper;
using SmartFactory.Application.DTOs.Factory;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Application.Mappings;

/// <summary>
/// AutoMapper profile for Factory and ProductionLine entity mappings.
/// </summary>
public class FactoryMappingProfile : Profile
{
    public FactoryMappingProfile()
    {
        // Factory -> FactoryDto
        CreateMap<Factory, FactoryDto>()
            .ForMember(dest => dest.ProductionLineCount,
                opt => opt.MapFrom(src => src.ProductionLines != null ? src.ProductionLines.Count : 0))
            .ForMember(dest => dest.EquipmentCount,
                opt => opt.MapFrom(src => src.ProductionLines != null
                    ? src.ProductionLines.Sum(pl => pl.Equipment != null ? pl.Equipment.Count : 0)
                    : 0));

        // Factory -> FactoryDetailDto
        CreateMap<Factory, FactoryDetailDto>()
            .ForMember(dest => dest.ProductionLineCount,
                opt => opt.MapFrom(src => src.ProductionLines != null ? src.ProductionLines.Count : 0))
            .ForMember(dest => dest.EquipmentCount,
                opt => opt.MapFrom(src => src.ProductionLines != null
                    ? src.ProductionLines.Sum(pl => pl.Equipment != null ? pl.Equipment.Count : 0)
                    : 0))
            .ForMember(dest => dest.ProductionLines,
                opt => opt.MapFrom(src => src.ProductionLines));

        // ProductionLine -> ProductionLineDto
        CreateMap<ProductionLine, ProductionLineDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.EquipmentCount,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Count : 0));
    }
}
