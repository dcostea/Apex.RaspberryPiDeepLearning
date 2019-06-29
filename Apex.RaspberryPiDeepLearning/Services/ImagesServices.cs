using Apex.RaspberryPiDeepLearning.ImageData;
using Apex.RaspberryPiDeepLearning.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using static Apex.RaspberryPiDeepLearning.Model.ConsoleHelpers;

namespace Apex.RaspberryPiDeepLearning.Services
{
    public static class ImagesServices
    {
        static readonly string assetsRelativePath = @"../../../assets";
        static readonly string assetsPath = GetAbsolutePath(assetsRelativePath);

        static readonly string tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv");
        static readonly string trainTagsTsv = Path.Combine(assetsPath, "inputs", "train", "tags.tsv");
        static readonly string yoloImagesFolder = Path.Combine(assetsPath, "inputs", "yolo_data");
        static readonly string inceptionImagesFolder = Path.Combine(assetsPath, "inputs", "inception_data");
        static readonly string inceptionTrainImagesFolder = Path.Combine(assetsPath, "inputs", "inception_train_data");
        static readonly string trainImagesFolder = Path.Combine(assetsPath, "inputs", "train");
        static readonly string inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb");
        //static readonly string inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "classify_image_graph_def.pb");
        static readonly string imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

        //static readonly string modelFilePath = Path.Combine(assetsPath, "inputs", "yolo", "yolov3.onnx");
        //static readonly string modelFilePath = Path.Combine(assetsPath, "inputs", "yolo", "Tiny-YoloV2-1.2.onnx");
        static readonly string modelFilePath = Path.Combine(assetsPath, "inputs", "yolo", "TinyYolo2_model.onnx");
        static readonly string labelsTxt = Path.Combine(assetsPath, "inputs", "inception", "imagenet_comp_graph_label_strings.txt");

        public static void TensorFlowTrain()
        {
            try
            {
                var modelBuilder = new TensorFlowModelTrainer(trainTagsTsv, trainImagesFolder, inceptionPb, imageClassifierZip);
                modelBuilder.BuildAndTrain();
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }
        }

        public static string TensorFlowPredict(string imageName)
        {
            if (imageName == null)
            {
                throw new ArgumentNullException(nameof(imageName));
            }

            try
            {
                var modelScorer = new TensorFlowModelPredicter(tagsTsv, inceptionImagesFolder, imageClassifierZip);
                return modelScorer.TensorFlowPredictImage(imageName);
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }

            return string.Empty;
        }

        public static IEnumerable<string> TensorFlowScore(string imageName)
        {
            if (imageName == null)
            {
                throw new ArgumentNullException(nameof(imageName));
            }

            var modelScorer = new TensorFlowModelScorer(tagsTsv, inceptionImagesFolder, inceptionPb, labelsTxt);
            var predicted = modelScorer.ScoreTestImage(imageName);

            foreach (var item in predicted)
            {
                yield return $"{item.PredictedLabel} ({item.Probability:P2})";
            }
        }
        
        public static IEnumerable<ImageNetDataProbability> YoloOnnxPredict(string imageName)
        {
            if (imageName == null)
            {
                throw new ArgumentNullException(nameof(imageName));
            }

            var modelScorer = new YoloOnnxModelScorer(tagsTsv, yoloImagesFolder, modelFilePath);
            return modelScorer.ScoreTestImage(imageName);
        }

        public static HashSet<string> GetImages(string data)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);

            var tagsTsv = Path.Combine(assetsPath, "inputs", data, "tags.tsv");
            var imagesFolder = Path.Combine(assetsPath, "inputs", data);

            var testData = ImageNetData.ReadFromCsv(tagsTsv, imagesFolder);

            return testData.Select(t => t.Label).ToHashSet();
        }

        public static void DownloadImage(string path, string url, string imageName, int? imageIndex)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);

            var tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv");
            var imagesFolder = Path.Combine(assetsPath, "inputs", "inception_data");

            using (var client = new WebClient())
            {
                if (imageIndex is null)
                {
                    client.DownloadFile($"{url}/img/{imageName}.jpg", $"{imagesFolder}/{imageName}.jpg");
                    client.DownloadFile($"{url}/img/{imageName}.jpg", $"{path}/images/{imageName}.jpg");
                }
                else
                {
                    client.DownloadFile($"{url}/img/{imageName}{imageIndex.Value}.jpg", $"{imagesFolder}/{imageName}{imageIndex.Value}.jpg");
                    client.DownloadFile($"{url}/img/{imageName}{imageIndex.Value}.jpg", $"{path}/images/{imageName}{imageIndex.Value}.jpg");

                    using (StreamWriter sw = File.AppendText(tagsTsv))
                    {
                        sw.Write($"\n{imageName}{imageIndex.Value}.jpg\t{imageName}");
                    }
                }
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;
            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
