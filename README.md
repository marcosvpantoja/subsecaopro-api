# SubseçãoPRO — Backend API

API REST do **SubseçãoPRO**, plataforma digital da subseção da OAB (inicialmente Cidade Exemplo/SP). Este repositório concentra o backend que atende o frontend web ([subse-opro](https://github.com)) e o app mobile Expo.

## O que é

O SubseçãoPRO oferece feed institucional, busca de advogados, reserva de salas, clube de benefícios, eleições, notificações e área autenticada para advogados inscritos (login OAB + senha). Este projeto é a camada de persistência e regras de negócio: **ASP.NET Core 10** com **Clean Architecture**, **PostgreSQL**, **EF Core** e autenticação **JWT**.

### Camadas

| Projeto | Responsabilidade |
|---|---|
| `SubseOpro.Api` | Controllers HTTP, JWT, Swagger, middleware |
| `SubseOpro.Application` | ViewModels, mappers, contratos de serviço |
| `SubseOpro.Domain` | Entidades, enums, interfaces de repositório |
| `SubseOpro.Infrastructure` | EF Core, repositórios, migrations, seed |

### Endpoints

Base URL em desenvolvimento: `http://localhost:5000/api/v1`

Principais grupos: `auth`, `lawyers`, `rooms`, `reservations`, `posts`, `banners`, `partners`, `notifications`, `articles`, `elections`, `subsection`, `adminusers` e demais controllers em `src/SubseOpro.Api/Controllers/`.

Documentação interativa (Development): `http://localhost:5000/swagger`

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (recomendado para PostgreSQL)
- Opcional: Visual Studio / Rider / VS Code

## Configuração rápida

### 1. Variáveis de ambiente

```bash
cp .env.example .env
```

Ajuste `ConnectionStrings__DefaultConnection` se necessário. Com o Postgres do Docker Compose deste repo, use a porta **5433** no host (`localhost:5433`).

### 2. Banco de dados

```bash
docker compose -f docker-compose.postgres.yml up -d
```

### 3. Rodar a API

```bash
cd src/SubseOpro.Api
dotnet run
```

Na primeira execução em Development, as **migrations** são aplicadas automaticamente e o **seed** de dados demo roda se `Seed__RunOnStartup=true` (padrão em `appsettings.json`).

Credencial demo (seed): OAB `378901` / senha `demo123`.

### 4. API + Postgres via Docker

```bash
docker compose up -d --build
```

A API fica em `http://localhost:5001` (mapeamento `5001:8080`).

## Migrations (EF Core)

```bash
docker compose -f docker-compose.postgres.yml up -d

cd src/SubseOpro.Infrastructure
dotnet ef migrations add NomeDaMigration \
  --startup-project ../SubseOpro.Api \
  --output-dir Persistence/Migrations

dotnet ef database update --startup-project ../SubseOpro.Api
```

> Não edite migrations já aplicadas em produção; crie sempre uma nova migration.

## Produção

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

Configure `.env.prod` com segredos fortes (`Jwt__Secret`, senha do Postgres, `Seed__RunOnStartup=false`).

## Estrutura do repositório

```
├── src/
│   ├── SubseOpro.Api/
│   ├── SubseOpro.Application/
│   ├── SubseOpro.Domain/
│   └── SubseOpro.Infrastructure/
├── docker/api/Dockerfile
├── docker-compose.yml
├── docker-compose.postgres.yml
├── docker-compose.prod.yml
├── SubseOpro.sln
└── SubseOpro.slnx
```

## Repositórios relacionados

- **Frontend web + app Expo:** repositório `subse-opro` — consome esta API via `VITE_API_URL` / `EXPO_PUBLIC_API_URL` apontando para `/api/v1`.

## Licença

Uso interno do projeto SubseçãoPRO.

---

## Autoria

Backend desenvolvido por **Luan Lima**, com a 29Tech. Os dados de seed são fictícios ("OAB Cidade Exemplo").

Publicado por **Marcos Pantoja** · [29Tech](https://www.vintenove.com)
