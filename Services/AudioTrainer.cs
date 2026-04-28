using Microsoft.ML;
using AudioClassification.Models;

namespace AudioClassification.Services
{
    public class AudioTrainer
    {
        private readonly MLContext mlContext = new MLContext();
        private readonly PathSettings _paths;
        private const string ModelName = "SdcaMaximumEntropy";

        public AudioTrainer(PathSettings paths)
        {
            _paths = paths;
        }

        public TrainingResult Train()
        {
            if (!File.Exists(_paths.FeaturesCsvPath))
            {
                throw new FileNotFoundException(
                    "Feature CSV was not found. Generate it first using /api/audio/generate-csv.",
                    _paths.FeaturesCsvPath);
            }

            var header = File.ReadLines(_paths.FeaturesCsvPath).FirstOrDefault();
            if (!string.Equals(header, "Label,F1,F2,F3,F4", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Feature CSV is invalid. Regenerate it first using /api/audio/generate-csv.");
            }

            var data = mlContext.Data.LoadFromTextFile<AudioData>(
                _paths.FeaturesCsvPath,
                separatorChar: ',',
                hasHeader: true
            );

            var split = mlContext.Data.TrainTestSplit(data, 0.2);

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(split.TrainSet);

            var metrics = mlContext.MulticlassClassification.Evaluate(model.Transform(split.TestSet));

            Directory.CreateDirectory(Path.GetDirectoryName(_paths.ModelPath)!);
            mlContext.Model.Save(model, data.Schema, _paths.ModelPath);

            Console.WriteLine("========================================");
            Console.WriteLine("Audio Classification Training Summary");
            Console.WriteLine("========================================");
            Console.WriteLine($"Model Name     : {ModelName}");
            Console.WriteLine($"Micro Accuracy : {metrics.MicroAccuracy:P2}");
            Console.WriteLine($"Macro Accuracy : {metrics.MacroAccuracy:P2}");
            Console.WriteLine($"Log Loss       : {metrics.LogLoss:F4}");
            Console.WriteLine($"Model Path     : {_paths.ModelPath}");
            Console.WriteLine("========================================");

            return new TrainingResult
            {
                ModelName = ModelName,
                MicroAccuracy = metrics.MicroAccuracy,
                MacroAccuracy = metrics.MacroAccuracy,
                LogLoss = metrics.LogLoss,
                ModelPath = _paths.ModelPath
            };
        }
    }
}
