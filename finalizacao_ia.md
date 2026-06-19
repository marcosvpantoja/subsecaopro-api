# Finalização — IA Jurídica (triagem) + PAT M2M

Documento de acompanhamento das fases pendentes para fechar o fluxo de **IA de triagem** no ecossistema SubseçãoPRO.

**Repositórios envolvidos**

| Repo | Papel |
|------|--------|
| `subse-opro-backend` | API, proxy `/ai/triagem/*`, PAT, criptografia, config por subseção/plataforma |
| `subse-opro-hub` | Sysadmin (`/platform/*`) e backoffice da subseção (`/ia`) |
| `subse-opro/expo` | App mobile — tela de triagem e consumo do PAT |
| Serviço externo (`subsecao-ai` ou similar) | Motor de triagem (LLM + regras); URL em `Subsection.AiServiceUrl` |

---

## Estado atual (já implementado)

### Backend

- **PAT M2M** (`PersonalAccessToken`, prefixo `spro_`)
  - CRUD sysadmin: `GET/POST/DELETE /api/v1/platform/api-tokens`
  - Middleware `PersonalAccessTokenMiddleware` autentica `Bearer spro_...`
  - Hash SHA-256 do token; expiração e revogação
- **Token ativo da IA por subseção**
  - `ActiveAiTokenId`, `ActiveAiTokenEncrypted` em `Subsection`
  - `AiTokenEncryptionService` (AES-256-GCM); chave `Encryption:AiTokenKey`
  - Criação com `SetAsActiveAiToken: true` grava ciphertext na subseção
- **Proxy de triagem** — `AiController` (`/api/v1/ai/triagem/*`)
  - `GET  triagem/inicio`
  - `POST triagem/mensagem`
  - `POST triagem/atendimento`
  - Repassa para `{AiServiceUrl}/triagem/...`; exige `[Authorize]` (JWT ou PAT)
- **Config por subseção** — `IaEnabled`, `IaSystemPrompt`, `IaDisclaimer`, `IaMaxTurns`, `IaAreasCobertasJson`, `AiServiceUrl`
- **Config de plataforma** — `PlatformAppSettings`: `AiMode`, `AiGlobalPrompt`, `AiDisclaimer`
- **Exposição ao app** — `GET /subsections/resolve` retorna `activeAiTokenEncrypted` (sem plaintext)

### Hub (sysadmin)

- `/platform/api-tokens` — gestão de PAT + checkbox “token ativo do app”
- `/platform/subsections/$id` — aba IA com `AiServiceUrl` e campos `Ia*`
- `/platform/app` — `aiMode`, prompt/disclaimer global (UI marcada como “futuro”)

### Mobile (expo)

- `lib/api/ai.api.ts` — cliente dos endpoints de triagem (usa PAT no `Authorization`)
- `lib/crypto/aiToken.ts` — descriptografia do PAT embutido (`EXPO_PUBLIC_AI_TOKEN_KEY`)
- `app/(tabs)/ai.tsx` — **ainda mock**; não chama a API

---

## Arquitetura alvo (resumo)

```
[App mobile]
    │ Bearer spro_... (PAT descriptografado de activeAiTokenEncrypted)
    │ X-Subsection-Slug
    ▼
[subse-opro-backend]  /api/v1/ai/triagem/*
    │ resolve tenant + AiServiceUrl
    ▼
[Serviço subsecao-ai]  /triagem/inicio | mensagem | atendimento
```

O PAT no app é **M2M**: o usuário final não digita o token; o sysadmin gera, marca como ativo, e o ciphertext vai no resolve da subseção.

---

## Fase 1 — Backend: robustez do PAT e proxy

**Objetivo:** fechar lacunas de segurança e operação no que já existe.

| # | Tarefa | Detalhe | Prioridade |
|---|--------|---------|------------|
| 1.1 | Limpar referência ao revogar PAT | Ao `DELETE /platform/api-tokens/{id}`, se `ActiveAiTokenId == id`, zerar `ActiveAiTokenId` e `ActiveAiTokenEncrypted` na subseção | Alta |
| 1.2 | Validar `AiServiceUrl` no save | URL absoluta `http(s)://`; opcional: healthcheck assíncrono ao salvar no sysadmin | Média |
| 1.3 | Política de acesso em `/ai/*` | Decidir e documentar: só `account_type=pat` e `lawyer`, ou PAT exclusivo no mobile; bloquear `backoffice` se necessário (`[Authorize(Roles = "pat,lawyer")]`) | Alta |
| 1.4 | Timeout e erros no proxy | `HttpClient` nomeado com timeout (ex.: 60s); mapear falha do serviço externo para 502/504 com `detail` amigável | Média |
| 1.5 | Headers para o serviço externo | Avaliar repasse de `X-Subsection-Slug`, `X-Request-Id`, locale; contrato com `subsecao-ai` | Média |
| 1.6 | Chave de criptografia em produção | `Encryption:AiTokenKey` forte (64 hex), distinta do dev; documentar rotação (novo PAT + novo ciphertext) | Alta |
| 1.7 | Seed de desenvolvimento | No `MockDataSeeder`: `AiServiceUrl` de exemplo + PAT ativo para subseção demo (facilita teste local) | Baixa |
| 1.8 | Migration `AddSubsectionAiFields` | Migration vazia — consolidar ou remover em favor de `AddPersonalAccessTokens` (sem editar histórico em prod) | Baixa |

