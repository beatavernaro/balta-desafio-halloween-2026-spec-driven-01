# Plano de Implementação — Gerador de Senhas Fortes

**Versão:** 1.1.0  
**Data:** 2026-10-16  
**Status:** Aprovado na revisão documental  
**Referências:** `constitution.md` e `spec.md`

## 1. Resumo

Implementar uma API HTTP para geração e consulta de senhas fortes, atendendo aos requisitos RF-01 a RF-17 da Specification.

A aplicação deverá gerar senhas de exatamente 16 caracteres, contendo letras maiúsculas, minúsculas, números e símbolos do conjunto `!@#$%&*`.

Cada senha será armazenada com um identificador GUID único, permitindo sua recuperação posterior.

A implementação seguirá uma arquitetura minimalista, priorizando simplicidade, separação de responsabilidades, geração criptograficamente segura e testes automatizados.

## 2. Stack tecnológica

| Categoria | Tecnologia |
|---|---|
| Linguagem | C# |
| Plataforma | .NET 10 |
| API HTTP | ASP.NET Core Minimal APIs |
| Persistência | SQLite |
| Acesso a dados | Entity Framework Core |
| Testes unitários | xUnit |
| Testes de integração | Microsoft.AspNetCore.Mvc.Testing |
| Cobertura | Coverlet |
| Consolidação da cobertura | ReportGenerator (ferramenta local) |
| Documentação da API | OpenAPI |
| Integração contínua | GitHub Actions |
| Análise estática | Analisadores nativos do .NET |

### Justificativa

A stack segue as recomendações do desafio e utiliza ferramentas integradas ao ecossistema .NET.

As dependências deverão possuir versões explícitas e compatíveis com .NET 10.

Não serão adicionados frameworks ou bibliotecas sem necessidade identificada.

## 3. Arquitetura

### 3.1 Organização

A aplicação será composta por um único projeto de API e dois projetos de testes.

A separação lógica será:

```text
Requisição HTTP
       |
       v
PasswordEndpoints
       |
       v
PasswordService
       |
       +------> PasswordGenerator
       |
       v
PasswordDbContext
       |
       v
SQLite
```

### 3.2 Responsabilidades

**PasswordEndpoints**

- Receber requisições HTTP.
- Validar o formato dos parâmetros.
- Acionar o serviço de aplicação.
- Construir as respostas HTTP.
- Não conter regras de geração ou acesso direto ao banco.

**PasswordService**

- Coordenar a geração e a persistência.
- Garantir que uma geração somente seja confirmada após a gravação.
- Recuperar senhas pelo identificador.
- Preservar as regras de negócio.

**PasswordGenerator**

- Gerar senhas de exatamente 16 caracteres.
- Garantir as categorias obrigatórias.
- Utilizar geração criptograficamente segura.
- Permanecer independente de HTTP e persistência.

**PasswordDbContext**

- Gerenciar o acesso ao SQLite.
- Configurar o mapeamento da entidade.
- Executar operações de leitura e gravação.

### 3.3 Restrições arquiteturais

- Não utilizar Clean Architecture completa.
- Não utilizar CQRS ou MediatR.
- Não implementar Repository Pattern.
- Não criar interfaces sem necessidade concreta.
- Utilizar injeção de dependências nativa do .NET.
- Não adicionar camadas para funcionalidades futuras hipotéticas.
- Manter as regras de negócio independentes dos endpoints e da persistência.

**Justificativa:** duas operações HTTP não justificam uma arquitetura mais complexa.

## 4. Geração de senhas

### 4.1 Conjunto de caracteres

| Categoria | Caracteres |
|---|---|
| Maiúsculas | `ABCDEFGHIJKLMNOPQRSTUVWXYZ` |
| Minúsculas | `abcdefghijklmnopqrstuvwxyz` |
| Números | `0123456789` |
| Especiais | `!@#$%&*` |

### 4.2 Estratégia de geração

