namespace AudioClassification.Services
{
    public static class PathResolver
    {
        public static PathSettings Build(string contentRootPath)
        {
            return new PathSettings
            {
                DatasetPath = Path.Combine(contentRootPath, "AudioDataset"),
                FeaturesCsvPath = Path.Combine(contentRootPath, "Data", "features.csv"),
                ModelPath = Path.Combine(contentRootPath, "Model", "audio_model.zip"),
                TempPath = Path.Combine(contentRootPath, "Temp")
            };
        }
    }
}
