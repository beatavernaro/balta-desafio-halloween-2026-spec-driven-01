<img width="100%" alt="Halloween 2026" src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/banner.jpg" />

## 🎃 Halloween - Desafio 1

Oi, eu sou o [seu nome aqui] e este é o espaço onde compartilho minha jornada de aprendizado durante o desafio **Halloween 2026**, realizado pelo [balta.io](https://balta.io). 👻

Aqui você vai encontrar projetos, exercícios e códigos que estou desenvolvendo durante o desafio.

### Sobre este desafio
Neste desafio o objetivo é consolidar os fundamentos do Spec Driven Development, criando uma constituição, especificação, planejamento e tarefas manualmente para serem implementadas pela IA posteriormente.

#### O que foi implementado
- API HTTP em .NET 10 (Minimal APIs) com exatamente duas operações: gerar senha (`POST /passwords`) e consultar senha por GUID (`GET /passwords/{id}`).
- Geração criptograficamente segura de senhas de 16 caracteres com maiúsculas, minúsculas, números e símbolos `!@#$%&*` (`RandomNumberGenerator.GetInt32` + Fisher–Yates).
- Persistência em SQLite com Entity Framework Core e migrations.
- Erros padronizados com ProblemDetails (400, 404 e 500), sem expor detalhes internos nem registrar senhas em logs.
- Testes unitários e de integração (xUnit + `WebApplicationFactory`), cobertura com Coverlet e verificação mínima de 80%.
- Integração contínua no GitHub Actions com formatação, análise estática, build sem warnings, testes e cobertura.

Os documentos de SDD estão em [`specs/`](specs/): [constituição](specs/constitution.md), [especificação](specs/spec.md), [plano](specs/plan.md) e [tarefas](specs/tasks.md).

Neste processo eu aprendi:
* ✅

## Bagde
<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/01.png" width="200" />

## Problema

Disponibilizar uma maneira simples de gerar senhas fortes e recuperá-las posteriormente por meio de um identificador único (GUID). Cada senha gerada tem exatamente 16 caracteres, contém pelo menos uma letra maiúscula, uma minúscula, um número e um símbolo permitido, é armazenada e pode ser consultada depois pelo GUID devolvido na geração.

## Como executar

### Pré-requisitos

- [.NET SDK 10.0.301](https://dotnet.microsoft.com/download/dotnet/10.0) (ou outro SDK 10.0.x compatível).
- PowerShell para os scripts de qualidade: Windows PowerShell 5.1 ou PowerShell 7 (`pwsh`).

As ferramentas `dotnet-ef` e `reportgenerator` são locais ao repositório (versões fixadas em [`dotnet-tools.json`](dotnet-tools.json)) e não precisam ser instaladas globalmente.

### Restaurar dependências e ferramentas

```powershell
dotnet restore PasswordGenerator.slnx
dotnet tool restore
```

### Criar o banco de dados (migrations)

```powershell
dotnet tool run dotnet-ef database update --project src/PasswordGenerator.Api
```

O banco é criado em `src/PasswordGenerator.Api/passwords.db`. O arquivo não é versionado e pode ser recriado a qualquer momento pelas migrations.

### Executar a API

```powershell
dotnet run --project src/PasswordGenerator.Api
```

A API fica disponível em `http://localhost:5110`. Em ambiente de desenvolvimento, o documento OpenAPI fica em `http://localhost:5110/openapi/v1.json`.

### Usar a API

No Windows PowerShell, use `curl.exe` no lugar de `curl` (`curl` é um alias de `Invoke-WebRequest`).

#### Gerar senha

```bash
curl -X POST http://localhost:5110/passwords
```

```http
HTTP 201 Created
Location: /passwords/3d34a5f3-a993-4245-b994-3e6da1c7d8da

{ "id": "3d34a5f3-a993-4245-b994-3e6da1c7d8da", "password": "GdV5n99T9SMP1T@!" }
```

#### Consultar senha

```bash
curl http://localhost:5110/passwords/3d34a5f3-a993-4245-b994-3e6da1c7d8da
```

```http
HTTP 200 OK

{ "id": "3d34a5f3-a993-4245-b994-3e6da1c7d8da", "password": "GdV5n99T9SMP1T@!" }
```

#### Erros

Formato ProblemDetails (`application/problem+json`):

| Situação | Status | `title` |
|---|---|---|
| Identificador com formato inválido | 400 | Invalid identifier |
| GUID válido sem registro | 404 | Password not found |
| Falha inesperada, inclusive no armazenamento | 500 | Internal server error |

### Testes e verificações de qualidade

```powershell
# Todos os testes (unitários e de integração)
dotnet test PasswordGenerator.slnx

# Formatação e estilo
dotnet format PasswordGenerator.slnx --verify-no-changes

# Testes com cobertura consolidada e verificação do mínimo de 80% de linhas
powershell -ExecutionPolicy Bypass -File scripts/test-coverage.ps1

# Demonstra que cada portão de qualidade reprova uma violação (executa em cópia descartável do repositório)
powershell -ExecutionPolicy Bypass -File scripts/verify-gate-failures.ps1
```

Com PowerShell 7, os scripts também podem ser executados diretamente, por exemplo `pwsh ./scripts/test-coverage.ps1`. O relatório de cobertura é gerado em `TestResults/coverage-report/`.

Os testes de integração usam um banco SQLite em memória isolado por teste e não dependem do `passwords.db` local.

### Estrutura

```text
src/PasswordGenerator.Api/      API (Endpoints, Contracts, Services, Models, Data e Migrations)
tests/PasswordGenerator.UnitTests/         Testes do gerador de senhas e dos casos de borda
tests/PasswordGenerator.IntegrationTests/  Testes HTTP, persistência, falhas, logs e contratos
scripts/                        Cobertura e verificação dos portões de qualidade
specs/                          Documentos de Spec-Driven Development
.github/workflows/ci.yml        Integração contínua
```

## Limitações de segurança

Este projeto é **educacional** e não deve ser usado em produção nem com credenciais reais:

- **Senhas armazenadas de forma recuperável:** o valor fica em texto no SQLite para permitir a consulta. Quem tiver acesso ao arquivo do banco vê todas as senhas.
- **Sem autenticação ou autorização:** qualquer pessoa que conheça um GUID pode recuperar a senha correspondente.
- **Persistência local:** a perda ou exclusão de `passwords.db` apaga os registros.
- **Sem limitação de requisições:** não há proteção contra uso abusivo da API.

A geração das senhas usa `RandomNumberGenerator` (criptograficamente seguro), as respostas de erro não expõem detalhes internos e os valores das senhas não são registrados em logs.

## Sobre o Halloween 2026
O desafio **Halloween 2026** consiste em implementar implementar o modelo Spec Driven Development de ponta a ponta, criando apps completas com IA.

### Veja meu progresso no desafio
[Incluir link para o repositório central]
