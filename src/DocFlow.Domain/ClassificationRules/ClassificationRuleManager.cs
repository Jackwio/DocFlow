using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocFlow.Documents;
using DocFlow.Enums;
using Volo.Abp.Domain.Services;

namespace DocFlow.ClassificationRules;

/// <summary>
/// Domain service for evaluating classification rules against documents.
/// Handles rule matching logic and priority-based evaluation.
/// </summary>
public sealed class ClassificationRuleManager : DomainService
{
    /// <summary>
    /// Evaluates all active rules against a document and returns matching rules.
    /// Rules are evaluated in priority order (lower priority number = higher priority).
    /// </summary>
    public async Task<List<ClassificationRule>> EvaluateRulesAsync(
        Document document,
        List<ClassificationRule> activeRules,
        string? extractedText = null)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (activeRules == null) throw new ArgumentNullException(nameof(activeRules));

        var matchedRules = new List<ClassificationRule>();

        // Sort rules by priority (lower number = higher priority)
        var sortedRules = activeRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority.Value)
            .ToList();

        foreach (var rule in sortedRules)
        {
            var matches = await EvaluateRuleConditionsAsync(document, rule, extractedText);
            if (matches)
            {
                matchedRules.Add(rule);
            }
        }

        return matchedRules;
    }

    /// <summary>
    /// Evaluates a single rule in dry-run mode without applying changes.
    /// Returns whether the rule would match and which conditions matched.
    /// </summary>
    public async Task<(bool Matches, List<string> MatchedConditions)> EvaluateRuleInDryRunModeAsync(
        Document document,
        ClassificationRule rule,
        string? extractedText = null)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (rule == null) throw new ArgumentNullException(nameof(rule));

        var matchedConditions = new List<string>();
        var allConditionsMatch = true;

        foreach (var condition in rule.Conditions)
        {
            var matches = EvaluateCondition(document, condition, extractedText);
            if (matches)
            {
                matchedConditions.Add($"{condition.Type}: {condition.Pattern}");
            }
            else
            {
                allConditionsMatch = false;
            }
        }

        // All conditions must match for the rule to match
        var ruleMatches = allConditionsMatch && rule.Conditions.Any();

        return await Task.FromResult((ruleMatches, matchedConditions));
    }

    private async Task<bool> EvaluateRuleConditionsAsync(
        Document document,
        ClassificationRule rule,
        string? extractedText)
    {
        if (!rule.Conditions.Any())
            return false;

        // All conditions must match (AND logic)
        foreach (var condition in rule.Conditions)
        {
            if (!EvaluateCondition(document, condition, extractedText))
            {
                return false;
            }
        }

        return await Task.FromResult(true);
    }

    private bool EvaluateCondition(Document document, RuleCondition condition, string? extractedText)
    {
        return condition.Type switch
        {
            RuleConditionType.FileNameRegex => EvaluateFileNameRegex(document.FileName, condition.Pattern),
            RuleConditionType.MimeType => EvaluateMimeType(document.MimeType, condition.Pattern),
            RuleConditionType.FileSize => EvaluateFileSize(document.FileSize, condition.Pattern),
            RuleConditionType.TextContent => EvaluateTextContent(extractedText, condition.Pattern),
            _ => false
        };
    }

    private bool EvaluateFileNameRegex(FileName fileName, string pattern)
    {
        try
        {
            return Regex.IsMatch(fileName.Value, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        }
        catch (RegexMatchTimeoutException)
        {
            // Prevent ReDoS attacks
            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool EvaluateMimeType(MimeType mimeType, string pattern)
    {
        return mimeType.Value.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private bool EvaluateFileSize(FileSize fileSize, string pattern)
    {
        // Pattern format: "minBytes-maxBytes"
        var parts = pattern.Split('-');
        if (parts.Length != 2)
            return false;

        if (!long.TryParse(parts[0], out var minBytes) || !long.TryParse(parts[1], out var maxBytes))
            return false;

        return fileSize.Bytes >= minBytes && fileSize.Bytes <= maxBytes;
    }

    private bool EvaluateTextContent(string? extractedText, string pattern)
    {
        if (string.IsNullOrEmpty(extractedText))
            return false;

        return extractedText.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
