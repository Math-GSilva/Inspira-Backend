namespace inspira_backend.Application.DTOs
{
    public class UsuarioProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string NomeCompleto { get; set; }
        public string? Bio { get; set; }
        public string? UrlFotoPerfil { get; set; }
        public int ContagemSeguidores { get; set; }
        public int ContagemSeguindo { get; set; }
        public bool SeguidoPeloUsuarioAtual { get; set; }
        public string? UrlPortifolio { get; set; }
        public string? UrlLinkedin { get; set; }
        public string? UrlInstagram { get; set; }
        public string? CategoriaPrincipalId { get; set; }
        public string? CategoriaPrincipalNome { get; set; }
    }
}
