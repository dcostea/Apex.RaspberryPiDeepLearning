using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Apex.RaspberryPiDeepLearning.ImageData;
using System.IO;
using static Apex.RaspberryPiDeepLearning.Model.ModelHelpers;

namespace Apex.RaspberryPiDeepLearning.Model
{
    public class TensorFlowModelScorer
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly string labelsLocation;
        private readonly MLContext mlContext;
        private static readonly string ImageReal = nameof(ImageReal);

        public TensorFlowModelScorer(string dataLocation, string imagesFolder, string modelLocation, string labelsLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
            this.labelsLocation = labelsLocation;
            mlContext = new MLContext();
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const bool channelsLast = true;
        }

        public struct InceptionSettings
        {
            // for checking tensor names, you can use tools like Netron,
            // which is installed by Visual Studio AI Tools

            // input tensor name
            public const string inputTensorName = "input";

            // output tensor name
            public const string outputTensorName = "softmax2";
        }

        /// <summary>
        /// Most 5 relevant scores
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ImageNetDataProbability> ScoreTestImage(string testImage)
        {
            Console.WriteLine("Score test image");

            var model = LoadAndScoreModel(dataLocation, imagesFolder, modelLocation);

            return PredictTestImageUsingModel(testImage, labelsLocation, model);
        }

        private PredictionEngine<ImageNetData, ImageNetPredictions> LoadAndScoreModel(string dataLocation, string imagesFolder, string modelLocation)
        {
            var data = mlContext.Data.LoadFromTextFile<ImageNetData>(dataLocation, hasHeader: true);

            var pipeline = mlContext
                .Transforms.LoadImages(
                    outputColumnName: "input",
                    imageFolder: imagesFolder,
                    inputColumnName: nameof(ImageNetData.ImagePath))
                .Append(mlContext.Transforms.ResizeImages(
                    outputColumnName: "input",
                    imageWidth: ImageNetSettings.imageWidth,
                    imageHeight: ImageNetSettings.imageHeight,
                    inputColumnName: "input"))
                .Append(mlContext.Transforms.ExtractPixels(
                    outputColumnName: "input",
                    interleavePixelColors: ImageNetSettings.channelsLast,
                    offsetImage: ImageNetSettings.mean))
                .Append(mlContext.Model.LoadTensorFlowModel(modelLocation).ScoreTensorFlowModel(
                    inputColumnNames: new[] { "input" },
                    outputColumnNames: new[] { "softmax2" },
                    addBatchDimensionInput: true));

            ITransformer model = pipeline.Fit(data);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPredictions>(model);

            return predictionEngine;
        }

        protected IEnumerable<ImageNetDataProbability> PredictTestImageUsingModel(
            string testImage,
            string labelsLocation,
            PredictionEngine<ImageNetData, ImageNetPredictions> model)
        {
            Console.WriteLine("Predict test image");

            var labels = ReadLabels(labelsLocation);

            var imageData = new ImageNetDataProbability()
            {
                ImagePath = testImage,
                Label = Path.GetFileNameWithoutExtension(testImage)
            };

            var probs = model.Predict(imageData).PredictedLabels;

            (imageData.PredictedLabel, imageData.Probability) = GetBestLabel(labels, probs);
            yield return imageData;

            (imageData.PredictedLabel, imageData.Probability) = GetSecondLabel(labels, probs);
            yield return imageData;

            (imageData.PredictedLabel, imageData.Probability) = GetThirdLabel(labels, probs);
            yield return imageData;
        }
    }
}