**Critério de conclusão:** revogar PAT ativo não deixa app com token morto; proxy estável com erros claros; política de auth documentada.

---

## Fase 2 — Backend: regras de negócio `AiMode` + `IaEnabled`

**Objetivo:** o backend aplicar as flags de plataforma/subseção antes de proxyar.

| # | Tarefa | Detalhe | Prioridade |
|---|--------|---------|------------|
| 2.1 | Gate no `AiController` | Se `!subsection.IaEnabled` → 403; se `PlatformAppSettings.AiMode == "off"` → 403 | Alta |
| 2.2 | Modos `mock` / `global` / `per_subsection` | Definir comportamento: `mock` = respostas fixas no backend sem chamar serviço externo; `global` = ignora prompt da subseção; `per_subsection` = usa `IaSystemPrompt` etc. | Alta |
| 2.3 | Expor flags no `/hub/config` | Já retorna `aiMode`; garantir que mobile/backoffice leiam o mesmo contrato | Média |
| 2.4 | `IaMaxTurns` | Validar no proxy ou delegar ao serviço externo; documentar quem conta turnos | Média |
| 2.5 | Disclaimer obrigatório | Retornar `iaDisclaimer` no `triagem/inicio` ou via `/subsection/info` para o app exibir antes do chat | Média |

**Critério de conclusão:** com `IaEnabled=false` ou `AiMode=off`, `/ai/triagem/*` não chama serviço externo; modo `mock` funciona sem `AiServiceUrl`.

---

## Fase 3 — Hub: configuração unificada (sysadmin)

**Objetivo:** operação clara para quem configura a plataforma.

| # | Tarefa | Repo | Detalhe | Prioridade |
|---|--------|------|---------|------------|
| 3.1 | Remover rótulo “futuro” em `/platform/app` | hub | IA deixa de ser experimental na UI quando Fase 2 estiver pronta | Baixa |
| 3.2 | Checklist na subseção | hub | Em `/platform/subsections/$id`: indicar ✅ PAT ativo, ✅ `AiServiceUrl`, ✅ `IaEnabled` | Alta |
| 3.3 | Link cruzado PAT ↔ subseção | hub | De `/platform/api-tokens`, atalho para editar IA da subseção; aviso se falta `AiServiceUrl` | Média |
| 3.4 | Módulo “IA” no app público | hub + backend | Incluir `ai` em `PublicModules` (como banners/lawyers) para habilitar tab no app multi-subseção | Média |
| 3.5 | Backoffice `/ia` vs sysadmin | hub | Documentar: admin da subseção edita prompt/disclaimer; só sysadmin define URL + PAT | Baixa |
| 3.6 | Logs de IA no backoffice | hub | Substituir `MOCK_AI_LOGS` por endpoint real (se houver persistência no serviço externo) | Baixa |

**Critério de conclusão:** sysadmin configura subseção ponta a ponta sem adivinhar ordem (PAT → URL → modo plataforma).

---

## Fase 4 — Mobile (expo): integração real

**Objetivo:** trocar mock por triagem via backend.

| # | Tarefa | Repo | Detalhe | Prioridade |
|---|--------|------|---------|------------|
| 4.1 | Obter PAT | expo | `useTenant` → `activeAiTokenEncrypted` → `decryptAiToken` | Alta |
| 4.2 | `.env` | expo | `EXPO_PUBLIC_AI_TOKEN_KEY` igual a `Encryption:AiTokenKey` do backend | Alta |
| 4.3 | Refatorar `ai.tsx` | expo | Fluxo: `getTriagemInicio` → loop `postTriagemMensagem` → `postTriagemAtendimento`; estados loading/erro | Alta |
| 4.4 | Respeitar flags | expo | Esconder tab IA se `aiMode=off` ou `!iaEnabled`; fallback mock só se `aiMode=mock` | Alta |
| 4.5 | Disclaimer na UI | expo | Exibir `iaDisclaimer` antes ou no topo do chat | Média |
| 4.6 | Cards de advogados | expo | Usar resposta de `postTriagemAtendimento` (`advogados`, `tipo`, `defensoria_url`) | Média |
| 4.7 | Hub multi-subseção | expo | Definir de qual subseção vem o PAT quando usuário está no modo público hub | Média |

