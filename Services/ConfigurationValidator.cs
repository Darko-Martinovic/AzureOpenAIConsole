using AzureOpenAIConsole.Models;

namespace AzureOpenAIConsole.Services
{
    public class ConfigurationValidator
    {
        public static ValidationResult ValidateConfiguration(AppConfiguration config)
        {
            var result = new ValidationResult();

            if (config == null)
            {
                result.AddError("Configuration cannot be null");
                return result;
            }

            // Validate DataSource configuration
            if (config.DataSource == null)
            {
                result.AddError("DataSource configuration is required");
            }
            else
            {
                ValidateDataSource(config.DataSource, result);
            }

            // Validate IndexName
            if (string.IsNullOrWhiteSpace(config.IndexName))
            {
                result.AddError("IndexName cannot be empty");
            }
            else if (config.IndexName.Length > 128)
            {
                result.AddError("IndexName cannot exceed 128 characters");
            }
            else if (!IsValidIndexName(config.IndexName))
            {
                result.AddError("IndexName can only contain lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen");
            }

            return result;
        }

        private static void ValidateDataSource(DataSourceConfig dataSource, ValidationResult result)
        {
            switch (dataSource.Type)
            {
                case DataSourceType.Json:
                    if (string.IsNullOrWhiteSpace(dataSource.FilePath))
                    {
                        result.AddError("FilePath is required for JSON data source");
                    }
                    else if (!dataSource.FilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        result.AddWarning("FilePath should end with .json for JSON data source");
                    }
                    break;

                case DataSourceType.Csv:
                    if (string.IsNullOrWhiteSpace(dataSource.FilePath))
                    {
                        result.AddError("FilePath is required for CSV data source");
                    }
                    else if (!dataSource.FilePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        result.AddWarning("FilePath should end with .csv for CSV data source");
                    }
                    break;

                case DataSourceType.TextFiles:
                    if (string.IsNullOrWhiteSpace(dataSource.DirectoryPath))
                    {
                        result.AddError("DirectoryPath is required for TextFiles data source");
                    }
                    break;

                case DataSourceType.HardCoded:
                    // No additional validation needed
                    break;

                default:
                    result.AddError($"Unknown data source type: {dataSource.Type}");
                    break;
            }
        }

        private static bool IsValidIndexName(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
                return false;

            // Index name must be lowercase
            if (indexName != indexName.ToLowerInvariant())
                return false;

            // Cannot start or end with hyphen
            if (indexName.StartsWith("-") || indexName.EndsWith("-"))
                return false;

            // Can only contain lowercase letters, numbers, and hyphens
            return indexName.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
        }

        public static ValidationResult ValidateEnvironmentVariables()
        {
            var result = new ValidationResult();
            var requiredVars = new[]
            {
                "AOAI_ENDPOINT",
                "AOAI_APIKEY",
                "CHATCOMPLETION_DEPLOYMENTNAME",
                "EMBEDDING_DEPLOYMENTNAME",
                "COGNITIVESEARCH_ENDPOINT",
                "COGNITIVESEARCH_APIKEY"
            };

            foreach (var varName in requiredVars)
            {
                var value = Environment.GetEnvironmentVariable(varName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    result.AddError($"Environment variable '{varName}' is not set or is empty");
                }
                else if (varName.Contains("ENDPOINT") && !Uri.TryCreate(value, UriKind.Absolute, out _))
                {
                    result.AddError($"Environment variable '{varName}' is not a valid URL: {value}");
                }
            }

            return result;
        }
    }

    public class ValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public bool IsValid => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        public void PrintResults()
        {
            if (Warnings.Any())
            {
                Console.WriteLine("[Validation] Warnings:");
                foreach (var warning in Warnings)
                {
                    Console.WriteLine($"  - {warning}");
                }
            }

            if (Errors.Any())
            {
                Console.WriteLine("[Validation] Errors:");
                foreach (var error in Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
            else
            {
                Console.WriteLine("[Validation] Configuration is valid");
            }
        }
    }
}
