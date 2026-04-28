using AudioClassification.Services;
using AudioClassification.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AudioClassification.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly AudioTrainer _trainer;
        private readonly PathSettings _paths;

        public AudioController(IWebHostEnvironment environment)
        {
            _paths = PathResolver.Build(environment.ContentRootPath);
            _trainer = new AudioTrainer(_paths);
        }
        [Authorize]

        [HttpGet("generate-csv")]
        public IActionResult GenerateCsv()
        {
            try
            {
                var result = CsvGenerator.Generate(_paths.DatasetPath, _paths.FeaturesCsvPath);
                return Ok(new
                {
                    Message = "CSV generated successfully",
                    Path = result.OutputPath,
                    ProcessedFiles = result.ProcessedFiles,
                    SkippedFiles = result.SkippedFiles,
                    Warnings = result.Warnings
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("train")]
        public IActionResult Train()
        {
            try
            {
                var result = _trainer.Train();
                return Ok(new
                {
                    Message = "Model trained successfully",
                    ModelName = result.ModelName,
                    MicroAccuracy = result.MicroAccuracy,
                    MacroAccuracy = result.MacroAccuracy,
                    LogLoss = result.LogLoss,
                    Path = result.ModelPath
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("predict")]
        public IActionResult Predict()
        {
            try
            {
                var predictor = new AudioPredictor(_paths);
                var result = predictor.Predict();

                return Ok(new
                {
                    Prediction = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]

        [HttpPost("predict")]
        [Consumes("multipart/form-data")]
        public IActionResult PredictAudio(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var filePath = Path.Combine(_paths.TempPath, Path.GetFileName(file.FileName));
            Directory.CreateDirectory(_paths.TempPath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            try
            {
                var features = FeatureExtractor.ExtractFeatures(filePath);
                var predictor = new AudioPredictor(_paths);
                var result = predictor.PredictFromFeatures(features);

                return Ok(new
                {
                    File = file.FileName,
                    Prediction = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
