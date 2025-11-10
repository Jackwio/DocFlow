using DocFlow.Documents;
using DocFlow.Quotas;
using DocFlow.Tenants;
using DocFlow.Webhooks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace DocFlow.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class DocFlowDbContext :
    AbpDbContext<DocFlowDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<Document> Documents { get; set; }
    public DbSet<TenantQuota> TenantQuotas { get; set; }
    public DbSet<TenantBillingStatus> TenantBillingStatuses { get; set; }
    public DbSet<WebhookEvent> WebhookEvents { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public DocFlowDbContext(DbContextOptions<DocFlowDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<Document>(b =>
        {
            b.ToTable(DocFlowConsts.DbTablePrefix + "Documents", DocFlowConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FileName).IsRequired().HasMaxLength(512);
            b.Property(x => x.FilePath).IsRequired().HasMaxLength(2048);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(256);
            b.Property(x => x.Classification).HasMaxLength(256);
            b.Property(x => x.RoutingDestination).HasMaxLength(512);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => new { x.TenantId, x.Status });
        });

        builder.Entity<TenantQuota>(b =>
        {
            b.ToTable(DocFlowConsts.DbTablePrefix + "TenantQuotas", DocFlowConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.TenantId).IsUnique();
        });

        builder.Entity<TenantBillingStatus>(b =>
        {
            b.ToTable(DocFlowConsts.DbTablePrefix + "TenantBillingStatuses", DocFlowConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.TenantId).IsUnique();
            b.HasIndex(x => new { x.Status, x.GracePeriodEndDate });
        });

        builder.Entity<WebhookEvent>(b =>
        {
            b.ToTable(DocFlowConsts.DbTablePrefix + "WebhookEvents", DocFlowConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.EventType).IsRequired().HasMaxLength(256);
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.TargetUrl).IsRequired().HasMaxLength(2048);
            b.Property(x => x.HmacSignature).HasMaxLength(512);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => new { x.TenantId, x.Status });
        });
    }
}
