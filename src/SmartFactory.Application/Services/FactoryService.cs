using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Factory;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Interfaces;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service implementation for factory operations.
/// </summary>
public class FactoryService : IFactoryService
{
    private readonly IFactoryRepository _factoryRepository;
    private readonly IProductionLineRepository _productionLineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<FactoryCreateDto> _factoryCreateValidator;
    private readonly IValidator<FactoryUpdateDto> _factoryUpdateValidator;
    private readonly IValidator<ProductionLineCreateDto> _lineCreateValidator;
    private readonly IValidator<ProductionLineUpdateDto> _lineUpdateValidator;
    private readonly ILogger<FactoryService> _logger;

    public FactoryService(
        IFactoryRepository factoryRepository,
        IProductionLineRepository productionLineRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<FactoryCreateDto> factoryCreateValidator,
        IValidator<FactoryUpdateDto> factoryUpdateValidator,
        IValidator<ProductionLineCreateDto> lineCreateValidator,
        IValidator<ProductionLineUpdateDto> lineUpdateValidator,
        ILogger<FactoryService> logger)
    {
        _factoryRepository = factoryRepository;
        _productionLineRepository = productionLineRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _factoryCreateValidator = factoryCreateValidator;
        _factoryUpdateValidator = factoryUpdateValidator;
        _lineCreateValidator = lineCreateValidator;
        _lineUpdateValidator = lineUpdateValidator;
        _logger = logger;
    }

    public async Task<IEnumerable<FactoryDto>> GetAllFactoriesAsync(CancellationToken cancellationToken = default)
    {
        var factories = await _factoryRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<FactoryDto>>(factories);
    }

    public async Task<IEnumerable<FactoryDto>> GetActiveFactoriesAsync(CancellationToken cancellationToken = default)
    {
        var factories = await _factoryRepository.GetActiveFactoriesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<FactoryDto>>(factories);
    }

    public async Task<FactoryDetailDto?> GetFactoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var factory = await _factoryRepository.GetWithProductionLinesAsync(id, cancellationToken);
        return factory != null ? _mapper.Map<FactoryDetailDto>(factory) : null;
    }

    public async Task<FactoryDto> CreateFactoryAsync(FactoryCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _factoryCreateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        // Check for duplicate code
        if (await _factoryRepository.CodeExistsAsync(dto.Code, cancellationToken))
            throw DuplicateEntityException.For<Factory>("Code", dto.Code);

        var factory = new Factory(dto.Code, dto.Name, dto.Location);

        if (!string.IsNullOrEmpty(dto.Description))
            factory.Update(dto.Name, dto.Location, dto.Description);

        if (!string.IsNullOrEmpty(dto.TimeZone))
            factory.SetTimeZone(dto.TimeZone);

        if (!string.IsNullOrEmpty(dto.ContactEmail) || !string.IsNullOrEmpty(dto.ContactPhone))
            factory.SetContactInfo(dto.ContactEmail, dto.ContactPhone);

        await _factoryRepository.AddAsync(factory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created factory {FactoryCode} - {FactoryName}", dto.Code, dto.Name);

        return _mapper.Map<FactoryDto>(factory);
    }

    public async Task UpdateFactoryAsync(Guid id, FactoryUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _factoryUpdateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var factory = await _factoryRepository.GetByIdAsync(id, cancellationToken);
        if (factory == null)
            throw NotFoundException.For<Factory>(id);

        factory.Update(dto.Name, dto.Location, dto.Description);

        if (!string.IsNullOrEmpty(dto.TimeZone))
            factory.SetTimeZone(dto.TimeZone);

        factory.SetContactInfo(dto.ContactEmail, dto.ContactPhone);

        if (dto.IsActive)
            factory.Activate();
        else
            factory.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated factory {FactoryId}", id);
    }

    public async Task DeleteFactoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var factory = await _factoryRepository.GetByIdAsync(id, cancellationToken);
        if (factory == null)
            throw NotFoundException.For<Factory>(id);

        // Soft delete
        factory.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated factory {FactoryId}", id);
    }

    public async Task<IEnumerable<ProductionLineDto>> GetProductionLinesAsync(Guid factoryId, CancellationToken cancellationToken = default)
    {
        var factory = await _factoryRepository.GetByIdAsync(factoryId, cancellationToken);
        if (factory == null)
            throw NotFoundException.For<Factory>(factoryId);

        var productionLines = await _productionLineRepository.GetByFactoryAsync(factoryId, cancellationToken);
        return _mapper.Map<IEnumerable<ProductionLineDto>>(productionLines);
    }

    public async Task<ProductionLineDto> CreateProductionLineAsync(ProductionLineCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _lineCreateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var factory = await _factoryRepository.GetByIdAsync(dto.FactoryId, cancellationToken);
        if (factory == null)
            throw NotFoundException.For<Factory>(dto.FactoryId);

        // Check for duplicate code within factory
        if (await _productionLineRepository.CodeExistsAsync(dto.FactoryId, dto.Code, cancellationToken: cancellationToken))
            throw DuplicateEntityException.For<ProductionLine>("Code", dto.Code);

        var line = new ProductionLine(dto.FactoryId, dto.Code, dto.Name);

        if (dto.DesignedCapacity > 0 || !string.IsNullOrEmpty(dto.Description))
            line.Update(dto.Name, dto.Description, dto.DesignedCapacity);

        if (dto.Sequence > 0)
            line.SetSequence(dto.Sequence);

        await _productionLineRepository.AddAsync(line, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created production line {LineCode} for factory {FactoryId}", dto.Code, dto.FactoryId);

        return _mapper.Map<ProductionLineDto>(line);
    }

    public async Task UpdateProductionLineAsync(Guid id, ProductionLineUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _lineUpdateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var line = await _productionLineRepository.GetByIdAsync(id, cancellationToken);
        if (line == null)
            throw NotFoundException.For<ProductionLine>(id);

        line.Update(dto.Name, dto.Description, dto.DesignedCapacity);

        if (dto.Sequence > 0)
            line.SetSequence(dto.Sequence);

        if (dto.IsActive)
            line.Activate();
        else
            line.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated production line {ProductionLineId}", id);
    }

    public async Task DeleteProductionLineAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var line = await _productionLineRepository.GetByIdAsync(id, cancellationToken);
        if (line == null)
            throw NotFoundException.For<ProductionLine>(id);

        // Soft delete
        line.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated production line {ProductionLineId}", id);
    }
}
