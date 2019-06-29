using System;
using System.IO;
using System.Linq;
using static System.Console;

namespace Apex.RaspberryPiDeepLearning.Model
{
    public static class ConsoleHelpers
    {
        public static void ConsoleWriteHeader(params string[] lines)
        {
            var defaultColor = ForegroundColor;
            ForegroundColor = ConsoleColor.Yellow;
            WriteLine(" ");
            foreach (var line in lines)
            {
                WriteLine(line);
            }
            var maxLength = lines.Select(x => x.Length).Max();
            WriteLine(new String('#', maxLength));
            ForegroundColor = defaultColor;
        }

        public static void ConsolePressAnyKey()
        {
            ForegroundColor = ConsoleColor.Green;
            WriteLine(" ");
            WriteLine("Press any key to finish.");
            ReadKey();
        }

        public static void ConsoleWriteException(params string[] lines)
        {
            var defaultColor = ForegroundColor;
            ForegroundColor = ConsoleColor.Red;
            const string exceptionTitle = "EXCEPTION";
            WriteLine(" ");
            WriteLine(exceptionTitle);
            WriteLine(new string('#', exceptionTitle.Length));
            ForegroundColor = defaultColor;
            foreach (var line in lines)
            {
                WriteLine(line);
            }
        }

        public static void ConsoleWriteImagePrediction(string ImagePath, string PredictedLabel, float Probability)
        {
            var defaultForeground = ForegroundColor;
            var labelColor = ConsoleColor.Magenta;
            var probColor = ConsoleColor.Blue;

            Write("ImagePath: ");
            ForegroundColor = labelColor;
            Write($"{Path.GetFileName(ImagePath)}");
            ForegroundColor = defaultForeground;
            Write(" predicted as ");
            ForegroundColor = labelColor;
            Write(PredictedLabel);
            ForegroundColor = defaultForeground;
            Write(" with score ");
            ForegroundColor = probColor;
            Write($"{Probability:P2}");
            ForegroundColor = defaultForeground;
            WriteLine("");
        }

        public static void PrintCancellationReason(string reason)
        {
            WriteLine($"CANCELED: Reason={reason}");
        }

        public static void PrintCancellationError(string errorCode, string errorDetails)
        {
            WriteLine($"CANCELED: ErrorCode={errorCode}");
            WriteLine($"CANCELED: ErrorDetails=[{errorDetails}]");
            WriteLine($"CANCELED: Did you update the subscription info?");
        }
    }
}