using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;
using static System.Console;

namespace Apex.RaspberryPiDeepLearning.Model
{
    public class SpeechHelpers
    {
        private readonly CognitiveServicesSettings cognitiveServicesSettings;

        public SpeechHelpers(CognitiveServicesSettings cognitiveServicesSettings)
        {
            this.cognitiveServicesSettings = cognitiveServicesSettings;
        }

        public async Task Listen()
        {
            var speechConfig = SpeechConfig.FromSubscription(cognitiveServicesSettings.SpeakSdkKey, cognitiveServicesSettings.SpeakSdkRegion);

            // Creates a speech recognizer.
            using var recognizer = new SpeechRecognizer(speechConfig);

            Console.WriteLine("Say something...");

            // Starts speech recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result. 
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query. 
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
            var result = await recognizer.RecognizeOnceAsync();

            // Checks result.
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                WriteLine($"We recognized: {result.Text}");
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                WriteLine($"NOMATCH: Speech could not be recognized.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                ConsoleHelpers.PrintCancellationReason(cancellation.Reason.ToString());
                if (cancellation.Reason == CancellationReason.Error)
                {
                    ConsoleHelpers.PrintCancellationError(cancellation.ErrorCode.ToString(), cancellation.ErrorDetails);
                }
            }
        }

        public async Task Speak(string text)
        {
            var speechConfig = SpeechConfig.FromSubscription(cognitiveServicesSettings.SpeakSdkKey, cognitiveServicesSettings.SpeakSdkRegion);

            // Creates a speech synthesizer using the default speaker as audio output.
            using var synthesizer = new SpeechSynthesizer(speechConfig);

            using var result = await synthesizer.SpeakTextAsync(text);
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                WriteLine($"Speech synthesized to speaker for text [{text}]");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                ConsoleHelpers.PrintCancellationReason(cancellation.Reason.ToString());
                if (cancellation.Reason == CancellationReason.Error)
                {
                    ConsoleHelpers.PrintCancellationError(cancellation.ErrorCode.ToString(), cancellation.ErrorDetails);
                }
            }
        }
    }
}
