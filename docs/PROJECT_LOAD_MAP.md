# SmartFactorySystem - Comprehensive Project Load Map

## Executive Summary

SmartFactorySystem is a comprehensive WPF-based manufacturing execution system built with .NET 8.0 using a clean architecture approach. The system manages factory operations, equipment monitoring, production workflows, quality control, maintenance scheduling, and alarm management, with OPC-UA integration for industrial equipment communication.

**Project Statistics:**
| Metric | Value |
|--------|-------|
| Total C# Files | 61 (excluding obj directories) |
| Total Projects | 6 |
| Architecture Pattern | Clean Architecture (Domain → Application → Infrastructure → Presentation) |
| UI Framework | WPF with MahApps.Metro |
| Database | SQL Server with Entity Framework Core |
| MVVM Pattern | Community Toolkit MVVM |

---

## 1. Solution Architecture Overview

```
SmartFactorySystem.sln
├── SmartFactory.Domain           (24 files) - Core business entities
├── SmartFactory.Shared           (0 files)  - Cross-cutting utilities
├── SmartFactory.Application      (0 files)  - Services, DTOs, Validators
├── SmartFactory.Infrastructure   (15 files) - EF Core, Repositories
├── SmartFactory.Infrastructure.OpcUa (0 files) - OPC-UA Client
└── SmartFactory.Presentation     (34 files) - WPF Views & ViewModels
```

### Dependency Flow
```
Presentation → Application → Infrastructure → Domain
         ↘            ↘              ↘           ↙
          Infrastructure.OpcUa (horizontal dependency)
          Shared (utility dependencies)
```

---

## 2. Domain Layer (`SmartFactory.Domain`)

**Purpose:** Core business entities, value objects, enums, and repository interfaces.

### 2.1 Entity Hierarchy

```
BaseEntity (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
├── AuditableEntity (+ IsDeleted, DeletedAt, DeletedBy)
│   └── Factory
├── ProductionLine
├── Equipment
├── WorkOrder
├── WorkOrderStep
├── Alarm
├── SensorData (uses long Id for time-series optimization)
├── MaintenanceRecord
└── QualityRecord
```

### 2.2 Entity Relationships

```
┌─────────────────────────────────────────────────────────────┐
│                         FACTORY                              │
│  Code, Name, Location, Address, TimeZone, ContactInfo       │
└────────────────┬────────────────────────────┬───────────────┘
                 │ (1:M)                      │ (1:M)
                 ▼                            ▼
    ┌────────────────────┐       ┌────────────────────────────┐
    │  PRODUCTION LINE   │       │       WORK ORDER           │
    │  Code, Sequence    │       │  OrderNumber, Status       │
    └────────┬───────────┘       └────────┬───────────────────┘
             │ (1:M)                      │ (1:M)
             ▼                            ├──▶ WorkOrderStep
    ┌────────────────────┐                └──▶ QualityRecord
    │     EQUIPMENT      │
    │  OpcNodeId, Status │
    └────────┬───────────┘
             │ (1:M)
    ┌────────┼────────┐
    ▼        ▼        ▼
SensorData  Alarm  MaintenanceRecord
```

### 2.3 Key Entities

| Entity | Key Properties | Key Methods |
|--------|---------------|-------------|
| **Factory** | Code, Name, Location, TimeZone | AddProductionLine(), MarkAsDeleted() |
| **Equipment** | OpcNodeId, Status, LastHeartbeat | UpdateStatus(), RecordHeartbeat(), IsMaintenanceDue() |
| **WorkOrder** | OrderNumber, Status, Priority | Start(), Pause(), Complete(), Cancel() |
| **Alarm** | AlarmCode, Severity, Status | Acknowledge(), Resolve() |
| **SensorData** | TagName, Value, Unit, Quality | (Time-series optimized with long Id) |

### 2.4 Enums

| Enum | Values |
|------|--------|
| EquipmentStatus | Offline, Idle, Running, Warning, Error, Maintenance, Setup |
| EquipmentType | SMTMachine, AOIMachine, ReflowOven, WaveSoldering, etc. |
| WorkOrderStatus | Draft, Scheduled, InProgress, Paused, Completed, Cancelled |
| AlarmSeverity | Information, Warning, Error, Critical |
| DataQuality | Good, Uncertain, Bad |

### 2.5 Value Objects

| Value Object | Purpose |
|--------------|---------|
| Measurement | Value + Unit + Quality with derived types (Temperature, Percentage) |
| DateTimeRange | Start/End time range with duration calculations |

### 2.6 Repository Interfaces

```
Domain/Interfaces/
├── IRepository<T>           - Generic CRUD operations
├── IFactoryRepository       - Factory-specific queries
├── IEquipmentRepository     - Equipment queries + status summary
├── IAlarmRepository         - Alarm queries with filtering
├── IWorkOrderRepository     - WorkOrder queries
└── IUnitOfWork              - Transaction management
```

---

## 3. Infrastructure Layer (`SmartFactory.Infrastructure`)

**Purpose:** Data persistence, repository implementations, and external service integrations.

### 3.1 Database Context

