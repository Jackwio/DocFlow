using System;
using System.Threading.Tasks;
using DocFlow.BackgroundJobs;
using DocFlow.Quotas;
using DocFlow.Documents;
using Shouldly;
using Xunit;

namespace DocFlow.Application.Tests.BackgroundJobs;

public class QuotaCheckJobTests : DocFlowApplicationTestBase<DocFlowApplicationTestModule>
{
    private readonly QuotaCheckJob _quotaCheckJob;
    private readonly ITenantQuotaRepository _quotaRepository;
    private readonly IDocumentRepository _documentRepository;

    public QuotaCheckJobTests()
    {
        _quotaCheckJob = GetRequiredService<QuotaCheckJob>();
        _quotaRepository = GetRequiredService<ITenantQuotaRepository>();
        _documentRepository = GetRequiredService<IDocumentRepository>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateQuotaUsage_ForExistingTenant()
    {
        // Arrange - Use null tenant for host
        var tenantId = Guid.NewGuid();

        var quota = TenantQuota.Create(Guid.NewGuid(), tenantId, 1000, 10737418240);
        await _quotaRepository.InsertAsync(quota);

        var document = Document.Create(Guid.NewGuid(), tenantId, "test.pdf", "/path", 1024, "application/pdf");
        await _documentRepository.InsertAsync(document);

        var args = new QuotaCheckJobArgs();

        // Act
        await _quotaCheckJob.ExecuteAsync(args);

        // Assert
        var updatedQuota = await _quotaRepository.FindByTenantIdAsync(tenantId);
        updatedQuota.ShouldNotBeNull();
        updatedQuota.CurrentDocumentCount.ShouldBe(1);
        updatedQuota.CurrentStorageBytes.ShouldBe(1024);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldBlockUploads_WhenQuotaExceeded()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var quota = TenantQuota.Create(Guid.NewGuid(), tenantId, 1, 10737418240); // Max 1 document
        await _quotaRepository.InsertAsync(quota);

        // Add 2 documents to exceed quota
        var document1 = Document.Create(Guid.NewGuid(), tenantId, "test1.pdf", "/path1", 1024, "application/pdf");
        var document2 = Document.Create(Guid.NewGuid(), tenantId, "test2.pdf", "/path2", 2048, "application/pdf");
        await _documentRepository.InsertAsync(document1);
        await _documentRepository.InsertAsync(document2);

        var args = new QuotaCheckJobArgs();

        // Act
        await _quotaCheckJob.ExecuteAsync(args);

        // Assert
        var updatedQuota = await _quotaRepository.FindByTenantIdAsync(tenantId);
        updatedQuota.ShouldNotBeNull();
        updatedQuota.IsBlocked.ShouldBeTrue();
    }
}
