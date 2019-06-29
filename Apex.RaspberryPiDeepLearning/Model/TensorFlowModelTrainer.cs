using System;
using System.Linq;
using Microsoft.ML;
using Apex.RaspberryPiDeepLearning.ImageData;
using static Apex.RaspberryPiDeepLearning.Model.ConsoleHelpers;

namespace Apex.RaspberryPiDeepLearning.Model
{
    public class TensorFlowModelTrainer
    {
        private readonly string trainDataLocation;
        private readonly string trainImagesFolder;
        private readonly string inputModelLocation;
        private readonly string outputModelLocation;
        private readonly MLContext mlContext;

        private static readonly string LabelTokey = nameof(LabelTokey);
        private static readonly string ImageReal = nameof(ImageReal);
        private static readonly string PredictedLabelValue = nameof(PredictedLabelValue);

        public TensorFlowModelTrainer(string trainDataLocation, string trainImagesFolder, string inputModelLocation, string outputModelLocation)
        {
            this.trainDataLocation = trainDataLocation;
            this.trainImagesFolder = trainImagesFolder;
            this.inputModelLocation = inputModelLocation;
            this.outputModelLocation = outputModelLocation;
            mlContext = new MLContext(seed: 1);
        }

        private struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const float scale = 1;
            public const bool channelsLast = true;
        }

        public void BuildAndTrain()
        {
            var featurizerModelLocation = inputModelLocation;

            var data = mlContext.Data.LoadFromTextFile<ImageNetData>(path: trainDataLocation, hasHeader: false);

            var pipeline = mlContext
                .Transforms.Conversion.MapValueToKey(
                    outputColumnName: LabelTokey,
                    inputColumnName: "Label")
                .Append(mlContext.Transforms.LoadImages(
                    outputColumnName: "input",
                    imageFolder: trainImagesFolder,
                    inputColumnName: nameof(ImageNetData.ImagePath)))
                .Append(mlContext.Transforms.ResizeImages(
                    outputColumnName: "input",
                    imageWidth: ImageNetSettings.imageWidth,
                    imageHeight: ImageNetSettings.imageHeight,
                    inputColumnName: "input"))
                .Append(mlContext.Transforms.ExtractPixels(
                    outputColumnName: "input",
                    interleavePixelColors: ImageNetSettings.channelsLast,
                    offsetImage: ImageNetSettings.mean))
                .Append(mlContext.Model.LoadTensorFlowModel(modelLocation: featurizerModelLocation)
                    .ScoreTensorFlowModel(
                        inputColumnNames: new[] { "input" },
                        outputColumnNames: new[] { "softmax2_pre_activation" }, 
                        addBatchDimensionInput: true))
                .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(
                    labelColumnName: LabelTokey, 
                    featureColumnName: "softmax2_pre_activation"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue, "PredictedLabel"))
                .AppendCacheCheckpoint(mlContext);

            // Train the model
            ConsoleWriteHeader("Training classification model");
            ITransformer model = pipeline.Fit(data);

            // Process the training data through the model
            // This is an optional step, but it's useful for debugging issues
            var trainData = model.Transform(data);
            var loadedModelOutputColumnNames = trainData.Schema
                .Where(col => !col.IsHidden).Select(col => col.Name);
            var trainData2 = mlContext.Data.CreateEnumerable<ImageNetPipeline>(trainData, false, true).ToList();
            trainData2.ForEach(pr => ConsoleWriteImagePrediction(pr.ImagePath, pr.PredictedLabelValue, pr.Score.Max()));

            // Get some performance metric on the model using training data            
            var classificationContext = mlContext.MulticlassClassification;
            ConsoleWriteHeader("Classification metrics");
            var metrics = classificationContext.Evaluate(trainData, labelColumnName: LabelTokey, predictedLabelColumnName: "PredictedLabel");
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            // Save the model to assets/outputs
            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(outputModelLocation);

            mlContext.Model.Save(model, trainData.Schema, outputModelLocation);

            Console.WriteLine($"Model saved: {outputModelLocation}");
        }
    }
}