**SmartFactoryDbContext.cs** - EF Core DbContext with:
- 9 DbSet<T> entities
- Soft delete query filter on Factory
- Auto-update audit fields (CreatedAt, UpdatedAt)
- SQL Server with retry-on-failure (3 retries)

### 3.2 Entity Configurations

| Configuration | Key Features |
|--------------|--------------|
| FactoryConfiguration | Unique Code index, soft delete setup |
| EquipmentConfiguration | OpcNodeId index, status constraints |
| SensorDataConfiguration | Time-series optimization, partitioning hints |
| WorkOrderConfiguration | Status workflow, priority handling |

### 3.3 Repositories

```
Infrastructure/Repositories/
├── RepositoryBase<T>     - Generic CRUD implementation
├── FactoryRepository     - GetActiveFactoriesAsync(), GetWithProductionLinesAsync()
├── EquipmentRepository   - GetByStatusAsync(), GetStatusSummaryAsync()
├── AlarmRepository       - GetActiveAlarmsAsync(), GetByEquipmentAsync()
└── UnitOfWork            - SaveChangesAsync(), Transaction management
```

### 3.4 Dependency Injection

```csharp
// Infrastructure/DependencyInjection.cs
services.AddInfrastructure(configuration)
    ├── DbContext<SmartFactoryDbContext> (SQL Server)
    ├── IFactoryRepository → FactoryRepository (Scoped)
    ├── IEquipmentRepository → EquipmentRepository (Scoped)
    ├── IAlarmRepository → AlarmRepository (Scoped)
    └── IUnitOfWork → UnitOfWork (Scoped)
```

---

## 4. Application Layer (`SmartFactory.Application`)

**Status:** Empty - Prepared for implementation

**Planned Structure:**
```
Application/
├── DTOs/           - Data transfer objects
├── Services/       - Business logic services
├── Interfaces/     - Service contracts
├── Mappings/       - AutoMapper profiles
├── Validators/     - FluentValidation rules
└── Common/         - Shared utilities
```

---

## 5. OPC-UA Integration (`SmartFactory.Infrastructure.OpcUa`)

**Status:** Empty - Prepared for implementation

**Planned Structure:**
```
Infrastructure.OpcUa/
├── Services/
│   └── OpcUaClientService   - OPC-UA client management
├── Configuration/
│   └── OpcUaSettings        - Connection configuration
└── Models/
    └── OpcUaTag             - Tag definitions
```

---

## 6. Presentation Layer (`SmartFactory.Presentation`)

**Purpose:** WPF user interface with MVVM pattern.

### 6.1 Application Entry Point

**App.xaml.cs** - Configuration:
- Host.CreateDefaultBuilder() for DI
- Infrastructure services registration
- Serilog logging (Console + File)
- ViewModel and View registrations

### 6.2 ViewModel Structure

```
ViewModels/
├── Base/
│   ├── ViewModelBase        - IsBusy, Title, ExecuteAsync()
│   └── PageViewModelBase    - INavigationAware implementation
├── Shell/
│   └── ShellViewModel       - Navigation, factory selection, alarms
├── Dashboard/
│   └── DashboardViewModel   - KPI overview
├── Equipment/
│   ├── EquipmentViewModel   - Equipment list, filtering
│   └── EquipmentDetailViewModel - Equipment details
├── Production/
│   └── ProductionViewModel  - Work order management
├── Quality/
│   └── QualityViewModel     - Quality records
├── Maintenance/
│   └── MaintenanceViewModel - Maintenance scheduling
├── Alarms/
│   └── AlarmsViewModel      - Alarm management
├── Reports/
│   └── ReportsViewModel     - Analytics
└── Settings/
    └── SettingsViewModel    - Configuration
```

### 6.3 Views

| View | Purpose | Status |
|------|---------|--------|
| ShellView | Main window with navigation | ✅ Implemented |
| DashboardView | KPI dashboard | ✅ Implemented |
| EquipmentView | Equipment list | ✅ Implemented |
| EquipmentDetailView | Equipment details | ✅ Implemented |
| ProductionView | Work orders | 📋 Placeholder |
| QualityView | Quality records | 📋 Placeholder |
| MaintenanceView | Maintenance | 📋 Placeholder |
| AlarmsView | Alarms | 📋 Placeholder |
| ReportsView | Reports | 📋 Placeholder |
| SettingsView | Settings | 📋 Placeholder |

### 6.4 Services

| Service | Purpose |
|---------|---------|
| INavigationService | View navigation with ViewModel-first approach |
| IFactoryContextService | Current factory context management |

### 6.5 Converters

| Converter | Purpose |
|-----------|---------|
| IntToVisibilityConverter | int > 0 → Visible |
| BoolToVisibilityConverter | bool → Visibility with invert support |
| StatusToColorConverter | EquipmentStatus → SolidColorBrush |
| AlarmSeverityToColorConverter | AlarmSeverity → SolidColorBrush |

### 6.6 Theme (SCADA Style)

```
Colors:
├── Background: #1E1E1E (primary), #252526 (secondary)
├── Running: #4CAF50 (green)
├── Idle: #2196F3 (blue)
├── Warning: #FF9800 (orange)
├── Error: #F44336 (red)
├── Maintenance: #9C27B0 (purple)
└── Offline: #607D8B (gray)
```

