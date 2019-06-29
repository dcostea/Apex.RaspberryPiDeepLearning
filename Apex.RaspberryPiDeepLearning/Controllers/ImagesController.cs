using Apex.RaspberryPiDeepLearning.Model;
using Apex.RaspberryPiDeepLearning.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Threading.Tasks;

namespace Apex.RaspberryPiDeepLearning.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly RaspberryPiSettings raspberryPiSettings;
        private readonly CognitiveServicesSettings cognitiveServicesSettings;

        public ImagesController(IOptions<CognitiveServicesSettings> cognitiveServicesSettings, IOptions<RaspberryPiSettings> raspberryPiSettings)
        {
            this.raspberryPiSettings = raspberryPiSettings.Value;
            this.cognitiveServicesSettings = cognitiveServicesSettings.Value;
        }

        [HttpGet("train")]
        public IActionResult ImageTrain()
        {
            ImagesServices.TensorFlowTrain();

            return Ok();
        }

        [HttpGet("yolo/{imageName}")]
        public async Task<IActionResult> YoloOnnxPredict(string imageName)
        {
            var metrics = ImagesServices.YoloOnnxPredict(imageName);
            //var prediction = metrics.Select(m => $"{m.PredictedLabel} {m.Probability:P2}");
            //Task.Run(() => SpeechHelper.Speak($"Detected as: {string.Join(",", prediction)}"));
            //SpeechHelper.Speak($"Detected as: {string.Join(",", prediction)}");

            return Ok(metrics);
        }

        [HttpGet("predict/{imageName}")]
        public IActionResult TensorFlowPredict(string imageName)
        {
            var prediction = ImagesServices.TensorFlowPredict(imageName);
            var speech = new SpeechHelpers(cognitiveServicesSettings);
            Task.Run(() => speech.Speak($"Predicted as {prediction}"));

            return Ok("<br />Predicted as: <br /><br />" + string.Join("<br />", prediction));
        }

        [HttpGet("score/{imageName}")]
        public IActionResult TensorFlowScore(string imageName)
        {
            var score = ImagesServices.TensorFlowScore(imageName);
            var speech = new SpeechHelpers(cognitiveServicesSettings);
            Task.Run(() => speech.Speak($"Scored as {string.Join(",", score)}"));

            return Ok("<br />Scored as: <br /><br />" + string.Join("<br />", score));
        }

        [HttpGet("yolo/image")]
        public IActionResult GetYoloImages()
        {
            var images = ImagesServices.GetImages("yolo_data");

            return Ok(images);
        }

        [HttpGet("inception/image")]
        public IActionResult GetInceptionImages()
        {
            var images = ImagesServices.GetImages("inception_data");

            return Ok(images);
        }

        [HttpGet("inception/train/image")]
        public IActionResult GetInceptionTrainImages()
        {
            var images = ImagesServices.GetImages("inception_train_data");

            return Ok(images);
        }

        [HttpGet("down/{imageName}/{imageIndex:int?}")]
        public IActionResult DownloadImage([FromServices] IWebHostEnvironment env, string imageName, int? imageIndex)
        {
            try
            {
                var url = raspberryPiSettings.Server;
                ImagesServices.DownloadImage(env.WebRootPath, url, imageName, imageIndex);
            }
            catch (WebException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
