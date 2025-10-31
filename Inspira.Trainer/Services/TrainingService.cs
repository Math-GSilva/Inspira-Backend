using Inspira.Trainer.Data;
using Inspira.Trainer.Models;
using Inspira.Trainer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inspira.Trainer.Services
{
    public class TrainingService : IHostedService
    {
        private readonly ILogger<TrainingService> _logger;
        private readonly TrainingDbContext _context;
        private readonly MLContext _mlContext;
        private readonly IUsuarioPreferenciaRepository _prefRepo;

        public TrainingService(
            ILogger<TrainingService> logger,
            TrainingDbContext context,
            IUsuarioPreferenciaRepository prefRepo)
        {
            _logger = logger;
            _context = context;
            _prefRepo = prefRepo;
            _mlContext = new MLContext();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // ... (Nenhuma mudança no StartAsync) ...
            _logger.LogInformation("Serviço de Treinamento e População de IA iniciado.");

            try
            {
                _logger.LogInformation("Carregando dados de curtidas do PostgreSQL...");
                var trainingData = await LoadDataAsync();

                if (trainingData.Count == 0)
                {
                    _logger.LogWarning("Nenhum dado de curtida encontrado. Abortando job.");
                    return;
                }
                _logger.LogInformation($"Carregados {trainingData.Count} registros de curtidas agregadas.");

                _logger.LogInformation("Configurando pipeline de treinamento...");
                var pipeline = BuildTrainingPipeline();

                _logger.LogInformation("Iniciando treinamento do modelo (em memória)...");
                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
                var model = pipeline.Fit(dataView);
                _logger.LogInformation("Treinamento concluído.");

                string modelPath = "model.zip";
                _logger.LogInformation($"Salvando backup do modelo em: {Path.GetFullPath(modelPath)}");
                _mlContext.Model.Save(model, dataView.Schema, modelPath);

                _logger.LogInformation("Iniciando job de cálculo e salvamento de scores...");
                await UpdateAllUserPreferencesAsync(model, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro crítico durante o job de IA.");
            }
            finally
            {
                _logger.LogInformation("Job de IA concluído.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task<List<RecommendationInput>> LoadDataAsync()
        {
            // ... (Nenhuma mudança no LoadDataAsync) ...
            var curtidas = await _context.Curtidas
                .AsNoTracking()
                .Include(c => c.ObraDeArte)
                .Select(c => new
                {
                    UsuarioId = c.UsuarioId.ToString(),
                    CategoriaId = c.ObraDeArte.CategoriaId.ToString(),
                    Rating = 1.0f
                })
                .ToListAsync();

            var aggregatedData = curtidas
                .GroupBy(c => new { c.UsuarioId, c.CategoriaId })
                .Select(g => new RecommendationInput
                {
                    UsuarioId = g.Key.UsuarioId,
                    CategoriaId = g.Key.CategoriaId,
                    Rating = g.Count() // Propriedade C# "Rating"
                })
                .ToList();

            return aggregatedData;
        }

        private IEstimator<ITransformer> BuildTrainingPipeline()
        {
            var dataProcessingPipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                    inputColumnName: "UsuarioId",
                    outputColumnName: "UserIdEncoded")
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                    inputColumnName: "CategoriaId",
                    outputColumnName: "ItemIdEncoded"));

            var trainer = _mlContext.Recommendation().Trainers.MatrixFactorization(
                new MatrixFactorizationTrainer.Options
                {
                    // --- =================================== ---
                    // --- ===         A CORREÇÃO          === ---
                    // --- =================================== ---
                    LabelColumnName = "Label", // <-- MUDADO DE VOLTA PARA "Label"
                    // --- =================================== ---
                    MatrixColumnIndexColumnName = "UserIdEncoded",
                    MatrixRowIndexColumnName = "ItemIdEncoded",
                    NumberOfIterations = 20,
                    ApproximationRank = 8,
                    LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass
                });

            return dataProcessingPipeline.Append(trainer);
        }

        private async Task UpdateAllUserPreferencesAsync(ITransformer model, CancellationToken token)
        {
            // ... (Nenhuma mudança no UpdateAllUserPreferencesAsync) ...
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<RecommendationInput, RecommendationOutput>(model);

            var allUsers = await _context.Usuarios.AsNoTracking()
                                .Select(u => new { u.Id, u.NomeUsuario }).ToListAsync(token);
            var allCategories = await _context.Categorias.AsNoTracking()
                                .Select(c => new { c.Id, c.Nome }).ToListAsync(token);

            _logger.LogInformation($"Calculando scores para {allUsers.Count} usuários e {allCategories.Count} categorias.");

            var input = new RecommendationInput();

            foreach (var user in allUsers)
            {
                if (token.IsCancellationRequested)
                {
                    _logger.LogWarning("Job de IA cancelado no meio do processamento.");
                    break;
                }

                var newPreferences = new List<UsuarioPreferenciaCategoria>();
                input.UsuarioId = user.Id.ToString();

                foreach (var category in allCategories)
                {
                    input.CategoriaId = category.Id.ToString();

                    var prediction = predictionEngine.Predict(input);

                    if (!float.IsNaN(prediction.Score) && prediction.Score > 0)
                    {
                        newPreferences.Add(new UsuarioPreferenciaCategoria
                        {
                            UsuarioId = user.Id,
                            CategoriaId = category.Id,
                            Score = prediction.Score
                        });
                    }
                }

                if (newPreferences.Any())
                {
                    await _prefRepo.BatchUpdatePreferenciasAsync(user.Id, newPreferences);
                }

                _logger.LogInformation($"Scores para o usuário '{user.NomeUsuario}' (ID: {user.Id}) atualizados. {newPreferences.Count} categorias pontuadas.");
            }

            _logger.LogInformation("Cálculo e salvamento de scores concluído.");
        }
    }
}