A geração utilizará `System.Security.Cryptography.RandomNumberGenerator`.

O algoritmo deverá:

1. Selecionar um caractere aleatório de cada categoria obrigatória.
2. Preencher as 12 posições restantes com caracteres selecionados do conjunto completo permitido.
3. Embaralhar os 16 caracteres utilizando Fisher–Yates.
4. Retornar a senha após verificar suas regras de composição.

Todas as seleções de índices deverão utilizar `RandomNumberGenerator.GetInt32`, evitando viés causado por operações de módulo sobre valores aleatórios.

O embaralhamento deverá utilizar a mesma fonte criptográfica.

A estratégia garante imprevisibilidade criptográfica e a presença das quatro categorias. Não exige distribuição uniforme entre todas as senhas válidas possíveis.

Não será utilizado `System.Random`.

### 4.3 Validação

Toda senha gerada deverá atender às seguintes condições:

- Exatamente 16 caracteres.
- Pelo menos uma letra maiúscula.
- Pelo menos uma letra minúscula.
- Pelo menos um número.
- Pelo menos um símbolo permitido.
- Nenhum espaço em branco.
- Nenhum caractere fora do conjunto permitido.

Caracteres repetidos e sequências serão aceitos, conforme CL-06.

## 5. Modelo de dados

### 5.1 Entidade GeneratedPassword

| Propriedade | Tipo | Restrição |
|---|---|---|
| Id | Guid | Chave primária, obrigatório |
| Value | string | Obrigatório, exatamente 16 caracteres |

### 5.2 Persistência

- O GUID será gerado pela aplicação.
- O identificador será armazenado em representação textual canônica.
- A senha será armazenada de forma recuperável, conforme o escopo educacional.
- O banco deverá garantir a unicidade do identificador.
- O valor não poderá ser nulo.
- O comprimento deverá ser validado pela aplicação e restringido no banco quando suportado pelo mapeamento.
- A geração somente será confirmada após a conclusão da gravação.
- O arquivo SQLite não será versionado.

### 5.3 Inicialização do banco

O esquema será gerenciado por migrations do Entity Framework Core.

O procedimento de desenvolvimento será:

1. Configurar a conexão SQLite.
2. Criar a migration inicial.
3. Aplicar as migrations antes de iniciar a aplicação.
4. Iniciar a API somente com o esquema disponível.

As migrations não serão aplicadas automaticamente a cada requisição.

A aplicação não utilizará `EnsureCreated` em conjunto com migrations.

## 6. Contratos da API

A aplicação disponibilizará exatamente duas operações funcionais.

### 6.1 Gerar senha

**POST `/passwords`**

A requisição não exige corpo.

**Sucesso — HTTP 201 Created**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "password": "Ab3!Cd4@Ef5#Gh6$"
}
```

O exemplo é ilustrativo.

**Falha de persistência — HTTP 500 Internal Server Error**

A resposta deverá utilizar ProblemDetails.

### 6.2 Consultar senha

**GET `/passwords/{id}`**

**Sucesso — HTTP 200 OK**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "password": "Ab3!Cd4@Ef5#Gh6$"
}
```

**Identificador inválido — HTTP 400 Bad Request**

**Identificador inexistente — HTTP 404 Not Found**

Ambos deverão utilizar ProblemDetails.

### 6.3 Padronização dos erros

As respostas de erro utilizarão o formato ProblemDetails, com os campos:

- `type`: identificador do tipo de problema.
- `title`: descrição resumida.
- `status`: código HTTP.
- `detail`: mensagem adequada ao consumidor.

Exemplo de identificador inválido:

```json
{
  "type": "about:blank",
  "title": "Invalid identifier",
  "status": 400,
  "detail": "The provided identifier is not a valid GUID."
}
```

Exemplo de registro inexistente:

```json
{
  "type": "about:blank",
  "title": "Password not found",
  "status": 404,
  "detail": "No password was found for the provided identifier."
}
```

