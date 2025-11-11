using System;
using System.IO;
using System.Threading.Tasks;
using DocFlow.DocumentManagement;
using DocFlow.Documents.Dtos;
using Shouldly;
using Xunit;

namespace DocFlow.Application.Tests.DocumentManagement;

/// <summary>
/// Unit tests for DocumentApplicationService statistics methods.
/// </summary>
public class DocumentStatisticsTests : DocFlowApplicationTestBase<DocFlowApplicationTestModule>
{
    private readonly IDocumentApplicationService _documentService;

    public DocumentStatisticsTests()
    {
        _documentService = GetRequiredService<IDocumentApplicationService>();
    }

    [Fact]
    public async Task GetUploadStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange - Upload some test documents
        var testFile = new MemoryStream(new byte[1024]); // 1KB file
        var uploadDto = new UploadDocumentDto { FileName = "test1.pdf" };

        // Act
        await _documentService.UploadDocumentAsync(testFile, "test1.pdf", 1024, "application/pdf", uploadDto);
        
        // Get statistics
        var statistics = await _documentService.GetUploadStatisticsAsync();

        // Assert
        statistics.ShouldNotBeNull();
        statistics.FilesTotal.ShouldBeGreaterThanOrEqualTo(1);
        statistics.FilesToday.ShouldBeGreaterThanOrEqualTo(1);
        statistics.StorageUsedBytes.ShouldBeGreaterThanOrEqualTo(1024);
        statistics.StorageQuotaBytes.ShouldBe(DocFlowConsts.DefaultStorageQuotaBytes);
        statistics.StorageUsagePercent.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetRecentUploadsAsync_ShouldReturnRecentDocuments()
    {
        // Arrange - Upload a test document
        var testFile = new MemoryStream(new byte[2048]); // 2KB file
        var uploadDto = new UploadDocumentDto { FileName = "recent.pdf" };

        // Act
        await _documentService.UploadDocumentAsync(testFile, "recent.pdf", 2048, "application/pdf", uploadDto);
        
        // Get recent uploads
        var recentUploads = await _documentService.GetRecentUploadsAsync(10);

        // Assert
        recentUploads.ShouldNotBeNull();
        recentUploads.ShouldNotBeEmpty();
        recentUploads.ShouldContain(u => u.FileName == "recent.pdf");
        
        var uploadedDoc = recentUploads.Find(u => u.FileName == "recent.pdf");
        uploadedDoc.ShouldNotBeNull();
        uploadedDoc.FileSizeBytes.ShouldBe(2048);
        uploadedDoc.MinutesAgo.ShouldBeLessThan(5); // Should be uploaded recently
    }

    [Fact]
    public async Task GetRecentUploadsAsync_ShouldOnlyReturnLast24Hours()
    {
        // Arrange
        var testFile = new MemoryStream(new byte[1024]);
        var uploadDto = new UploadDocumentDto { FileName = "new_upload.pdf" };

        // Act
        await _documentService.UploadDocumentAsync(testFile, "new_upload.pdf", 1024, "application/pdf", uploadDto);
        
        // Get recent uploads
        var recentUploads = await _documentService.GetRecentUploadsAsync(50);

        // Assert
        recentUploads.ShouldNotBeNull();
        
        // All uploads should be from the last 24 hours
        foreach (var upload in recentUploads)
        {
            upload.MinutesAgo.ShouldBeLessThan(24 * 60); // Less than 24 hours
        }
    }

    [Fact]
    public async Task UploadStatistics_ShouldShowCorrectFileSize()
    {
        // Arrange
        var file1Size = 1024 * 1024; // 1MB
        var file2Size = 2 * 1024 * 1024; // 2MB
        
        var testFile1 = new MemoryStream(new byte[file1Size]);
        var testFile2 = new MemoryStream(new byte[file2Size]);
        
        var uploadDto1 = new UploadDocumentDto { FileName = "file1.pdf" };
        var uploadDto2 = new UploadDocumentDto { FileName = "file2.pdf" };

        // Act
        var beforeStats = await _documentService.GetUploadStatisticsAsync();
        var beforeUsed = beforeStats.StorageUsedBytes;
        
        await _documentService.UploadDocumentAsync(testFile1, "file1.pdf", file1Size, "application/pdf", uploadDto1);
        await _documentService.UploadDocumentAsync(testFile2, "file2.pdf", file2Size, "application/pdf", uploadDto2);
        
        var afterStats = await _documentService.GetUploadStatisticsAsync();

        // Assert
        var addedSize = file1Size + file2Size;
        afterStats.StorageUsedBytes.ShouldBe(beforeUsed + addedSize);
        afterStats.StorageUsedGB.ShouldBeGreaterThan(0);
    }
}
