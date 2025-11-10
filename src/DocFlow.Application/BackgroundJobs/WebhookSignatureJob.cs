using System;
using System.Threading.Tasks;
using DocFlow.Webhooks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace DocFlow.BackgroundJobs;

public class WebhookSignatureJob : AsyncBackgroundJob<WebhookSignatureJobArgs>, ITransientDependency
{
    private readonly IWebhookEventRepository _webhookEventRepository;
    private readonly IHmacSignatureService _hmacSignatureService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookSignatureJob> _logger;

    public WebhookSignatureJob(
        IWebhookEventRepository webhookEventRepository,
        IHmacSignatureService hmacSignatureService,
        IConfiguration configuration,
        ILogger<WebhookSignatureJob> logger)
    {
        _webhookEventRepository = webhookEventRepository;
        _hmacSignatureService = hmacSignatureService;
        _configuration = configuration;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(WebhookSignatureJobArgs args)
    {
        try
        {
            var webhooks = await _webhookEventRepository.GetPendingWebhooksAsync(args.BatchSize);

            var webhookSecret = _configuration["Webhook:Secret"] ?? "default-secret-key";

            foreach (var webhook in webhooks)
            {
                try
                {
                    // Generate HMAC signature
                    var signature = _hmacSignatureService.GenerateSignature(webhook.Payload, webhookSecret);
                    webhook.SetHmacSignature(signature);

                    webhook.MarkAsSending();
                    await _webhookEventRepository.UpdateAsync(webhook);

                    // TODO: Send webhook to target URL with signature
                    await SendWebhook(webhook);

                    webhook.MarkAsSent();
                    await _webhookEventRepository.UpdateAsync(webhook);

                    _logger.LogInformation(
                        "Webhook {WebhookId} signed and sent to {TargetUrl}",
                        webhook.Id, webhook.TargetUrl);
                }
                catch (Exception ex)
                {
                    webhook.MarkAsFailed();
                    await _webhookEventRepository.UpdateAsync(webhook);
                    
                    _logger.LogError(ex, "Failed to process webhook {WebhookId}", webhook.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing webhook signature job");
            throw;
        }
    }

    private Task SendWebhook(WebhookEvent webhook)
    {
        // Placeholder for actual webhook sending logic
        // In production, this would make an HTTP POST to the target URL
        _logger.LogDebug(
            "Sending webhook {WebhookId} to {TargetUrl} with signature {Signature}",
            webhook.Id, webhook.TargetUrl, webhook.HmacSignature);
        
        return Task.CompletedTask;
    }
}

public class WebhookSignatureJobArgs
{
    public int BatchSize { get; set; } = 100;
}
