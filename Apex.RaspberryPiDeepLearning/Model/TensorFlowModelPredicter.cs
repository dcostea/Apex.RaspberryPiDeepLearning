using System;
using System.Linq;
using Apex.RaspberryPiDeepLearning.ImageData;
using System.IO;
using Microsoft.ML;
using static Apex.RaspberryPiDeepLearning.Model.ConsoleHelpers;

namespace Apex.RaspberryPiDeepLearning.Model
{
    public class TensorFlowModelPredicter
    {
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly string dataLocation;
        private readonly MLContext mlContext;

        public TensorFlowModelPredicter(string dataLocation, string imagesFolder, string modelLocation)
        {
            this.imagesFolder = imagesFolder;
            this.dataLocation = dataLocation;
            this.modelLocation = modelLocation;
            mlContext = new MLContext(seed: 1);
        }

        public string TensorFlowPredictImage(string file)
        {
            ConsoleWriteHeader("Loading model");

            // Load the model
            ITransformer loadedModel = mlContext.Model.Load(modelLocation, out _);
            Console.WriteLine($"Model loaded: {modelLocation}");

            // Make prediction function (input = ImageNetData, output = ImageNetPrediction)
            var predictor = mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPrediction>(loadedModel);

            var image = new 
            {
                pred = predictor.Predict(new ImageNetData { ImagePath = Path.Combine(imagesFolder, file), Label = Path.GetFileNameWithoutExtension(file) }),
                ImagePath = Path.Combine(imagesFolder, file),
                Label = Path.GetFileNameWithoutExtension(file)
            };

            ConsoleWriteHeader("Making classifications");
            ConsoleWriteImagePrediction(Path.GetFileName(image.ImagePath), image.pred.PredictedLabelValue, image.pred.Score.Max());

            return $"{image.pred.PredictedLabelValue} ({image.pred.Score.Max():P2})";
        }
    }
}