**Critério de conclusão:** app em dev local conversa com API Docker + serviço de IA; mock removido ou só quando `aiMode=mock`.

---

## Fase 5 — Serviço externo de triagem (`subsecao-ai`)

**Objetivo:** contrato estável entre backend proxy e motor de IA.

| # | Tarefa | Detalhe | Prioridade |
|---|--------|---------|------------|
| 5.1 | Contrato OpenAPI | Documentar request/response de `triagem/inicio`, `mensagem`, `atendimento` (alinhado a `ai.api.ts` no expo) | Alta |
| 5.2 | Deploy por ambiente | URL por subseção ou instância compartilhada; Docker compose local na porta esperada pelo seed | Alta |
| 5.3 | Autenticação serviço ↔ backend | Hoje o proxy não adiciona secret ao chamar o externo; avaliar mTLS ou API key interna | Média |
| 5.4 | Prompt e áreas | Serviço deve receber ou buscar `IaSystemPrompt` / `IaAreasCobertas` (header ou endpoint de config) | Média |
| 5.5 | Observabilidade | Logs estruturados, métricas de latência e custo por subseção | Baixa |

**Critério de conclusão:** backend proxy retorna 200 com payloads reais; contrato versionado.

---

## Fase 6 — Testes, segurança e produção

| # | Tarefa | Detalhe | Prioridade |
|---|--------|---------|------------|
| 6.1 | Testes de integração | PAT válido/inválido/revogado; proxy com `AiServiceUrl` mock (TestServer); revogação limpa ciphertext | Alta |
| 6.2 | Teste E2E manual | Checklist: criar PAT → configurar URL → resolve retorna ciphertext → app descriptografa → triagem completa | Alta |
| 6.3 | Risco do PAT embutido | PAT no app é extraível do binário; mitigar: escopo mínimo (`pat` só em `/ai/*`), rotação, expiração, rate limit por slug | Alta |
| 6.4 | Rate limiting | Por `subsection_id` + IP no `/ai/*` | Média |
| 6.5 | Auditoria | `LastUsedAt` no PAT já existe; expor no hub; alerta se token ativo expira em N dias | Baixa |
| 6.6 | CORS / mobile | Portas Expo (`8081`) já em `appsettings`; validar produção | Baixa |

---

## Ordem sugerida de execução

```
Fase 1 (backend hardening)
    → Fase 5 (contrato + serviço IA local)
        → Fase 2 (AiMode / IaEnabled no backend)
            → Fase 4 (mobile)
                → Fase 3 (polish hub)
                    → Fase 6 (testes + prod)
```

---

## Checklist rápido de validação local

1. Backend: `docker compose up` — API em `http://localhost:5001`
2. Hub sysadmin: criar PAT com “token ativo do app” para subseção `cidade-exemplo`
3. Hub: preencher `AiServiceUrl` da subseção (serviço de triagem rodando)
4. `Encryption:AiTokenKey` no backend = `EXPO_PUBLIC_AI_TOKEN_KEY` no expo `.env`
5. `GET /api/v1/subsections/resolve?slug=cidade-exemplo` → `activeAiTokenEncrypted` preenchido
6. `GET /api/v1/ai/triagem/inicio` com `Authorization: Bearer spro_...` e header `X-Subsection-Slug: cidade-exemplo` → 200
7. Expo: tab IA com conversa real (após Fase 4)

---

## Referências no código (backend)

| Componente | Caminho |
|----------|---------|
| Proxy IA | `src/SubseOpro.Api/Controllers/AiController.cs` |
| PAT middleware | `src/SubseOpro.Api/Middleware/PersonalAccessTokenMiddleware.cs` |
| CRUD PAT (sysadmin) | `src/SubseOpro.Api/Controllers/PlatformPersonalAccessTokensController.cs` |
| Criptografia | `src/SubseOpro.Api/Services/AiTokenEncryptionService.cs` |
| Resolve (ciphertext) | `src/SubseOpro.Application/Mappers/SubsectionMapper.cs` → `ToResolveViewModel` |
| Config plataforma | `src/SubseOpro.Domain/Entities/PlatformAppSettings.cs` |
| Config subseção | `src/SubseOpro.Domain/Entities/Subsection.cs` |
| Chave dev | `src/SubseOpro.Api/appsettings.json` → `Encryption:AiTokenKey` |

---

*Última atualização: junho/2026 — refletir o estado dos repositórios `subse-opro-backend`, `subse-opro-hub` e `subse-opro/expo`.*