Exemplo de falha interna:

```json
{
  "type": "about:blank",
  "title": "Internal server error",
  "status": 500,
  "detail": "The operation could not be completed."
}
```

As mensagens técnicas de exceções não serão retornadas ao consumidor.

### 6.4 Tratamento e registro de erros

- Exceções inesperadas deverão ser tratadas de forma centralizada.
- Falhas internas deverão retornar mensagens genéricas.
- Logs deverão registrar a operação e informações técnicas necessárias ao diagnóstico.
- Valores de senhas não deverão aparecer em logs.
- Stack traces não deverão ser expostos nas respostas.
- O comportamento HTTP deverá ser consistente com os códigos definidos.

## 7. Estrutura do projeto

```text
src/
└── PasswordGenerator.Api/
    ├── Endpoints/
    │   └── PasswordEndpoints.cs
    ├── Models/
    │   └── GeneratedPassword.cs
    ├── Contracts/
    │   └── PasswordResponse.cs
    ├── Services/
    │   ├── PasswordService.cs
    │   └── PasswordGenerator.cs
    ├── Data/
    │   ├── PasswordDbContext.cs
    │   └── Migrations/
    ├── Program.cs
    └── appsettings.json

tests/
├── PasswordGenerator.UnitTests/
└── PasswordGenerator.IntegrationTests/

constitution.md
spec.md
plan.md
tasks.md
.editorconfig
.gitignore
README.md
```

A estrutura mantém os componentes necessários sem introduzir projetos adicionais de domínio ou infraestrutura.

## 8. Estratégia de testes

### 8.1 Testes unitários

O gerador deverá ser testado quanto a:

- Comprimento exato.
- Presença das quatro categorias.
- Ausência de espaços em branco.
- Ausência de caracteres inválidos.
- Aceitação de repetições.
- Aceitação de sequências válidas.

A geração será executada múltiplas vezes para verificar as propriedades obrigatórias, sem utilizar testes estatísticos como prova de segurança criptográfica.

A imprevisibilidade será avaliada pela revisão do algoritmo e pela utilização correta de `RandomNumberGenerator`.

### 8.2 Testes de integração

Os testes deverão verificar:

- Geração com HTTP 201.
- Retorno do GUID e da senha.
- Persistência do registro.
- Recuperação da senha original.
- Consulta com HTTP 200.
- GUID inválido com HTTP 400.
- GUID inexistente com HTTP 404.
- Falha de persistência com HTTP 500.
- Unicidade dos identificadores.

Os testes utilizarão `WebApplicationFactory` e SQLite isolado.

### 8.3 Simulação de falha de persistência

Para validar RF-16 e CA-09, será utilizado um ambiente de integração configurado para provocar uma falha controlada durante a gravação.

A estratégia será substituir a configuração de persistência do ambiente de testes por um DbContext configurado para falhar na operação de gravação, utilizando um mecanismo de interceptação do EF Core ou outro recurso nativo equivalente.

A validação deverá confirmar que:

1. A requisição não retorna sucesso.
2. A resposta utiliza HTTP 500.
3. A resposta não expõe informações internas.
4. Nenhuma senha é confirmada como armazenada.

Não será criado um Repository Pattern apenas para simular falhas.

### 8.4 Cobertura

A cobertura mínima obrigatória será de 80% das linhas de código da aplicação.

A medição utilizará Coverlet.

Os projetos de teste serão excluídos do cálculo.

A integração contínua deverá:

1. Executar os testes.
2. Coletar a cobertura de linhas.
3. Calcular a cobertura agregada do código de produção.
4. Reprovar a execução quando o resultado for inferior a 80%.

A cobertura não substitui os testes obrigatórios dos critérios de aceite.

## 9. Análise estática e integração contínua

### 9.1 Análise estática

O projeto utilizará `.editorconfig` e os analisadores nativos do .NET.

As verificações deverão identificar:

