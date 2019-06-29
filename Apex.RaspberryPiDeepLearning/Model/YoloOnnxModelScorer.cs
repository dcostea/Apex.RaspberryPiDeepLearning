using Apex.RaspberryPiDeepLearning.ImageData;
using Apex.RaspberryPiDeepLearning.YoloParser;
using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Apex.RaspberryPiDeepLearning.Model
{
    public class YoloOnnxModelScorer
    {
        private readonly string imagesLocation;
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly MLContext mlContext;

        private IList<YoloBoundingBox> _boxes = new List<YoloBoundingBox>();
        private readonly YoloWinMlParser _parser = new YoloWinMlParser();

        public YoloOnnxModelScorer(string imagesLocation, string imagesFolder, string modelLocation)
        {
            this.imagesLocation = imagesLocation;
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
            mlContext = new MLContext();
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 416;
            public const int imageWidth = 416;
        }

        public struct TinyYoloModelSettings
        {
            // for checking Tiny Yolo2 Model input and  output  parameter names,
            // you can use tools like Netron, which is installed by Visual Studio AI Tools

            // input tensor name
            public const string ModelInput = "image";

            // output tensor name
            public const string ModelOutput = "grid";
        }

        public IEnumerable<ImageNetDataProbability> ScoreTestImage(string imageName)
        {
            var model = LoadModel(imagesFolder, modelLocation);

            return PredictDataUsingModel(imageName, model);
        }

        private PredictionEngine<ImageNetData, ImageNetPredictionOnnx> LoadModel(string imagesFolder, string modelLocation)
        {
            var data = mlContext.Data.LoadFromTextFile<ImageNetData>(imagesLocation, hasHeader: true);

            var pipeline = mlContext
                .Transforms.LoadImages(outputColumnName: "image", imageFolder: imagesFolder, inputColumnName: nameof(ImageNetData.ImagePath))
                .Append(mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "image"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            var model = pipeline.Fit(data);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPredictionOnnx>(model);

            return predictionEngine;
        }

        protected IEnumerable<ImageNetDataProbability> PredictDataUsingModel(
            string imageName,
            PredictionEngine<ImageNetData, ImageNetPredictionOnnx> model)
        {
            Console.WriteLine("=====Identify the objects in the images=====");
            Console.WriteLine("");

            var imageData = new ImageNetDataProbability()
            {
                ImagePath = imageName,
                Label = Path.GetFileNameWithoutExtension(imageName)
            };

            var probs = model.Predict(imageData).PredictedLabels;
            _boxes = _parser.ParseOutputs(probs);
            var filteredBoxes = _parser.NonMaxSuppress(_boxes, 10, 0.5F);

            Console.WriteLine($"The objects in the image {imageData.Label} are detected as below...");
            foreach (var box in filteredBoxes)
            {
                Console.WriteLine($"{box.Label} ({box.Confidence:P2})");
                imageData.Probability = box.Confidence;
                imageData.PredictedLabel = box.Label;
                imageData.Rectangle = box.Rect;

                yield return imageData;
            }
        }
    }
}
