using System.Text.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using AzureOpenAIConsole.Models;

namespace AzureOpenAIConsole.Services
{
    public class DocumentLoader
    {
        private static readonly ConcurrentDictionary<string, CachedDocuments> _documentCache = new();
        private static readonly object _lockObject = new object();

        private class CachedDocuments
        {
            public List<DocumentInfo> Documents { get; set; } = new();
            public DateTime CacheTime { get; set; }
            public string FileHash { get; set; } = string.Empty;
        }

        public async Task<List<DocumentInfo>> LoadDocumentsFromJsonAsync(string filePath)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"[Performance] Starting JSON document loading from: {filePath}");

                // Validate file path
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"JSON document file not found at path: '{Path.GetFullPath(filePath)}'. Please ensure the file exists and the path is correct.");
                }

                // Check cache first
                var fullPath = Path.GetFullPath(filePath);
                if (TryGetFromCache(fullPath, out var cachedDocs))
                {
                    Console.WriteLine($"[Performance] Loaded {cachedDocs.Count} documents from cache in {stopwatch.ElapsedMilliseconds}ms");
                    return cachedDocs;
                }

                // Validate file accessibility
                ValidateFileAccess(fullPath);

                string jsonContent;
                try
                {
                    jsonContent = await File.ReadAllTextAsync(fullPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new UnauthorizedAccessException($"Access denied to file '{fullPath}'. Please check file permissions.", ex);
                }
                catch (IOException ex)
                {
                    throw new IOException($"Error reading file '{fullPath}'. The file may be in use by another process.", ex);
                }

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    throw new InvalidDataException($"JSON file '{fullPath}' is empty or contains only whitespace.");
                }

                List<DocumentInfo> documents;
                try
                {
                    documents = JsonSerializer.Deserialize<List<DocumentInfo>>(jsonContent);
                    if (documents == null)
                    {
                        throw new InvalidDataException($"Failed to deserialize JSON content from '{fullPath}'. The file may not contain a valid JSON array.");
                    }
                }
                catch (JsonException ex)
                {
                    throw new InvalidDataException($"Invalid JSON format in file '{fullPath}'. Error: {ex.Message}", ex);
                }

                // Validate documents
                ValidateDocuments(documents, "JSON");

                // Cache the results
                CacheDocuments(fullPath, documents);

                Console.WriteLine($"[Performance] Successfully loaded {documents.Count} documents from JSON in {stopwatch.ElapsedMilliseconds}ms");
                return documents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to load JSON documents from '{filePath}' after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DocumentInfo>> LoadDocumentsFromTextFilesAsync(string directoryPath)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"[Performance] Starting text files loading from: {directoryPath}");

                // Validate directory path
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));
                }

                if (!Directory.Exists(directoryPath))
                {
                    throw new DirectoryNotFoundException($"Documents directory not found at path: '{Path.GetFullPath(directoryPath)}'. Please ensure the directory exists.");
                }

                // Check cache first
                var fullPath = Path.GetFullPath(directoryPath);
                if (TryGetFromCache(fullPath, out var cachedDocs))
                {
                    Console.WriteLine($"[Performance] Loaded {cachedDocs.Count} documents from cache in {stopwatch.ElapsedMilliseconds}ms");
                    return cachedDocs;
                }

                var documents = new List<DocumentInfo>();
                string[] textFiles;

                try
                {
                    textFiles = Directory.GetFiles(fullPath, "*.txt", SearchOption.TopDirectoryOnly);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new UnauthorizedAccessException($"Access denied to directory '{fullPath}'. Please check directory permissions.", ex);
                }
                catch (IOException ex)
                {
                    throw new IOException($"Error accessing directory '{fullPath}': {ex.Message}", ex);
                }

                if (textFiles.Length == 0)
                {
                    Console.WriteLine($"[Warning] No .txt files found in directory '{fullPath}'");
                    return documents;
                }

                Console.WriteLine($"[Performance] Found {textFiles.Length} text files to process");

                for (int i = 0; i < textFiles.Length; i++)
                {
                    try
                    {
                        var filePath = textFiles[i];
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        // Validate file accessibility
                        ValidateFileAccess(filePath);

                        var content = await File.ReadAllTextAsync(filePath);

                        if (string.IsNullOrWhiteSpace(content))
                        {
                            Console.WriteLine($"[Warning] File '{fileName}' is empty or contains only whitespace, skipping");
                            continue;
                        }

                        documents.Add(new DocumentInfo
                        {
                            Id = (i + 1).ToString(),
                            Title = CleanFileName(fileName),
                            Content = content.Trim()
                        });

                        Console.WriteLine($"[Performance] Processed file: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Warning] Failed to process file '{textFiles[i]}': {ex.Message}. Continuing with other files.");
                    }
                }

                // Validate documents
                ValidateDocuments(documents, "Text Files");

                // Cache the results
                CacheDocuments(fullPath, documents);

                Console.WriteLine($"[Performance] Successfully loaded {documents.Count} documents from text files in {stopwatch.ElapsedMilliseconds}ms");
                return documents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to load text documents from '{directoryPath}' after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DocumentInfo>> LoadDocumentsFromCsvAsync(string filePath)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"[Performance] Starting CSV document loading from: {filePath}");

                // Validate file path
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"CSV document file not found at path: '{Path.GetFullPath(filePath)}'. Please ensure the file exists and the path is correct.");
                }

                // Check cache first
                var fullPath = Path.GetFullPath(filePath);
                if (TryGetFromCache(fullPath, out var cachedDocs))
                {
                    Console.WriteLine($"[Performance] Loaded {cachedDocs.Count} documents from cache in {stopwatch.ElapsedMilliseconds}ms");
                    return cachedDocs;
                }

                // Validate file accessibility
                ValidateFileAccess(fullPath);

                var documents = new List<DocumentInfo>();
                string[] lines;

                try
                {
                    lines = await File.ReadAllLinesAsync(fullPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new UnauthorizedAccessException($"Access denied to file '{fullPath}'. Please check file permissions.", ex);
                }
                catch (IOException ex)
                {
                    throw new IOException($"Error reading CSV file '{fullPath}'. The file may be in use by another process.", ex);
                }

                if (lines.Length == 0)
                {
                    throw new InvalidDataException($"CSV file '{fullPath}' is empty.");
                }

                if (lines.Length < 2)
                {
                    throw new InvalidDataException($"CSV file '{fullPath}' must contain at least a header row and one data row.");
                }

                // Validate header
                var headerColumns = ParseCsvLine(lines[0]);
                if (headerColumns.Length < 3)
                {
                    throw new InvalidDataException($"CSV file '{fullPath}' header must contain at least 3 columns (Id, Title, Content). Found: {string.Join(", ", headerColumns)}");
                }

                // Process data rows (skip header)
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                        {
                            continue; // Skip empty lines
                        }

                        var columns = ParseCsvLine(lines[i]);
                        if (columns.Length < 3)
                        {
                            Console.WriteLine($"[Warning] CSV row {i + 1} has insufficient columns ({columns.Length}), skipping. Expected: Id, Title, Content");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(columns[0]) || string.IsNullOrWhiteSpace(columns[1]) || string.IsNullOrWhiteSpace(columns[2]))
                        {
                            Console.WriteLine($"[Warning] CSV row {i + 1} has empty required fields, skipping");
                            continue;
                        }

                        documents.Add(new DocumentInfo
                        {
                            Id = columns[0].Trim(),
                            Title = columns[1].Trim(),
                            Content = columns[2].Trim()
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Warning] Failed to process CSV row {i + 1}: {ex.Message}. Continuing with other rows.");
                    }
                }

                // Validate documents
                ValidateDocuments(documents, "CSV");

                // Cache the results
                CacheDocuments(fullPath, documents);

                Console.WriteLine($"[Performance] Successfully loaded {documents.Count} documents from CSV in {stopwatch.ElapsedMilliseconds}ms");
                return documents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to load CSV documents from '{filePath}' after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
                throw;
            }
        }

        private bool TryGetFromCache(string fullPath, out List<DocumentInfo> documents)
        {
            documents = new List<DocumentInfo>();

            if (!_documentCache.TryGetValue(fullPath, out var cached))
            {
                return false;
            }

            // Check if cache is still valid (within 5 minutes and file hasn't changed)
            var cacheAge = DateTime.Now - cached.CacheTime;
            if (cacheAge.TotalMinutes > 5)
            {
                _documentCache.TryRemove(fullPath, out _);
                return false;
            }

            // For files, check if the file has been modified
            if (File.Exists(fullPath))
            {
                var currentHash = GetFileHash(fullPath);
                if (currentHash != cached.FileHash)
                {
                    _documentCache.TryRemove(fullPath, out _);
                    return false;
                }
            }
            // For directories, check if the directory has been modified
            else if (Directory.Exists(fullPath))
            {
                var currentHash = GetDirectoryHash(fullPath);
                if (currentHash != cached.FileHash)
                {
                    _documentCache.TryRemove(fullPath, out _);
                    return false;
                }
            }

            documents = cached.Documents;
            return true;
        }

        private void CacheDocuments(string fullPath, List<DocumentInfo> documents)
        {
            var hash = File.Exists(fullPath) ? GetFileHash(fullPath) : GetDirectoryHash(fullPath);

            var cached = new CachedDocuments
            {
                Documents = documents,
                CacheTime = DateTime.Now,
                FileHash = hash
            };

            _documentCache.AddOrUpdate(fullPath, cached, (key, oldValue) => cached);
        }

        private string GetFileHash(string filePath)
        {
            return File.GetLastWriteTime(filePath).Ticks.ToString();
        }

        private string GetDirectoryHash(string directoryPath)
        {
            return Directory.GetLastWriteTime(directoryPath).Ticks.ToString();
        }

        private void ValidateFileAccess(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                // Just check if we can open the file
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"Access denied to file '{filePath}'. Please check file permissions.");
            }
            catch (IOException ex)
            {
                throw new IOException($"Cannot access file '{filePath}'. The file may be in use by another process or corrupted.", ex);
            }
        }

        private void ValidateDocuments(List<DocumentInfo> documents, string sourceType)
        {
            if (documents.Count == 0)
            {
                throw new InvalidDataException($"No valid documents found in {sourceType} source.");
            }

            var duplicateIds = documents.GroupBy(d => d.Id).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicateIds.Any())
            {
                throw new InvalidDataException($"Duplicate document IDs found in {sourceType}: {string.Join(", ", duplicateIds)}");
            }

            var invalidDocs = documents.Where(d => string.IsNullOrWhiteSpace(d.Id) || string.IsNullOrWhiteSpace(d.Title) || string.IsNullOrWhiteSpace(d.Content)).ToList();
            if (invalidDocs.Any())
            {
                throw new InvalidDataException($"Found {invalidDocs.Count} documents with missing required fields (Id, Title, or Content) in {sourceType}.");
            }
        }

        private string CleanFileName(string fileName)
        {
            return fileName.Replace("_", " ").Replace("-", " ");
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            result.Add(current.Trim());
            return result.ToArray();
        }

        public void ClearCache()
        {
            _documentCache.Clear();
            Console.WriteLine("[Performance] Document cache cleared");
        }

        public void GetCacheStats()
        {
            Console.WriteLine($"[Performance] Cache contains {_documentCache.Count} entries");
            foreach (var kvp in _documentCache)
            {
                var age = DateTime.Now - kvp.Value.CacheTime;
                Console.WriteLine($"  - {Path.GetFileName(kvp.Key)}: {kvp.Value.Documents.Count} docs, cached {age.TotalMinutes:F1} minutes ago");
            }
        }
    }
}
