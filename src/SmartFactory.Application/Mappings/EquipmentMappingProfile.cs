using AutoMapper;
using SmartFactory.Application.DTOs.Equipment;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Application.Mappings;

/// <summary>
/// AutoMapper profile for Equipment entity mappings.
/// </summary>
public class EquipmentMappingProfile : Profile
{
    public EquipmentMappingProfile()
    {
        // Equipment -> EquipmentDto
        CreateMap<Equipment, EquipmentDto>()
            .ForMember(dest => dest.ProductionLineName,
                opt => opt.MapFrom(src => src.ProductionLine != null ? src.ProductionLine.Name : string.Empty))
            .ForMember(dest => dest.IsOnline,
                opt => opt.MapFrom(src => src.IsOnline))
            .ForMember(dest => dest.IsMaintenanceDue,
                opt => opt.MapFrom(src => src.IsMaintenanceDue()));

        // Equipment -> EquipmentDetailDto
        CreateMap<Equipment, EquipmentDetailDto>()
            .ForMember(dest => dest.ProductionLineName,
                opt => opt.MapFrom(src => src.ProductionLine != null ? src.ProductionLine.Name : string.Empty))
            .ForMember(dest => dest.IsOnline,
                opt => opt.MapFrom(src => src.IsOnline))
            .ForMember(dest => dest.IsMaintenanceDue,
                opt => opt.MapFrom(src => src.IsMaintenanceDue()));

        // EquipmentCreateDto -> Equipment (manual creation in service)
        // EquipmentUpdateDto -> Equipment (manual update in service)
    }
}
