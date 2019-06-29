using Apex.RaspberryPiDeepLearning.Model;
using Microsoft.ML.Data;

namespace Apex.RaspberryPiDeepLearning.ImageData
{
    public class ImageNetPredictions
    {
        [ColumnName(TensorFlowModelScorer.InceptionSettings.outputTensorName)]
        public float[] PredictedLabels;
    }
}
