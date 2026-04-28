namespace AudioClassification.Services
{
    public class CsvGenerationResult
    {
        public int ProcessedFiles { get; init; }
        public int SkippedFiles { get; init; }
        public string OutputPath { get; init; } = string.Empty;
        public List<string> Warnings { get; init; } = new();
    }
}
