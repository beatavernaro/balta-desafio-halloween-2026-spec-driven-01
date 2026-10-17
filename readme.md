<img width="100%" alt="Halloween 2026" src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/banner.jpg" />

## 🎃 Halloween - Desafio 1

Oi, eu sou a Beatriz e este é o espaço onde compartilho minha jornada de aprendizado durante o desafio **Halloween 2026**, realizado pelo [balta.io](https://balta.io). 👻

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

## Validação final

### Procedimento reproduzível

Executado a partir da raiz do repositório, na mesma ordem da integração contínua:

```powershell
dotnet restore PasswordGenerator.slnx
dotnet tool restore
dotnet format PasswordGenerator.slnx --verify-no-changes --no-restore
dotnet build PasswordGenerator.slnx --no-restore -warnaserror
powershell -ExecutionPolicy Bypass -File scripts/test-coverage.ps1 -NoBuild
powershell -ExecutionPolicy Bypass -File scripts/verify-gate-failures.ps1
dotnet list PasswordGenerator.slnx package --vulnerable --include-transitive
```

### Resultados (2026-10-16, commit `9030b13`, .NET SDK 10.0.301, Windows)

| Portão | Verificação | Resultado |
|---|---|---|
| 1. Compilação | Restauração e build com warnings como erros | ✅ 0 avisos, 0 erros |
| 2. Testes | Testes unitários | ✅ 23 de 23 aprovados |
| 2. Testes | Testes de integração | ✅ 21 de 21 aprovados |
| 2. Testes | Cobertura agregada de linhas (mínimo 80%) | ✅ 95,74% (135 de 141 linhas); ramos 87,5% |
| 3. Análise estática | `dotnet format --verify-no-changes` e analisadores `latest-recommended` com estilo aplicado no build | ✅ Sem violações |
| 3. Análise estática | Pacotes com vulnerabilidades conhecidas, inclusive transitivos | ✅ Nenhum |
| 4. Conformidade funcional | RF-01 a RF-17, CA-01 a CA-11 e CB-01 a CB-13 com evidência | ✅ Ver seções 9, 10 e 11.1 de [`tasks.md`](specs/tasks.md) |
| 5. Integração contínua | Workflow [`ci.yml`](.github/workflows/ci.yml) | ✅ [Execução no GitHub Actions](https://github.com/beatavernaro/balta-desafio-halloween-2026-spec-driven-01/actions/runs/38963703869) (commit `2f1d3d1`, Ubuntu): os jobs *Build, test and coverage* e *Verify gates reject violations* aprovados |

**Portões reprovam violações** (`scripts/verify-gate-failures.ps1`, em cópia descartável do repositório):

| Violação injetada | Resultado esperado | Observado |
|---|---|---|
| Variável não utilizada (CS0219) | Build falha | ✅ exit 1 |
| Espaçamento fora do padrão | `dotnet format` falha | ✅ exit 2 |
| Teste com falha | `dotnet test` falha | ✅ exit 1 |
| Cobertura de 79,99% | Verificação falha | ✅ exit 1 |
| Cobertura de 80% (controle) | Verificação passa | ✅ exit 0 |

As 6 linhas não cobertas são o método `Down` da migration inicial (reversão, não executada pelos testes) e a salvaguarda do gerador que lança exceção caso uma senha gerada não passe na validação, inalcançável enquanto o algoritmo estiver correto.

### Conformidade com a Constituição

| Princípio | Evidência | Resultado |
|---|---|---|
| I. Tecnologias e dependências | Stack do Plan; versões fixas nos `.csproj` e em `dotnet-tools.json`; ReportGenerator documentado no Plan | ✅ Conforme |
| II. Simplicidade e separação | Endpoints → `PasswordService` → `PasswordGenerator`/`PasswordDbContext`; sem Repository, CQRS ou interfaces ociosas | ✅ Conforme |
| III. Escopo | Somente as duas operações (`Application_ExposesOnlyTwoFunctionalOperations`) | ✅ Conforme |
| IV. Testes automatizados | Regras de negócio com testes unitários; endpoints com integração; CB-01 a CB-13 cobertos; cobertura ≥80% | ✅ Conforme |
| V. Qualidade estática e CI | `.editorconfig`, warnings como erros, analisadores no build e workflow de CI | ✅ Conforme |
| VI. Segurança e dados | `RandomNumberGenerator`; validação do identificador; ProblemDetails sem detalhes internos; senhas fora dos logs (testado); restrições no banco | ✅ Conforme |
| VII. Código e nomenclatura | Identificadores em inglês; regras de nomenclatura aplicadas no build | ✅ Conforme |
| VIII. Documentação e versionamento | Documentos em português; um commit por tarefa com Conventional Commits | ✅ Conforme |
| IX–XII. Processo, rastreabilidade e execução | Tarefas executadas em ordem, com testes antes da implementação e rastreabilidade RF/CA/CB em `tasks.md` | ✅ Conforme |

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
