using inspira_backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Domain.Interfaces
{
    public interface ISeguidorRepository
    {
        /// <summary>
        /// Obtém uma relação de seguidor específica com base em quem segue e quem é seguido.
        /// </summary>
        /// <param name="seguidorId">O ID do usuário que segue.</param>
        /// <param name="seguidoId">O ID do usuário que está a ser seguido.</param>
        /// <returns>A entidade Seguidor se a relação existir, caso contrário null.</returns>
        Task<Seguidor?> GetByFollowerAndFollowedAsync(Guid seguidorId, Guid seguidoId);

        /// <summary>
        /// Adiciona uma nova relação de seguidor à base de dados.
        /// </summary>
        /// <param name="seguidor">A entidade Seguidor a ser adicionada.</param>
        Task AddAsync(Seguidor seguidor);

        /// <summary>
        /// Remove uma relação de seguidor da base de dados.
        /// </summary>
        /// <param name="seguidor">A entidade Seguidor a ser removida.</param>
        Task DeleteAsync(Seguidor seguidor);
    }
}
