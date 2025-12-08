using AutoMapper;
using SmartFactory.Application.DTOs.Alarm;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Application.Mappings;

/// <summary>
/// AutoMapper profile for Alarm entity mappings.
/// </summary>
public class AlarmMappingProfile : Profile
{
    public AlarmMappingProfile()
    {
        // Alarm -> AlarmDto
        CreateMap<Alarm, AlarmDto>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Name : string.Empty))
            .ForMember(dest => dest.TimeElapsed,
                opt => opt.MapFrom(src => src.IsResolved
                    ? src.TimeToResolve
                    : (DateTime.UtcNow - src.OccurredAt)));

        // Alarm -> AlarmDetailDto
        CreateMap<Alarm, AlarmDetailDto>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Name : string.Empty))
            .ForMember(dest => dest.ProductionLineName,
                opt => opt.MapFrom(src => src.Equipment != null && src.Equipment.ProductionLine != null
                    ? src.Equipment.ProductionLine.Name
                    : string.Empty))
            .ForMember(dest => dest.TimeElapsed,
                opt => opt.MapFrom(src => src.IsResolved
                    ? src.TimeToResolve
                    : (DateTime.UtcNow - src.OccurredAt)));
    }
}
