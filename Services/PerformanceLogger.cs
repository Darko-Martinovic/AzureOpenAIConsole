using System.Diagnostics;

namespace AzureOpenAIConsole.Services
{
    public static class PerformanceLogger
    {
        private static readonly Dictionary<string, List<long>> _operationTimes = new();
        private static readonly object _lock = new object();

        public static IDisposable StartOperation(string operationName)
        {
            return new OperationTimer(operationName);
        }

        public static void LogOperation(string operationName, long milliseconds, string details = null)
        {
            lock (_lock)
            {
                if (!_operationTimes.ContainsKey(operationName))
                {
                    _operationTimes[operationName] = new List<long>();
                }
                _operationTimes[operationName].Add(milliseconds);
            }

            var message = $"[Performance] {operationName}: {milliseconds}ms";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" - {details}";
            }
            Console.WriteLine(message);
        }

        public static void PrintPerformanceStats()
        {
            Console.WriteLine("\n=== Performance Statistics ===");

            lock (_lock)
            {
                foreach (var kvp in _operationTimes)
                {
                    var times = kvp.Value;
                    if (times.Any())
                    {
                        var avg = times.Average();
                        var min = times.Min();
                        var max = times.Max();
                        var total = times.Sum();

                        Console.WriteLine($"{kvp.Key}:");
                        Console.WriteLine($"  Count: {times.Count}");
                        Console.WriteLine($"  Total: {total}ms");
                        Console.WriteLine($"  Average: {avg:F1}ms");
                        Console.WriteLine($"  Min: {min}ms");
                        Console.WriteLine($"  Max: {max}ms");
                    }
                }
            }
            Console.WriteLine("================================\n");
        }

        public static void ClearStats()
        {
            lock (_lock)
            {
                _operationTimes.Clear();
            }
            Console.WriteLine("[Performance] Statistics cleared");
        }

        private class OperationTimer : IDisposable
        {
            private readonly string _operationName;
            private readonly Stopwatch _stopwatch;
            private bool _disposed = false;

            public OperationTimer(string operationName)
            {
                _operationName = operationName;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                    LogOperation(_operationName, _stopwatch.ElapsedMilliseconds);
                    _disposed = true;
                }
            }
        }
    }
}
