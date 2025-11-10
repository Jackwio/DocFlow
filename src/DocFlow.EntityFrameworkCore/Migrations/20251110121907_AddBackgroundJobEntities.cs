using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddBackgroundJobEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Classification = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RoutingDestination = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LastRetryTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RetentionExpiryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTenantBillingStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentFailureDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GracePeriodEndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GracePeriodDays = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTenantBillingStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTenantQuotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxDocuments = table.Column<int>(type: "integer", nullable: false),
                    MaxStorageBytes = table.Column<long>(type: "bigint", nullable: false),
                    CurrentDocumentCount = table.Column<int>(type: "integer", nullable: false),
                    CurrentStorageBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTenantQuotas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    TargetUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    HmacSignature = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SentTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDocuments_Status",
                table: "AppDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppDocuments_TenantId_Status",
                table: "AppDocuments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppTenantBillingStatuses_Status_GracePeriodEndDate",
                table: "AppTenantBillingStatuses",
                columns: new[] { "Status", "GracePeriodEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppTenantBillingStatuses_TenantId",
                table: "AppTenantBillingStatuses",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppTenantQuotas_TenantId",
                table: "AppTenantQuotas",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppWebhookEvents_Status",
                table: "AppWebhookEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppWebhookEvents_TenantId_Status",
                table: "AppWebhookEvents",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDocuments");

            migrationBuilder.DropTable(
                name: "AppTenantBillingStatuses");

            migrationBuilder.DropTable(
                name: "AppTenantQuotas");

            migrationBuilder.DropTable(
                name: "AppWebhookEvents");
        }
    }
}
