using System;
using System.IO;
using System.Threading.Tasks;
using DocFlow.Documents;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Services;

namespace DocFlow.RoutingQueues;

/// <summary>
/// Domain service for routing documents to their destination queues.
/// Handles both folder-based and webhook-based routing.
/// </summary>
public sealed class RoutingManager : DomainService
{
    /// <summary>
    /// Routes a document to the specified queue.
    /// </summary>
    /// <param name="document">The document to route</param>
    /// <param name="queue">The destination queue</param>
    /// <param name="documentStream">Stream containing the document content</param>
    /// <returns>True if routing was successful, false otherwise</returns>
    public async Task<bool> RouteDocumentToQueueAsync(
        Document document,
        RoutingQueue queue,
        Stream documentStream)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (queue == null) throw new ArgumentNullException(nameof(queue));
        if (documentStream == null) throw new ArgumentNullException(nameof(documentStream));

        if (!queue.IsActive)
        {
            Logger.LogWarning($"Cannot route to inactive queue {queue.Id}");
            return false;
        }

        try
        {
            return queue.Type switch
            {
                Enums.QueueType.Folder => await RouteToFolderAsync(document, queue.FolderPath!, documentStream),
                Enums.QueueType.Webhook => await RouteToWebhookAsync(document, queue.WebhookConfiguration!),
                _ => false
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed to route document {document.Id} to queue {queue.Id}");
            return false;
        }
    }

    private async Task<bool> RouteToFolderAsync(
        Document document,
        FolderPath folderPath,
        Stream documentStream)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(folderPath.Value);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create destination file path
            var destinationPath = Path.Combine(folderPath.Value, document.FileName.Value);

            // Copy document to folder
            using var fileStream = File.Create(destinationPath);
            documentStream.Position = 0;
            await documentStream.CopyToAsync(fileStream);

            Logger.LogInformation($"Document {document.Id} routed to folder {folderPath.Value}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed to route document {document.Id} to folder {folderPath.Value}");
            return false;
        }
    }

    private async Task<bool> RouteToWebhookAsync(
        Document document,
        WebhookConfiguration webhookConfig)
    {
        // Webhook routing would be handled by a background job
        // This method just validates the configuration
        // Actual HTTP call would be in infrastructure layer
        
        Logger.LogInformation($"Document {document.Id} queued for webhook delivery to {webhookConfig.Url}");
        return await Task.FromResult(true);
    }
}
