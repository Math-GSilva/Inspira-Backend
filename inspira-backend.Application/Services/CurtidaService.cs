using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Services
{
    public class CurtidaService : ICurtidaService
    {
        private readonly ICurtidaRepository _curtidaRepository;
        private readonly IObraDeArteRepository _obraDeArteRepository;

        public CurtidaService(ICurtidaRepository curtidaRepository, IObraDeArteRepository obraDeArteRepository)
        {
            _curtidaRepository = curtidaRepository;
            _obraDeArteRepository = obraDeArteRepository;
        }

        public async Task<CurtidaStatusDto?> CurtirAsync(Guid obraDeArteId, Guid userId)
        {
            // Verifica se a obra de arte existe
            var obraDeArte = await _obraDeArteRepository.GetByIdAsync(obraDeArteId);
            if (obraDeArte == null) return null;

            // Verifica se o utilizador já não curtiu esta obra
            var curtidaExistente = await _curtidaRepository.GetByUserAndArtAsync(userId, obraDeArteId);
            if (curtidaExistente == null)
            {
                var novaCurtida = new Curtida { UsuarioId = userId, ObraDeArteId = obraDeArteId };
                await _curtidaRepository.AddAsync(novaCurtida);
            }

            // Recalcula o total de curtidas e retorna o status
            var totalCurtidas = await _curtidaRepository.CountByObraDeArteIdAsync(obraDeArteId);
            return new CurtidaStatusDto { Curtiu = true, TotalCurtidas = totalCurtidas };
        }

        public async Task<CurtidaStatusDto?> DescurtirAsync(Guid obraDeArteId, Guid userId)
        {
            var obraDeArte = await _obraDeArteRepository.GetByIdAsync(obraDeArteId);
            if (obraDeArte == null) return null;

            var curtidaExistente = await _curtidaRepository.GetByUserAndArtAsync(userId, obraDeArteId);
            if (curtidaExistente != null)
            {
                await _curtidaRepository.DeleteAsync(curtidaExistente);
            }

            var totalCurtidas = await _curtidaRepository.CountByObraDeArteIdAsync(obraDeArteId);
            return new CurtidaStatusDto { Curtiu = false, TotalCurtidas = totalCurtidas };
        }
    }
}
