using AutoMapper;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Application.Mappings;

/// <summary>
/// AutoMapper profile for MaintenanceRecord entity mappings.
/// </summary>
public class MaintenanceMappingProfile : Profile
{
    public MaintenanceMappingProfile()
    {
        // MaintenanceRecord -> MaintenanceRecordDto
        CreateMap<MaintenanceRecord, MaintenanceRecordDto>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Name : string.Empty))
            .ForMember(dest => dest.EquipmentCode,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Code : string.Empty))
            .ForMember(dest => dest.IsOverdue,
                opt => opt.MapFrom(src => src.IsOverdue))
            .ForMember(dest => dest.Duration,
                opt => opt.MapFrom(src => src.Duration));

        // MaintenanceRecord -> MaintenanceRecordDetailDto
        CreateMap<MaintenanceRecord, MaintenanceRecordDetailDto>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Name : string.Empty))
            .ForMember(dest => dest.EquipmentCode,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Code : string.Empty))
            .ForMember(dest => dest.ProductionLineName,
                opt => opt.MapFrom(src => src.Equipment != null && src.Equipment.ProductionLine != null
                    ? src.Equipment.ProductionLine.Name
                    : string.Empty))
            .ForMember(dest => dest.IsOverdue,
                opt => opt.MapFrom(src => src.IsOverdue))
            .ForMember(dest => dest.Duration,
                opt => opt.MapFrom(src => src.Duration));
    }
}
