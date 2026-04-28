using Microsoft.ML.Data;

namespace AudioClassification.Models
{
    public class AudioData
    {
        [LoadColumn(0)]
        public string Label { get; set; } = "";

        [LoadColumn(1, 4)]
        [VectorType(4)]
        public float[] Features { get; set; } = new float[4];
    }
}
