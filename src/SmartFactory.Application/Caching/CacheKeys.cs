namespace SmartFactory.Application.Caching;

/// <summary>
/// Centralized cache key definitions for the Smart Factory system.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Cache key prefix for alarm-related data.
    /// </summary>
    public const string AlarmPrefix = "alarm";

    /// <summary>
    /// Cache key prefix for equipment-related data.
    /// </summary>
    public const string EquipmentPrefix = "equipment";

    /// <summary>
    /// Cache key prefix for factory-related data.
    /// </summary>
    public const string FactoryPrefix = "factory";

    /// <summary>
    /// Cache key prefix for production line-related data.
    /// </summary>
    public const string ProductionLinePrefix = "productionline";

    /// <summary>
    /// Cache key prefix for report-related data.
    /// </summary>
    public const string ReportPrefix = "report";

    /// <summary>
    /// Cache key prefix for work order-related data.
    /// </summary>
    public const string WorkOrderPrefix = "workorder";

    #region Alarm Keys

    /// <summary>
    /// Gets the cache key for alarm summary.
    /// </summary>
    /// <param name="factoryId">Optional factory ID for filtering.</param>
    public static string AlarmSummary(Guid? factoryId = null) =>
        factoryId.HasValue
            ? $"{AlarmPrefix}:summary:{factoryId.Value}"
            : $"{AlarmPrefix}:summary:all";

    /// <summary>
    /// Gets the cache key for active alarm count.
    /// </summary>
    /// <param name="factoryId">Optional factory ID for filtering.</param>
    public static string ActiveAlarmCount(Guid? factoryId = null) =>
        factoryId.HasValue
            ? $"{AlarmPrefix}:activecount:{factoryId.Value}"
            : $"{AlarmPrefix}:activecount:all";

    /// <summary>
    /// Pattern to invalidate all alarm-related cache entries.
    /// </summary>
    public static string AlarmPattern => $"{AlarmPrefix}:*";

    #endregion

    #region Equipment Keys

    /// <summary>
    /// Gets the cache key for equipment status summary.
    /// </summary>
    /// <param name="factoryId">Optional factory ID for filtering.</param>
    public static string EquipmentStatusSummary(Guid? factoryId = null) =>
        factoryId.HasValue
            ? $"{EquipmentPrefix}:status:summary:{factoryId.Value}"
            : $"{EquipmentPrefix}:status:summary:all";

    /// <summary>
    /// Gets the cache key for a specific equipment's status.
    /// </summary>
    /// <param name="equipmentId">The equipment ID.</param>
    public static string EquipmentStatus(Guid equipmentId) =>
        $"{EquipmentPrefix}:status:{equipmentId}";

    /// <summary>
    /// Gets the cache key for equipment list by factory.
    /// </summary>
    /// <param name="factoryId">The factory ID.</param>
    public static string EquipmentByFactory(Guid factoryId) =>
        $"{EquipmentPrefix}:byfactory:{factoryId}";

    /// <summary>
    /// Pattern to invalidate all equipment-related cache entries.
    /// </summary>
    public static string EquipmentPattern => $"{EquipmentPrefix}:*";

    #endregion

    #region Factory Keys

    /// <summary>
    /// Gets the cache key for all factories list.
    /// </summary>
    public static string AllFactories => $"{FactoryPrefix}:all";

    /// <summary>
    /// Gets the cache key for a specific factory.
    /// </summary>
    /// <param name="factoryId">The factory ID.</param>
    public static string Factory(Guid factoryId) =>
        $"{FactoryPrefix}:{factoryId}";

    /// <summary>
    /// Gets the cache key for factory with full hierarchy (production lines, equipment).
    /// </summary>
    /// <param name="factoryId">The factory ID.</param>
    public static string FactoryHierarchy(Guid factoryId) =>
        $"{FactoryPrefix}:hierarchy:{factoryId}";

    /// <summary>
    /// Pattern to invalidate all factory-related cache entries.
    /// </summary>
    public static string FactoryPattern => $"{FactoryPrefix}:*";

    #endregion

    #region Report Keys

    /// <summary>
    /// Gets the cache key for OEE report.
    /// </summary>
    /// <param name="factoryId">Optional factory ID.</param>
    /// <param name="startDate">Report start date.</param>
    /// <param name="endDate">Report end date.</param>
    public static string OeeReport(Guid? factoryId, DateTime startDate, DateTime endDate) =>
        $"{ReportPrefix}:oee:{factoryId?.ToString() ?? "all"}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";

    /// <summary>
    /// Gets the cache key for production report.
    /// </summary>
    /// <param name="factoryId">Optional factory ID.</param>
    /// <param name="startDate">Report start date.</param>
    /// <param name="endDate">Report end date.</param>
    public static string ProductionReport(Guid? factoryId, DateTime startDate, DateTime endDate) =>
        $"{ReportPrefix}:production:{factoryId?.ToString() ?? "all"}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";

    /// <summary>
    /// Gets the cache key for quality report.
    /// </summary>
    /// <param name="factoryId">Optional factory ID.</param>
    /// <param name="startDate">Report start date.</param>
    /// <param name="endDate">Report end date.</param>
    public static string QualityReport(Guid? factoryId, DateTime startDate, DateTime endDate) =>
        $"{ReportPrefix}:quality:{factoryId?.ToString() ?? "all"}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";

    /// <summary>
    /// Gets the cache key for maintenance report.
    /// </summary>
    /// <param name="factoryId">Optional factory ID.</param>
    /// <param name="startDate">Report start date.</param>
    /// <param name="endDate">Report end date.</param>
    public static string MaintenanceReport(Guid? factoryId, DateTime startDate, DateTime endDate) =>
        $"{ReportPrefix}:maintenance:{factoryId?.ToString() ?? "all"}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";

    /// <summary>
    /// Pattern to invalidate all report-related cache entries.
    /// </summary>
    public static string ReportPattern => $"{ReportPrefix}:*";

    #endregion

    #region Cache Durations

    /// <summary>
    /// Default cache durations for different data types.
    /// </summary>
    public static class Durations
    {
        /// <summary>
        /// Duration for alarm summary data (30 seconds sliding).
        /// </summary>
        public static readonly CacheEntryOptions AlarmSummary =
            CacheEntryOptions.Sliding(TimeSpan.FromSeconds(30));

        /// <summary>
        /// Duration for equipment status summary (5 minutes).
        /// </summary>
        public static readonly CacheEntryOptions EquipmentStatusSummary =
            CacheEntryOptions.Absolute(TimeSpan.FromMinutes(5));

        /// <summary>
        /// Duration for individual equipment status (1 minute sliding).
        /// </summary>
        public static readonly CacheEntryOptions EquipmentStatus =
            CacheEntryOptions.Sliding(TimeSpan.FromMinutes(1));

        /// <summary>
        /// Duration for factory master data (1 hour).
        /// </summary>
        public static readonly CacheEntryOptions FactoryMasterData =
            CacheEntryOptions.Absolute(TimeSpan.FromHours(1));

        /// <summary>
        /// Duration for reports (1 hour).
        /// </summary>
        public static readonly CacheEntryOptions Reports =
            CacheEntryOptions.Absolute(TimeSpan.FromHours(1));

        /// <summary>
        /// Duration for frequently changing data (30 seconds).
        /// </summary>
        public static readonly CacheEntryOptions ShortLived =
            CacheEntryOptions.Absolute(TimeSpan.FromSeconds(30));

        /// <summary>
        /// Duration for rarely changing reference data (24 hours).
        /// </summary>
        public static readonly CacheEntryOptions LongLived =
            CacheEntryOptions.Absolute(TimeSpan.FromHours(24));
    }

    #endregion
}
