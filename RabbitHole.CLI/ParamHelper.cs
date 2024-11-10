using System;

public static class ParamHelper
{
    // Helper method to format environment variable key by removing dashes
    private static string FormatEnvKey(string key) => key.Replace("-", "").ToUpperInvariant();

    // Gets a string value from the args array or environment variables based on the provided keys
    public static string GetString(string[] args, string defaultValue, params string[] keys)
    {
        foreach (var key in keys)
        {
            // Check if the formatted key exists in environment variables
            string envKey = FormatEnvKey(key);
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }

            // Otherwise, check the args array
            var index = Array.IndexOf(args, key);
            if (index >= 0 && index < args.Length - 1)
            {
                var value = args[index + 1].Trim();
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                return value;
            }
        }
        return defaultValue;
    }

    // Gets a long value from the args array or environment variables based on the provided keys
    public static long GetLong(string[] args, long defaultValue, params string[] keys)
    {
        foreach (var key in keys)
        {
            string envKey = FormatEnvKey(key);
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(envValue) && long.TryParse(envValue, out var envResult))
            {
                return envResult;
            }

            var index = Array.IndexOf(args, key);
            if (index >= 0 && index < args.Length - 1)
            {
                if (long.TryParse(args[index + 1], out var result))
                {
                    return result;
                }
            }
        }
        return defaultValue;
    }

    // Gets a bool value from the args array or environment variables based on the provided keys
    public static bool GetBool(string[] args, bool defaultValue, params string[] keys)
    {
        foreach (var key in keys)
        {
            string envKey = FormatEnvKey(key);
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(envValue) && bool.TryParse(envValue, out var envResult))
            {
                return envResult;
            }

            var index = Array.IndexOf(args, key);
            if (index >= 0 && index < args.Length - 1)
            {
                if (bool.TryParse(args[index + 1], out var result))
                {
                    return result;
                }
            }
        }
        return defaultValue;
    }
}
