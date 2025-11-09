using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Volo.Abp.Domain.Services;

namespace DocFlow.Documents;

/// <summary>
/// Domain service for extracting text content from PDF documents.
/// Uses PdfPig library for PDF processing.
/// </summary>
public sealed class PdfTextExtractionManager : DomainService
{
    /// <summary>
    /// Extracts text content from a PDF document stream.
    /// </summary>
    /// <param name="pdfStream">Stream containing the PDF document</param>
    /// <returns>Extracted text content, or empty string if extraction fails</returns>
    public async Task<string> ExtractTextAsync(Stream pdfStream)
    {
        if (pdfStream == null) throw new ArgumentNullException(nameof(pdfStream));

        try
        {
            return await Task.Run(() => ExtractTextFromPdf(pdfStream));
        }
        catch (Exception ex)
        {
            // Log the exception (in production, use ILogger)
            Logger.LogWarning(ex, "Failed to extract text from PDF");
            return string.Empty;
        }
    }

    private string ExtractTextFromPdf(Stream pdfStream)
    {
        try
        {
            using var document = PdfDocument.Open(pdfStream);
            var textBuilder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                var pageText = ExtractTextFromPage(page);
                textBuilder.AppendLine(pageText);
            }

            return textBuilder.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractTextFromPage(Page page)
    {
        try
        {
            return page.Text;
        }
        catch
        {
            // If text extraction fails for this page, return empty string
            return string.Empty;
        }
    }
}
