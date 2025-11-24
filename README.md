# 🎨 Inspira Backend

<div align="center">

![Status do Projeto](https://img.shields.io/badge/Status-Em%20Desenvolvimento-green?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Available-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![CI/CD](https://img.shields.io/github/actions/workflow/status/math-gsilva/inspira-backend/build-and-analyze.yml?label=Build%20%26%20Test&style=for-the-badge)

</div>

<p align="center">
  <b>Plataforma de rede social para artistas com recomendação de conteúdo baseada em Inteligência Artificial.</b>
</p>

---

## 📖 Sobre o Projeto

O **Inspira Backend** é uma API REST robusta desenvolvida para suportar uma rede social focada em artistas. O sistema permite o compartilhamento de obras (imagens, vídeos e áudios), interação social (curtidas, comentários, seguidores) e possui um diferencial técnico importante: um motor de recomendação personalizado.

A solução implementa **Clean Architecture** para garantir desacoplamento e testabilidade, e utiliza **ML.NET** rodando em uma **Azure Function** para processar recomendações baseadas em filtragem colaborativa (Matrix Factorization).

---

## 🚀 Funcionalidades Principais

### 📱 Core API (`inspira-backend.Api`)
* **Autenticação Segura**: Login e Registro com **JWT (Bearer Token)** e criptografia BCrypt.
* **Controle de Acesso (RBAC)**: Perfis de `Artista`, `Comum` e `Administrador`.
* **Gestão de Conteúdo**: Upload de imagens e vídeos integrado com **Cloudinary**.
* **Social**: Sistema completo de seguidores, curtidas e comentários.
* **Feed Inteligente**: Endpoint que ordena obras baseado no *score* de afinidade do usuário gerado pela IA.

### 🧠 Motor de IA (`Inspira.Trainer`)
* **Arquitetura Serverless**: Implementado como uma **Azure Function** (Timer Trigger) que roda periodicamente.
* **Machine Learning**: Utiliza o algoritmo de **Matrix Factorization** do ML.NET.
* **Personalização**: Analisa o histórico de interações (curtidas) para prever quais categorias o usuário tem maior probabilidade de gostar, atualizando os pesos no banco de dados.

---

## 🛠️ Tecnologias Utilizadas

Este projeto utiliza as tecnologias mais modernas do ecossistema .NET:

* **Linguagem & Framework**: .NET 8.0, C#.
* **Banco de Dados**: PostgreSQL 15 (Utilizando EF Core).
* **Arquitetura**: Clean Architecture (Domain, Application, Infra, API).
* **IA/ML**: Microsoft.ML (ML.NET).
* **Cloud & Deploy**: Azure Functions, Azure (Também tem suporte para Docker).
* **Armazenamento**: Cloudinary (Media Management).
* **Qualidade & Testes**:
    * **xUnit, FluentAssertions, Moq**.
    * **Testcontainers**: Testes de integração com banco de dados real em contêineres.
    * **SonarCloud**: Análise estática de código e cobertura.
    * **GitHub Actions**: Pipelines de CI/CD configurados.

---

## 📂 Estrutura da Solução

```bash
inspira-backend/
├── inspira-backend.Api/          # Entry point, Controllers, Configurações
├── inspira-backend.Application/  # Casos de uso, Services, DTOs, Interfaces
├── inspira-backend.Domain/       # Entidades, Enums, Interfaces de Repositório
├── inspira-backend.Infra/        # Implementação EF Core, Repositórios, Cloudinary
├── Inspira.Trainer/              # Azure Function para treinamento da IA
├── Inspira.Test/                 # Testes Unitários
└── Inspira.IntegrationTests/     # Testes de Integração (com Testcontainers)
```

---

## ⚙️ Como Executar

### Pré-requisitos
* [Docker](https://www.docker.com/) e Docker Compose instalados.
* (Opcional) .NET SDK 8.0 para rodar fora do Docker.

### 🐳 Rodando com Docker (Recomendado)

1.  **Clone o repositório**
    ```bash
    git clone [https://github.com/math-gsilva/inspira-backend.git](https://github.com/math-gsilva/inspira-backend.git)
    cd inspira-backend
    ```

2.  **Configure as Variáveis de Ambiente**
    Crie um arquivo `.env` na raiz ou edite o `docker-compose.yml` (não recomendado para produção) com suas credenciais:
    * Credenciais do PostgreSQL.
    * Credenciais do Cloudinary.
    * `JwtSettings:Secret` (Deve ser uma string forte).

3.  **Suba os containers**
    ```bash
    docker-compose up --build
    ```

4.  **Acesse a API**
    * A API estará disponível em: `http://localhost:8000`
    * Documentação Swagger: `http://localhost:8000/swagger`

### 🧪 Rodando os Testes

Para executar a suíte de testes (unitários e de integração):

```bash
dotnet test
```
*Nota: Os testes de integração utilizam Testcontainers, então é necessário ter o Docker rodando na máquina.*

---

## 🔧 Configuração (`appsettings.json`)

Para rodar localmente sem Docker, configure o `appsettings.Development.json` na API e no Trainer:

```json
{
  "ConnectionStrings": {
    "InspiraDbConnection": "Host=localhost;Port=5432;Database=InspiraDB;Username=seu_user;Password=sua_senha"
  },
  "JwtSettings": {
    "Secret": "SUA_CHAVE_PRIVADA_MUITO_SECRETA_E_LONGA",
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

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Se você tiver sugestões de melhoria ou novas features:

1.  Faça um **Fork** do projeto.
2.  Crie uma Branch: `git checkout -b feature/MinhaFeature`.
3.  Faça o Commit: `git commit -m 'Adiciona MinhaFeature'`.
4.  Faça o Push: `git push origin feature/MinhaFeature`.
5.  Abra um **Pull Request**.

---

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

<div align="center">
  <sub>Desenvolvido por <a href="https://github.com/math-gsilva">Math-GSilva</a>.</sub>
</div>
