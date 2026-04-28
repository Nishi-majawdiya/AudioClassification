using Microsoft.ML;
using AudioClassification.Models;

namespace AudioClassification.Services
{
    public class AudioPredictor
    {
        private readonly PredictionEngine<AudioData, AudioPrediction> engine;

        public AudioPredictor(PathSettings paths)
        {
            var mlContext = new MLContext();

            if (!File.Exists(paths.ModelPath))
            {
                throw new FileNotFoundException(
                    "Trained model was not found. Train it first using /api/audio/train.",
                    paths.ModelPath);
            }

            var model = mlContext.Model.Load(paths.ModelPath, out _);

            engine = mlContext.Model.CreatePredictionEngine<AudioData, AudioPrediction>(model);
        }

        public string Predict()
        {
            var sample = new AudioData
            {
                Features = new float[]
                {
                    0.12f, 0.45f, 0.33f, 0.21f
                }
            };

            var result = engine.Predict(sample);

            return result.PredictedLabel;
        }
        public string PredictFromFeatures(float[] features)
        {
            var input = new AudioData
            {
                Features = features
            };

            var result = engine.Predict(input);

            return result.PredictedLabel;
        }
    }
}
