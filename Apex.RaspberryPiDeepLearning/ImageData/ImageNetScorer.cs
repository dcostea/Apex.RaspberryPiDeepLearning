using Apex.RaspberryPiDeepLearning.Model;
using Microsoft.ML.Data;

namespace Apex.RaspberryPiDeepLearning.ImageData
{
    public class ImageNetScorer
    {
        [ColumnName(TensorFlowModelScorer.InceptionSettings.outputTensorName)]
        public float[] PredictedLabels;
    }

    public class ImageNetWithLabelScorer : ImageNetScorer
    {
        public string Label { get; set; }

        public ImageNetWithLabelScorer()
        {
        }

        public ImageNetWithLabelScorer(ImageNetScorer pred, string label)
        {
            Label = label;
            PredictedLabels = pred.PredictedLabels;
        }
    }
}
