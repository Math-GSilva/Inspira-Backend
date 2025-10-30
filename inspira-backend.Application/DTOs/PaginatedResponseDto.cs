using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class PaginatedResponseDto<T>
    {
        /// <summary>
        /// Os itens da página atual.
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// O cursor para buscar a próxima página (será a DataPublicacao do último item).
        /// Formatado como string ISO 8601.
        /// </summary>
        public string? NextCursor { get; set; }

        /// <summary>
        /// Indica se existem mais itens para serem buscados.
        /// </summary>
        public bool HasMoreItems { get; set; }
    }
}
