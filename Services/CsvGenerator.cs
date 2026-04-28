using AudioClassification.Utils;
using System.Globalization;

namespace AudioClassification.Services
{
    public class CsvGenerator
    {
        public static CsvGenerationResult Generate(string datasetPath, string outputPath)
        {
            if (!Directory.Exists(datasetPath))
            {
                throw new DirectoryNotFoundException($"Dataset folder was not found: {datasetPath}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var lines = new List<string>();

            // Header
            lines.Add("Label,F1,F2,F3,F4");

            int processedFiles = 0;
            var warnings = new List<string>();

            foreach (var labelDir in Directory.GetDirectories(datasetPath))
            {
                string label = Path.GetFileName(labelDir);

                foreach (var file in Directory.GetFiles(labelDir, "*.wav"))
                {
                    try
                    {
                        var features = FeatureExtractor.ExtractFeatures(file);

                        string row = label + "," +
                            string.Join(",", features.Select(f => f.ToString(CultureInfo.InvariantCulture)));

                        lines.Add(row);
                        processedFiles++;
                    }
                    catch (Exception ex)
                    {
                        warnings.Add($"{Path.GetFileName(file)} skipped: {ex.Message}");
                    }
                }
            }

            if (processedFiles == 0)
            {
                throw new InvalidOperationException("No readable audio files were found in the dataset.");
            }

            File.WriteAllLines(outputPath, lines);

            Console.WriteLine("features.csv generated");

            return new CsvGenerationResult
            {
                ProcessedFiles = processedFiles,
                SkippedFiles = warnings.Count,
                OutputPath = outputPath,
                Warnings = warnings
            };
        }
    }
}
