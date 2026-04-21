using System;
public static class IDGenerator
{
    public static string GenerateUniqueID(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}