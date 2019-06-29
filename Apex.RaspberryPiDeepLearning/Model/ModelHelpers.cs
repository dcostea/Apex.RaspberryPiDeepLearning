using System;
using System.IO;
using System.Linq;

namespace Apex.RaspberryPiDeepLearning.Model
{
    public static class ModelHelpers
    {
        static readonly FileInfo _dataRoot;

        static ModelHelpers()
        {
            _dataRoot = new FileInfo(typeof(ModelHelpers).Assembly.Location);
        }

        public static string GetAssetsPath(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                return null;
            }

            return Path.Combine(paths.Prepend(_dataRoot.Directory.FullName).ToArray());
        }

        public static string DeleteAssets(params string[] paths)
        {
            var location = GetAssetsPath(paths);

            if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
                File.Delete(location);
            return location;
        }

        public static (string, float) GetBestLabel(string[] labels, float[] probs)
        {
            var max = probs.Max();
            var index = probs.AsSpan().IndexOf(max);
            return (labels[index], max);
        }

        public static (string, float) GetSecondLabel(string[] labels, float[] probs)
        {
            var max = probs.Max();
            var second = probs.Where(p => p != max).Max();
            var index = probs.AsSpan().IndexOf(second);
            return (labels[index], second);
        }

        public static (string, float) GetThirdLabel(string[] labels, float[] probs)
        {
            var max = probs.Max();
            var second = probs.Where(p => p != max).Max();
            var third = probs.Where(p => p != max && p!= second).Max();
            var index = probs.AsSpan().IndexOf(third);
            return (labels[index], third);
        }

        public static string[] ReadLabels(string labelsLocation)
        {
            return File.ReadAllLines(labelsLocation);
        }
    }
}
