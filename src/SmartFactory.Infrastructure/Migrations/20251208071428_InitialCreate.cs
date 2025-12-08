using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Factories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "UTC"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FactoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DesignedCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionLines_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FactoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TargetQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    DefectQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduledEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualEnd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CustomerOrderRef = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductionLineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    OpcNodeId = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Manufacturer = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InstallationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastHeartbeat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MaintenanceIntervalDays = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_ProductionLines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlarmCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alarms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alarms_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TechnicianName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    ActualCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    DowntimeMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    PartsUsed = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    InspectionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Result = table.Column<int>(type: "INTEGER", nullable: false),
                    DefectType = table.Column<int>(type: "INTEGER", nullable: true),
                    DefectDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    InspectorId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InspectorName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SampleSize = table.Column<int>(type: "INTEGER", nullable: true),
                    DefectCount = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityRecords_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityRecords_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SensorData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorData_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TargetQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    DefectCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderSteps_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderSteps_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_EquipmentId",
                table: "Alarms",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_OccurredAt",
                table: "Alarms",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_Severity",
                table: "Alarms",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_Status",
                table: "Alarms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_ProductionLineId_Code",
                table: "Equipment",
                columns: new[] { "ProductionLineId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Status",
                table: "Equipment",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Factories_Code",
                table: "Factories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_EquipmentId",
                table: "MaintenanceRecords",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_ScheduledDate",
                table: "MaintenanceRecords",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_Status",
                table: "MaintenanceRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_Type",
                table: "MaintenanceRecords",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLines_FactoryId_Code",
                table: "ProductionLines",
                columns: new[] { "FactoryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityRecords_EquipmentId",
                table: "QualityRecords",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRecords_InspectedAt",
                table: "QualityRecords",
                column: "InspectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRecords_Result",
                table: "QualityRecords",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRecords_WorkOrderId",
                table: "QualityRecords",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorData_EquipmentId_Timestamp",
                table: "SensorData",
                columns: new[] { "EquipmentId", "Timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_SensorData_Timestamp",
                table: "SensorData",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_FactoryId",
                table: "WorkOrders",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_OrderNumber",
                table: "WorkOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ScheduledStart",
                table: "WorkOrders",
                column: "ScheduledStart");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Status",
                table: "WorkOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSteps_EquipmentId",
                table: "WorkOrderSteps",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSteps_WorkOrderId_Sequence",
                table: "WorkOrderSteps",
                columns: new[] { "WorkOrderId", "Sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alarms");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "QualityRecords");

            migrationBuilder.DropTable(
                name: "SensorData");

            migrationBuilder.DropTable(
                name: "WorkOrderSteps");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "ProductionLines");

            migrationBuilder.DropTable(
                name: "Factories");
        }
    }
}
