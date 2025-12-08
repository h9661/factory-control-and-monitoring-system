using AutoMapper;
using SmartFactory.Application.DTOs.Quality;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Application.Mappings;

/// <summary>
/// AutoMapper profile for QualityRecord entity mappings.
/// </summary>
public class QualityMappingProfile : Profile
{
    public QualityMappingProfile()
    {
        // QualityRecord -> QualityRecordDto
        CreateMap<QualityRecord, QualityRecordDto>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Name : string.Empty))
            .ForMember(dest => dest.WorkOrderNumber,
                opt => opt.MapFrom(src => src.WorkOrder != null ? src.WorkOrder.OrderNumber : null))
            .ForMember(dest => dest.DefectRate,
                opt => opt.MapFrom(src => src.DefectRate));

        // QualityRecord -> QualityRecordDetailDto
        CreateMap<QualityRecord, QualityRecordDetailDto>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Name : string.Empty))
            .ForMember(dest => dest.WorkOrderNumber,
                opt => opt.MapFrom(src => src.WorkOrder != null ? src.WorkOrder.OrderNumber : null))
            .ForMember(dest => dest.ProductionLineName,
                opt => opt.MapFrom(src => src.Equipment != null && src.Equipment.ProductionLine != null
                    ? src.Equipment.ProductionLine.Name
                    : string.Empty))
            .ForMember(dest => dest.DefectRate,
                opt => opt.MapFrom(src => src.DefectRate));
    }
}