---

## 7. Navigation Flow

```
Application Start
    ↓
App.xaml.cs (DI Setup)
    ↓
ShellView (Main Window)
    ├── Factory Selector (top)
    ├── Navigation Menu (left sidebar)
    │   ├── Dashboard
    │   ├── Equipment
    │   ├── Production
    │   ├── Quality
    │   ├── Maintenance
    │   ├── Alarms
    │   ├── Reports
    │   └── Settings
    └── Content Area (center)
        └── DashboardView (initial)
```

---

## 8. Configuration

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "SmartFactory": "Server=.;Database=SmartFactory;Trusted_Connection=True;"
  },
  "OpcUa": {
    "ServerUrl": "opc.tcp://localhost:4840",
    "SessionTimeout": "00:30:00",
    "DefaultSamplingInterval": 1000
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": ["Console", "File (logs/smartfactory-.log)"]
  },
  "Application": {
    "Title": "Smart Factory System",
    "RefreshIntervalSeconds": 5,
    "AlarmPollingIntervalSeconds": 10
  }
}
```

---

## 9. Key Design Patterns

| Pattern | Implementation |
|---------|---------------|
| Clean Architecture | Domain → Application → Infrastructure → Presentation |
| Repository Pattern | IRepository<T> with RepositoryBase<T> |
| Unit of Work | IUnitOfWork for transaction management |
| MVVM | Community Toolkit MVVM with source generators |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| Value Objects | Measurement, DateTimeRange |
| State Machine | WorkOrder, Alarm status transitions |

---

## 10. Technology Stack

| Category | Technology |
|----------|------------|
| Runtime | .NET 8.0 (LTS) |
| UI Framework | WPF |
| MVVM Toolkit | CommunityToolkit.Mvvm 8.4.0 |
| UI Theme | MahApps.Metro 2.4.10 |
| Icons | MahApps.Metro.IconPacks.Material 5.1.0 |
| Charts | LiveChartsCore.SkiaSharpView.WPF 2.0.0-rc3.3 |
| ORM | Entity Framework Core 8.0 |
| Database | SQL Server |
| Logging | Serilog |
| Validation | FluentValidation (planned) |
| Mapping | AutoMapper (planned) |

---

## 11. File Structure

```
SmartFactorySystem/
├── SmartFactorySystem.sln
├── docs/
│   └── PROJECT_LOAD_MAP.md (this file)
└── src/
    ├── SmartFactory.Domain/                    (24 files)
    │   ├── Common/BaseEntity.cs
    │   ├── Entities/                           (9 files)
    │   ├── Enums/                              (7 files)
    │   ├── Interfaces/                         (5 files)
    │   └── ValueObjects/                       (2 files)
    │
    ├── SmartFactory.Shared/                    (empty)
    │
    ├── SmartFactory.Application/               (empty)
    │
    ├── SmartFactory.Infrastructure/            (15 files)
    │   ├── Data/
    │   │   ├── SmartFactoryDbContext.cs
    │   │   └── Configurations/                 (9 files)
    │   ├── Repositories/                       (4 files)
    │   └── DependencyInjection.cs
    │
    ├── SmartFactory.Infrastructure.OpcUa/      (empty)
    │
    └── SmartFactory.Presentation/              (34 files)
        ├── App.xaml.cs
        ├── ViewModels/                         (10 files)
        ├── Views/                              (10 XAML + code-behind)
        ├── Services/                           (4 files)
        ├── Converters/                         (2 files)
        ├── Themes/                             (2 files)
        └── appsettings.json
```

---

## 12. Implementation Status

| Layer | Status | Completion |
|-------|--------|------------|
| Domain | ✅ Complete | 100% |
| Infrastructure | ✅ Complete | 100% |
| Infrastructure.OpcUa | 📋 Prepared | 0% |
| Application | 📋 Prepared | 0% |
| Shared | 📋 Prepared | 0% |
| Presentation | 🔄 In Progress | 60% |

### Next Steps
1. Implement Application layer services and DTOs
2. Complete remaining ViewModels (Production, Quality, Maintenance, etc.)
3. Implement OPC-UA client service
4. Add AutoMapper profiles
5. Add FluentValidation rules
6. Create EF Core migrations

---

## 13. Quick Reference

### Adding a New Entity
1. Create entity in `Domain/Entities/`
2. Add repository interface in `Domain/Interfaces/`
3. Create configuration in `Infrastructure/Data/Configurations/`
4. Add DbSet in SmartFactoryDbContext
5. Implement repository in `Infrastructure/Repositories/`
6. Register in DependencyInjection.cs

### Adding a New Page
1. Create ViewModel in `Presentation/ViewModels/{Feature}/`
2. Create View in `Presentation/Views/{Feature}/`
3. Register both in App.xaml.cs
4. Add navigation item in ShellViewModel

### Running the Application
```bash
# Requires Windows with SQL Server
cd src/SmartFactory.Presentation
dotnet run
```

---

*Generated: 2025-12-07*
*Version: 1.0.0*
