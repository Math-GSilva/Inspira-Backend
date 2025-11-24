# 🎨 Inspira Backend

> Plataforma de rede social para artistas com recomendação de conteúdo baseada em Inteligência Artificial.

O **Inspira Backend** é uma API REST robusta construída em .NET 8 que auxilia plataforma onde artistas podem compartilhar suas obras (imagensk, vídeos e áudios), interagir com a comunidade e receber recomendações personalizadas através de um motor de Machine Learning (ML.NET).

## 🚀 Funcionalidades

### 📱 API Principal
- **Autenticação & Segurança**: Registro e Login com JWT (JSON Web Tokens) e controle de acesso baseado em Roles (Artista, Comum, Administrador).
- **Gestão de Obras de Arte**: CRUD completo de obras, upload de mídia (imagens/vídeos) integrado com Cloudinary.
- **Interação Social**:
  - Curtir e descurtir obras.
  - Seguir e deixar de seguir outros usuários.
- **Feed Inteligente**: Endpoint de listagem de obras que ordena o conteúdo baseado na afinidade do usuário (calculada pela IA).
- **Perfis de Usuário**: Personalização de perfil com bio, foto e links para redes sociais (Instagram, LinkedIn, Portfólio).

### 🧠 Inspira.Trainer (IA)
- **Serviço de Recomendação**: Um *Background Worker* (Azure Function) que processa periodicamente os dados de interação.
- **Machine Learning**: Utiliza o algoritmo de **Fatoração de Matrizes (Matrix Factorization)** do ML.NET.
- **Predição de Preferências**: Analisa o histórico de curtidas para calcular um `Score` de afinidade entre usuários e categorias de arte, personalizando o feed de cada usuário.

## 🛠️ Tecnologias Utilizadas

- **Core**: C# .NET 8.0
- **Arquitetura**: MVC
- **Banco de Dados**: PostgreSQL 15
- **ORM**: Entity Framework Core
- **Machine Learning**: ML.NET (Microsoft.ML)
- **Armazenamento de Mídia**: Cloudinary
- **Containerização**: Docker & Docker Compose
- **Testes**: xUnit, FluentAssertions, Moq
- **CI/CD**: GitHub Actions

## 📂 Estrutura do Projeto

A solução segue os princípios da Clean Architecture:

- **inspira-backend.Api**: Camada de entrada (Controllers, Configurações).
- **inspira-backend.Application**: Regras de negócio, Serviços, DTOs e Interfaces.
- **inspira-backend.Domain**: Entidades, Enums e Interfaces de Repositório.
- **inspira-backend.Infra**: Implementação de acesso a dados (EF Core), Repositórios e Integrações externas.
- **Inspira.Trainer**: Projeto isolado (Azure Function) responsável pelo treinamento do modelo de IA.
- **Inspira.Test/Inspira.IntegrationTests**: Testes unitários e de integração.

## ⚙️ Pré-requisitos

Antes de começar, certifique-se de ter instalado:
- [Docker](https://www.docker.com/) e Docker Compose
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) (opcional, para rodar fora do Docker)
- Conta no [Cloudinary](https://cloudinary.com/) (para as credenciais de API)

## 🚀 Como Executar

### Opção 1: Usando Docker (Recomendado)

O projeto já está configurado com `docker-compose` para subir a API e o Banco de Dados.

1. Clone o repositório:
   ```bash
   git clone https://github.com/seu-usuario/inspira-backend.git
   cd inspira-backend
   ```

2. Configure as variáveis de ambiente:
   Edite o arquivo `docker-compose.yml` ou crie um arquivo `.env` com suas credenciais reais (especialmente as do Cloudinary e JWT Secret).

3. Suba os containers:
   ```bash
   docker-compose up --build
   ```

A API estará disponível em: `http://localhost:8000` (Swagger em `/swagger`).

### Opção 2: Execução Manual

1. Configure o `appsettings.json` na pasta `inspira-backend.Api` com sua string de conexão PostgreSQL e credenciais do Cloudinary.

2. Aplique as migrações do banco de dados:
   ```bash
   dotnet ef database update --project inspira-backend.Infra --startup-project inspira-backend.Api
   ```

3. Execute a API:
   ```bash
   dotnet run --project inspira-backend.Api
   ```

## 🔧 Configuração de Variáveis

Certifique-se de configurar as seguintes chaves no seu `appsettings.json` ou variáveis de ambiente:

```json
{
  "ConnectionStrings": {
    "InspiraDbConnection": "Host=localhost;Port=5432;Database=InspiraDB;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Secret": "SUA_CHAVE_SUPER_SECRETA_MUITO_LONGA_PARA_SEGURANCA",
    "Issuer": "InspiraBackend",
    "Audience": "InspiraFrontend",
    "ExpiryMinutes": 120
  },
  "CloudinarySettings": {
    "CloudName": "seu_cloud_name",
    "ApiKey": "sua_api_key",
    "ApiSecret": "seu_api_secret"
  }
}
```

## 🧠 O Serviço de IA (Inspira.Trainer)

O **Inspira.Trainer** é executado separadamente. Ele é configurado como uma **Azure Function** com gatilho de timer (`0 0 0 * * *`), rodando uma vez por dia para retreinar o modelo com os dados mais recentes.

Para rodar localmente:
1. Navegue até a pasta `Inspira.Trainer`.
2. Configure o `local.settings.json` com a connection string do banco.
3. Execute com `func start` (requer Azure Functions Core Tools) ou via Visual Studio.

## 🧪 Testes

O projeto possui testes unitários e de integração cobrindo serviços, controladores e repositórios.

Para rodar os testes:
```bash
dotnet test
```

## 🤝 Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou enviar pull requests.

1. Faça um Fork do projeto
2. Crie sua Feature Branch (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a Branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

## 🙏 Agradecimentos

Este projeto foi desenvolvido como parte do portfólio do curso de **Engenharia de Software** da **Católica SC em Joinville**.

Agradeço aos professores, colegas e à instituição pelo suporte e conhecimento compartilhados durante o desenvolvimento deste sistema.

## 📞 Contato

**Matheus Gabriel da Silva**

Entre em contato para tirar dúvidas sobre o projeto ou para oportunidades de networking:

- 💼 [LinkedIn](https://www.linkedin.com/in/matheus-gabriel-da-silva-55bb88215/)
- 🐙 [GitHub](https://github.com/Math-GSilva)

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.
