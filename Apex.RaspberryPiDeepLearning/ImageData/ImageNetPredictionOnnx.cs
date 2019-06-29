using Apex.RaspberryPiDeepLearning.Model;
using Microsoft.ML.Data;

namespace Apex.RaspberryPiDeepLearning.ImageData
{
    public class ImageNetPredictionOnnx
    {
        [ColumnName(YoloOnnxModelScorer.TinyYoloModelSettings.ModelOutput)]
        public float[] PredictedLabels;
    }
}
