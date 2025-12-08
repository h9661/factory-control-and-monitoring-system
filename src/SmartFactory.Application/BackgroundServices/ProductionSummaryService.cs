using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Application.Events;
using SmartFactory.Application.Interfaces;

namespace SmartFactory.Application.BackgroundServices;

/// <summary>
/// Background service that updates production summary and publishes events.
/// </summary>
public class ProductionSummaryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<ProductionSummaryService> _logger;
    private readonly PollingOptions _options;

    public ProductionSummaryService(
        IServiceProvider serviceProvider,
        IEventAggregator eventAggregator,
        IOptions<PollingOptions> options,
        ILogger<ProductionSummaryService> logger)
    {
        _serviceProvider = serviceProvider;
        _eventAggregator = eventAggregator;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Production Summary Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateProductionSummaryAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating production summary");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.ProductionSummaryIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Production Summary Service stopped");
    }

    private async Task UpdateProductionSummaryAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var workOrderService = scope.ServiceProvider.GetRequiredService<IWorkOrderService>();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

        // Get production summary for today
        var summary = await workOrderService.GetProductionSummaryAsync(null, DateTime.Today, cancellationToken);

        // Get OEE data
        var oeeData = await reportService.GenerateOeeReportAsync(null, DateTime.Today, DateTime.Today, cancellationToken);

        _logger.LogDebug(
            "Production summary: {Total} work orders, {Completed} completed, {InProgress} in progress",
            summary.TotalWorkOrders, summary.CompletedWorkOrders, summary.InProgressWorkOrders);

        await _eventAggregator.PublishAsync(new ProductionSummaryUpdatedEvent
        {
            TotalWorkOrders = summary.TotalWorkOrders,
            CompletedWorkOrders = summary.CompletedWorkOrders,
            InProgressWorkOrders = summary.InProgressWorkOrders,
            TotalTargetUnits = summary.TotalTargetUnits,
            TotalCompletedUnits = summary.TotalCompletedUnits,
            OeeScore = oeeData.OverallOee
        }, cancellationToken);
    }
}
