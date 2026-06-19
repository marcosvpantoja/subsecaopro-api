# Sanitization Guardrails (Backend)

- Build obrigatório: `dotnet build SubseOpro.sln`.
- Analyzers obrigatórios (sem auto-fix em PR): `dotnet format analyzers SubseOpro.sln --verify-no-changes --severity info`.
- Não remover/editar migrations históricas em `src/SubseOpro.Infrastructure/Persistence/Migrations`.
- Qualquer remoção de contrato deve ser propagada em cadeia (`Interface -> Implementação -> Chamadores`) no mesmo PR.
