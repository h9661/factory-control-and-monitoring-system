using AutoMapper;
using SmartFactory.Application.DTOs.SensorData;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Application.Mappings;

/// <summary>
/// AutoMapper profile for SensorData entity mappings.
/// </summary>
public class SensorDataMappingProfile : Profile
{
    public SensorDataMappingProfile()
    {
        // SensorData -> SensorDataDto
        CreateMap<SensorData, SensorDataDto>();

        // SensorData -> LatestSensorValueDto
        CreateMap<SensorData, LatestSensorValueDto>();
    }
}
