namespace AudioClassification.Services
{
    public class PathSettings
    {
        public string DatasetPath { get; init; } = string.Empty;
        public string FeaturesCsvPath { get; init; } = string.Empty;
        public string ModelPath { get; init; } = string.Empty;
        public string TempPath { get; init; } = string.Empty;
    }
}