- Violações de formatação.
- Problemas de nomenclatura.
- Warnings de compilação.
- Problemas identificados pelos analisadores habilitados.

Warnings deverão ser tratados como erros na compilação.

### 9.2 GitHub Actions

O workflow será executado em pull requests e alterações da branch principal.

Etapas obrigatórias:

1. Restaurar dependências.
2. Verificar formatação.
3. Compilar sem warnings.
4. Executar testes unitários.
5. Executar testes de integração.
6. Coletar cobertura.
7. Validar cobertura mínima de 80%.

A execução deverá falhar caso qualquer etapa obrigatória não seja concluída com sucesso.

## 10. Rastreabilidade dos requisitos

| Requisito | Componente responsável | Validação prevista |
|---|---|---|
| RF-01 | PasswordEndpoints | Integração POST |
| RF-02 | PasswordGenerator | Teste de comprimento |
| RF-03 | PasswordGenerator | Teste de maiúsculas |
| RF-04 | PasswordGenerator | Teste de minúsculas |
| RF-05 | PasswordGenerator | Teste de números |
| RF-06 | PasswordGenerator | Teste de símbolos |
| RF-07 | PasswordGenerator | Teste de caracteres permitidos |
| RF-08 | PasswordGenerator | Teste de espaços em branco |
| RF-09 | PasswordGenerator | Revisão do algoritmo criptográfico |
| RF-10 | PasswordService | Integração de persistência |
| RF-11 | GeneratedPassword | Teste de GUID único |
| RF-12 | PasswordEndpoints | Integração da resposta POST |
| RF-13 | PasswordEndpoints | Integração GET |
| RF-14 | PasswordService | Integração de recuperação |
| RF-15 | PasswordEndpoints | Integração HTTP 400 e 404 |
| RF-16 | PasswordService | Integração de falha HTTP 500 |
| RF-17 | PasswordEndpoints | Verificação das operações expostas |

Todos os requisitos funcionais possuem componente responsável e estratégia de validação definida.

A associação com tarefas específicas será realizada no `tasks.md`.

## 11. Portões de qualidade

### Portão 1 — Compilação

- Dependências restauradas.
- Compilação sem erros.
- Nenhum warning.

### Portão 2 — Testes

- Testes unitários passando.
- Testes de integração passando.
- Cobertura mínima de 80%.

### Portão 3 — Análise estática

- Formatação conforme `.editorconfig`.
- Nenhuma violação bloqueante dos analisadores.
- Ausência de código morto ou dependências desnecessárias.

### Portão 4 — Conformidade funcional

- RF-01 a RF-17 contemplados.
- CA-01 a CA-11 verificáveis.
- Casos de borda contemplados.
- Nenhuma funcionalidade fora de escopo.

### Portão 5 — Integração contínua

- Workflow executado com sucesso.
- Todas as verificações obrigatórias aprovadas.

## 12. Execução local

Os comandos abaixo representam o procedimento esperado após a criação dos projetos.

### Restaurar dependências

```powershell
dotnet restore
```

### Aplicar migrations

```powershell
dotnet ef database update --project src/PasswordGenerator.Api
```

### Compilar

```powershell
dotnet build --no-restore -warnaserror
```

### Executar testes

```powershell
dotnet test --no-restore
```

### Executar a API

```powershell
dotnet run --project src/PasswordGenerator.Api
```

### Validar manualmente

- Solicitar geração de senha.
- Verificar comprimento e composição.
- Consultar o GUID retornado.
- Confirmar que a senha recuperada corresponde à original.
- Consultar GUID inválido.
- Consultar GUID inexistente.

Os comandos de cobertura e análise estática deverão ser consolidados na configuração do workflow, conforme as ferramentas e versões efetivamente instaladas.

## 13. Decisões técnicas

