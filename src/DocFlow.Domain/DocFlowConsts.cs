namespace DocFlow;

public static class DocFlowConsts
{
    public const string DbTablePrefix = "App";

    public const string DbSchema = null;

    /// <summary>
    /// Default storage quota per user in bytes (100 GB).
    /// </summary>
    public const long DefaultStorageQuotaBytes = 100L * 1024 * 1024 * 1024; // 100 GB
}
