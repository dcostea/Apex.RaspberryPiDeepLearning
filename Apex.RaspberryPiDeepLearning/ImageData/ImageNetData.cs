using Microsoft.ML.Data;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Apex.RaspberryPiDeepLearning.ImageData
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;

        public static IEnumerable<ImageNetData> ReadFromCsv(string file, string folder)
        {
            return File.ReadAllLines(file)
             .Select(x => x.Split('\t'))
             .Select(x => new ImageNetData { ImagePath = Path.Combine(folder, x[0]), Label = x[1] });
        }
    }

    public class ImageNetDataProbability : ImageNetData
    {
        public float Probability { get; set; }

        public string PredictedLabel { get; set; }
        public RectangleF Rectangle { get; set; }
    }
}