| Decisão | Justificativa |
|---|---|
| Minimal APIs | Simplicidade para duas operações |
| EF Core | Persistência e migrations integradas |
| SQLite | Banco local sem servidor dedicado |
| DbContext direto no serviço | Evita repositórios desnecessários |
| RandomNumberGenerator | Aleatoriedade criptograficamente segura |
| Fisher–Yates | Embaralhamento sem viés quando utilizado com seleção uniforme |
| ProblemDetails | Padronização de erros |
| xUnit | Testes automatizados no ecossistema .NET |
| WebApplicationFactory | Testes HTTP de integração |
| Coverlet | Medição automatizada de cobertura |
| ReportGenerator | Consolida os relatórios do Coverlet dos dois projetos de teste em uma cobertura agregada |
| GitHub Actions | Execução dos portões de qualidade |

## 14. Riscos e limitações

### Armazenamento recuperável

As senhas serão armazenadas de forma recuperável.

**Risco:** acesso ao banco permite visualizar as senhas.

**Mitigação:** utilização exclusivamente educacional, sem credenciais reais.

### Ausência de autenticação

A consulta não exigirá autenticação.

**Risco:** qualquer consumidor que conheça um GUID poderá recuperar a senha correspondente.

**Mitigação:** não disponibilizar a aplicação para uso em produção.

### Persistência local

O banco será armazenado em arquivo SQLite.

**Risco:** perda ou exclusão do arquivo resulta em perda dos registros.

**Mitigação:** manter o banco fora do versionamento e utilizar dados descartáveis.

### Complexidade arquitetural

A introdução de padrões adicionais pode aumentar desnecessariamente a complexidade.

**Mitigação:** exigir justificativa para novas abstrações, camadas e dependências.

## 15. Verificação de conformidade com a Constituição

| Princípio | Evidência no Plan | Resultado |
|---|---|---|
| Stack justificada | Seções 2 e 13 | Conforme |
| Simplicidade arquitetural | Seção 3 | Conforme |
| Separação de responsabilidades | Seções 3 e 7 | Conforme |
| Testabilidade | Seção 8 | Conforme |
| Cobertura mínima | Seções 8 e 11 | Conforme |
| Segurança | Seções 4, 6 e 14 | Conforme |
| Convenções | Seções 7 e 9 | Conforme |
| Rastreabilidade | Seção 10 | Conforme |
| Escopo | Seções 1 e 6 | Conforme |
| Integração contínua | Seção 9 | Conforme |
| Governança | Seções 10, 11 e 16 | Conforme |

Nenhuma exceção aos princípios arquiteturais foi identificada.

## 16. Resultado da revisão cruzada

Foram incorporadas as correções identificadas na revisão documental.

| Identificador | Correção | Resultado |
|---|---|---|
| RC-01 | Seleção aleatória e embaralhamento seguros | Resolvido |
| RC-02 | Simulação de falha de persistência | Resolvido |
| RC-03 | Matriz de rastreabilidade RF → componente → teste | Resolvido |
| RC-04 | Contratos ProblemDetails e tratamento de erros | Resolvido |
| RC-05 | Procedimento de inicialização do banco | Resolvido |
| RC-06 | Verificação automatizada de cobertura | Resolvido |
| RC-07 | Nomenclatura GeneratedPassword | Resolvido |
| RC-08 | Instruções de execução e validação | Resolvido |

### Critérios de aprovação

- As decisões técnicas estão documentadas e justificadas.
- A arquitetura contempla os requisitos funcionais.
- Os contratos HTTP estão definidos.
- O modelo de dados está definido.
- A estratégia de testes está documentada.
- Os riscos conhecidos estão registrados.
- Os requisitos possuem responsáveis e validações previstas.
- Não foram identificadas contradições documentais bloqueantes com a Constitution ou a Specification.

**Resultado: aprovado na revisão documental para elaboração das tarefas.**

A aprovação não representa validação de código. Os portões de qualidade da implementação deverão ser executados durante o desenvolvimento.

---

**Versão:** 1.1.0  
**Status:** Aprovado na revisão documental  
**Próxima etapa:** Tasks