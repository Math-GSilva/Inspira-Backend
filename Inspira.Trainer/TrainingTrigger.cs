using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Inspira.Trainer.Services;

namespace Inspira.Trainer
{
    public class TrainingTrigger
    {
        private readonly TrainingService _trainingService;
        private readonly ILogger _logger;

        public TrainingTrigger(TrainingService trainingService, ILoggerFactory loggerFactory)
        {
            _trainingService = trainingService;
            _logger = loggerFactory.CreateLogger<TrainingTrigger>();
        }

        [Function("DailyTrainingJob")]
        public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Job de treinamento iniciado em: {DateTime.Now}");

            await _trainingService.RunOnceAsync();

            _logger.LogInformation($"Job de treinamento finalizado.");
        }
    }
}