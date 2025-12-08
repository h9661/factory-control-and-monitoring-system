using AutoMapper;
using SmartFactory.Application.DTOs.WorkOrder;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Application.Mappings;

/// <summary>
/// AutoMapper profile for WorkOrder entity mappings.
/// </summary>
public class WorkOrderMappingProfile : Profile
{
    public WorkOrderMappingProfile()
    {
        // WorkOrder -> WorkOrderDto
        CreateMap<WorkOrder, WorkOrderDto>()
            .ForMember(dest => dest.YieldRate,
                opt => opt.MapFrom(src => src.YieldRate))
            .ForMember(dest => dest.CompletionPercentage,
                opt => opt.MapFrom(src => src.CompletionPercentage))
            .ForMember(dest => dest.IsOnSchedule,
                opt => opt.MapFrom(src => src.IsOnSchedule));

        // WorkOrder -> WorkOrderDetailDto
        CreateMap<WorkOrder, WorkOrderDetailDto>()
            .ForMember(dest => dest.FactoryName,
                opt => opt.MapFrom(src => src.Factory != null ? src.Factory.Name : string.Empty))
            .ForMember(dest => dest.YieldRate,
                opt => opt.MapFrom(src => src.YieldRate))
            .ForMember(dest => dest.CompletionPercentage,
                opt => opt.MapFrom(src => src.CompletionPercentage))
            .ForMember(dest => dest.IsOnSchedule,
                opt => opt.MapFrom(src => src.IsOnSchedule))
            .ForMember(dest => dest.Steps,
                opt => opt.MapFrom(src => src.Steps));

        // WorkOrderStep -> WorkOrderStepDto
        CreateMap<WorkOrderStep, WorkOrderStepDto>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Name : string.Empty));
    }
}
