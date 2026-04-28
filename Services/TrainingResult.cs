namespace AudioClassification.Services
{
    public class TrainingResult
    {
        public string ModelName { get; init; } = string.Empty;
        public double MicroAccuracy { get; init; }
        public double MacroAccuracy { get; init; }
        public double LogLoss { get; init; }
        public string ModelPath { get; init; } = string.Empty;
    }
}
