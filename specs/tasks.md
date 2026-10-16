# Tarefas — Gerador de Senhas Fortes

**Versão:** 1.2.0  
**Data:** 2026-10-16  
**Status:** Aprovado para implementação  
**Referências:** `constitution.md`, `spec.md` e `plan.md` (v1.1.0)

## 1. Objetivo

Decompor a implementação da API de geração e consulta de senhas em tarefas pequenas, ordenadas, rastreáveis e verificáveis. Cada tarefa deve produzir uma alteração versionável e um commit com Conventional Commits, respeitando a Constitution.

## 2. Convenções e política de execução

- `[ ]`: tarefa pendente; `[X]`: tarefa concluída.
- `[P]`: tarefa potencialmente paralelizável, desde que não haja dependências pendentes nem conflito de arquivos.
- `[US1]`: geração de senhas; `[US2]`: consulta de senhas.
- `T001` etc.: identificadores únicos.
- Os caminhos são relativos à raiz do repositório.

**Testes antes da implementação:** testes introduzidos antes do comportamento correspondente **devem compilar e executar**. A tarefa de teste pode criar o contrato ou esqueleto mínimo necessário (por exemplo, método que lança `NotImplementedException` ou endpoint provisório que retorna erro), sem implementar antecipadamente a funcionalidade. Nesse estágio, uma falha de asserção esperada é aceitável e deve ser registrada. Erros de compilação não são evidência válida de teste vermelho. A história somente é concluída com todos os seus testes passando.

**Commits:** cada tarefa deve produzir uma alteração versionável e um commit rastreável. Não criar commits vazios apenas para registrar verificações. Validações operacionais devem integrar uma tarefa que altere testes, configuração ou documentação.

**Casos de borda:** antes de implementar US1 e US2, conferir as descrições **originais** de CB-01 a CB-13 no `spec.md` e associá-las às tarefas de geração, persistência ou consulta. Os testes de cada caso devem ser incorporados à respectiva história; T031 será uma auditoria de cobertura, não o primeiro momento de descoberta de casos pendentes. Não inferir a natureza de um caso apenas por seu número.

## 3. Fase 1 — Preparação

**Objetivo:** criar a solução e estabelecer as configurações básicas.

- [X] **T001** Criar a solução .NET 10 e o projeto Minimal API em `src/PasswordGenerator.Api/`, incluindo-o na solução. **Concluído quando:** a API inicial compilar sem erros ou warnings.
- [X] **T002** Criar os projetos xUnit em `tests/PasswordGenerator.UnitTests/` e `tests/PasswordGenerator.IntegrationTests/`, incluindo-os na solução. **Concluído quando:** ambos executarem seus testes iniciais com sucesso.
- [X] **T003** [P] Configurar `.editorconfig`, `.gitignore` e propriedades de compilação para tratar warnings como erros. **Concluído quando:** a configuração estiver versionada e a solução compilar.
- [X] **T004** Configurar EF Core, SQLite, OpenAPI, `Microsoft.AspNetCore.Mvc.Testing` e Coverlet nos respectivos `.csproj`, com versões compatíveis. **Concluído quando:** restauração e compilação forem bem-sucedidas.
- [X] **T005** Organizar `src/PasswordGenerator.Api/Program.cs` e os arquivos iniciais necessários à estrutura definida no Plan, sem criar componentes ociosos. **Concluído quando:** a API iniciar e a estrutura permanecer minimalista.

**Dependências:** T001 → T002 e T003; T002/T003 → T004 → T005.  
**Portão:** restauração, compilação e análise estática sem falhas.

## 4. Fase 2 — Persistência

**Objetivo:** preparar o armazenamento SQLite.

- [X] **T006** Criar `src/PasswordGenerator.Api/Models/GeneratedPassword.cs` com `Id` e `Value`. **Concluído quando:** a entidade representar o modelo aprovado.
- [X] **T007** Criar `src/PasswordGenerator.Api/Data/PasswordDbContext.cs`, configurando chave primária, campo obrigatório e restrição de comprimento. **Concluído quando:** o mapeamento refletir o Plan.
- [X] **T008** Configurar a conexão SQLite em `src/PasswordGenerator.Api/appsettings.json` e registrar o DbContext em `Program.cs`. **Concluído quando:** a aplicação resolver o DbContext por injeção de dependências.
- [X] **T009** Criar a migration inicial em `src/PasswordGenerator.Api/Data/Migrations/`, aplicá-la em desenvolvimento e verificar o esquema. **Concluído quando:** o banco puder ser recriado pelas migrations e não for versionado.

**Dependências:** T005 → T006 → T007 → T008 → T009.  
**Requisitos:** RF-10, RF-11.  
**Portão:** esquema reproduzível, restrições aplicadas e compilação aprovada.

## 5. Fase 3 — US-01: geração de senhas

**Prioridade:** P1. **Objetivo:** gerar, persistir e retornar senhas válidas.

### 5.1 Geração criptográfica

- [X] **T010** [US1] Criar `tests/PasswordGenerator.UnitTests/PasswordGeneratorTests.cs` com testes para comprimento de 16 caracteres e presença das quatro categorias. Criar o esqueleto mínimo compilável de `src/PasswordGenerator.Api/Services/PasswordGenerator.cs`, se necessário. **Concluído quando:** testes compilarem, executarem e falharem de forma esperada antes da implementação.
- [X] **T011** [US1] Implementar seleção criptográfica de caracteres em `PasswordGenerator.cs` usando `RandomNumberGenerator.GetInt32`. **Concluído quando:** testes de comprimento e categorias passarem.
- [X] **T012** [US1] Acrescentar a `PasswordGeneratorTests.cs` testes de conjunto permitido, ausência de espaços e não rejeição de repetições ou sequências válidas. Incluir os casos de borda de geração identificados no `spec.md`. **Concluído quando:** os testes compilarem e verificarem as regras sem restrições extras.
- [X] **T013** [US1] Implementar embaralhamento Fisher–Yates em `PasswordGenerator.cs` com `RandomNumberGenerator.GetInt32`. **Concluído quando:** testes de geração passarem e a revisão confirmar ausência de `System.Random` ou seleção enviesada por módulo.

### 5.2 Geração e persistência

- [X] **T014** [US1] Criar `tests/PasswordGenerator.IntegrationTests/PasswordCreationTests.cs` e infraestrutura `WebApplicationFactory` com SQLite isolado. Testar HTTP 201, GUID, senha e persistência. Se necessário, criar contrato/endpoint mínimo compilável sem comportamento funcional. Incorporar os casos de borda de criação identificados na Specification. **Concluído quando:** testes compilarem, executarem e evidenciarem a funcionalidade ainda ausente.
- [X] **T015** [US1] Implementar `src/PasswordGenerator.Api/Services/PasswordService.cs` para gerar a senha, criar GUID e persistir antes de confirmar sucesso. **Concluído quando:** o serviço compilar e respeitar a ordem de operações.
- [ ] **T016** [US1] Criar `src/PasswordGenerator.Api/Contracts/PasswordResponse.cs` e implementar `POST /passwords` em `src/PasswordGenerator.Api/Endpoints/PasswordEndpoints.cs`, registrando-o em `Program.cs`. **Concluído quando:** testes retornarem HTTP 201 e confirmarem GUID e senha persistida.

### 5.3 Falhas e segurança

- [ ] **T017** [US1] Adicionar a `PasswordCreationTests.cs` um cenário de falha controlada de gravação por interceptação do EF Core ou mecanismo nativo equivalente. **Concluído quando:** o teste compilar, executar e demonstrar o comportamento de erro ainda não implementado, sem falso sucesso.
- [ ] **T018** [US1] Implementar tratamento centralizado de exceções em `Program.cs` com HTTP 500 e ProblemDetails. **Concluído quando:** o teste de falha passar, sem confirmação de persistência ou exposição de detalhes internos.
- [ ] **T019** [US1] Adicionar testes em `PasswordCreationTests.cs` para unicidade de GUIDs e ausência de senhas nos logs, inclusive durante erros. **Concluído quando:** os testes passarem e a suíte US1 estiver integralmente aprovada.

**Dependências:** T009 → T010 → T011 → T012 → T013 → T014 → T015 → T016 → T017 → T018 → T019.  
**Requisitos:** RF-01 a RF-12, RF-16.  
**Critérios de aceite:** CA-01 a CA-05, CA-09, CA-10.  
**Portão:** geração criptográfica revisada, persistência comprovada, respostas corretas e testes US1 aprovados.

## 6. Fase 4 — US-02: consulta de senhas

**Prioridade:** P2. **Objetivo:** recuperar a senha original por GUID.

- [ ] **T020** [US2] Criar `tests/PasswordGenerator.IntegrationTests/PasswordRetrievalTests.cs` para consulta de registro existente e comparação com a senha original. Criar contrato/endpoint mínimo compilável, se necessário, sem antecipar a lógica. **Concluído quando:** o teste compilar, executar e evidenciar o comportamento ausente.
- [ ] **T021** [US2] Implementar consulta por GUID em `PasswordService.cs`. **Concluído quando:** o serviço recuperar registros sem modificá-los.
- [ ] **T022** [US2] Implementar `GET /passwords/{id}` em `PasswordEndpoints.cs`. **Concluído quando:** a integração retornar HTTP 200 e a senha original.
- [ ] **T023** [US2] Adicionar a `PasswordRetrievalTests.cs` testes para GUID inválido, GUID inexistente e demais casos de borda de consulta identificados no `spec.md`. **Concluído quando:** testes compilarem, executarem e identificarem comportamentos pendentes.
- [ ] **T024** [US2] Implementar HTTP 400 e 404 com ProblemDetails em `PasswordEndpoints.cs`. **Concluído quando:** todos os testes de consulta passarem e o contrato de erro estiver correto.

**Dependências:** T019 → T020 → T021 → T022 → T023 → T024.  
**Requisitos:** RF-13 a RF-15.  
**Critérios de aceite:** CA-06 a CA-08.  
**Portão:** consulta correta, erros padronizados e testes US2 aprovados.

## 7. Fase 5 — Qualidade e integração contínua

**Objetivo:** automatizar os portões de qualidade da Constitution.

- [ ] **T025** Configurar coleta e consolidação de cobertura do código de produção nos projetos de teste. **Concluído quando:** a execução produzir relatórios válidos.
- [ ] **T026** Configurar verificação automática de cobertura agregada de linhas mínima de 80%. **Concluído quando:** a verificação retornar código de saída não zero abaixo de 80% e zero a partir de 80%.
- [ ] **T027** [P] Configurar análise estática e verificação de formatação nos arquivos de configuração da solução. **Concluído quando:** violações bloqueantes provocarem falha.
- [ ] **T028** Criar `.github/workflows/ci.yml` para restauração, compilação, formatação, testes e cobertura em pull requests e alterações da branch principal. **Concluído quando:** todas as verificações forem executadas pelo workflow.
- [ ] **T029** Adicionar verificações automatizadas das condições de reprovação dos portões, usando execução controlada e sem quebrar a branch principal. **Concluído quando:** as falhas forem demonstráveis e a configuração final permanecer válida.

**Dependências:** T024 → T025 → T026; T027 pode ocorrer em paralelo com T025; T026/T027 → T028 → T029.  
**Portão:** build sem warnings, testes aprovados, cobertura ≥80% e CI funcional.

## 8. Fase 6 — Validação final

**Objetivo:** consolidar evidências de conformidade.

- [ ] **T030** Criar `tests/PasswordGenerator.IntegrationTests/PasswordContractTests.cs` para formatos HTTP e exposição de somente duas operações funcionais. **Concluído quando:** contratos estiverem cobertos por testes.
- [ ] **T031** Auditar a cobertura de **cada** caso de borda CB-01 a CB-13, adicionando aos testes da história correspondente apenas os cenários ainda não contemplados. **Concluído quando:** cada CB tiver teste identificável ou justificativa explícita de revisão técnica, sem descobrir requisitos apenas nesta fase.
- [ ] **T032** Atualizar `README.md` com pré-requisitos, restauração, migrations, execução, testes e limitações de segurança. **Concluído quando:** outra pessoa puder reproduzir a execução.
- [ ] **T033** Atualizar a rastreabilidade deste `tasks.md` com nomes de testes e evidências para RF-01 a RF-17, CA-01 a CA-11 e CB-01 a CB-13. **Concluído quando:** todos possuírem evidência verificável.
- [ ] **T034** Registrar no `README.md` o procedimento reproduzível e os resultados finais de build, testes, cobertura, análise estática e conformidade com a Constitution. **Concluído quando:** todos os portões estiverem aprovados.

**Dependências:** T029 → T030 → T031 → T032 → T033 → T034.  
**Portão final:** requisitos e critérios de aceite validados, testes passando, cobertura ≥80%, CI aprovada e nenhuma violação da Constitution.

## 9. Matriz de rastreabilidade funcional

| Requisito | Tarefas |
|---|---|
| RF-01 | T014–T016 |
| RF-02 | T010–T013 |
| RF-03 | T010–T013 |
| RF-04 | T010–T013 |
| RF-05 | T010–T013 |
| RF-06 | T010–T013 |
| RF-07 | T012–T013 |
| RF-08 | T012–T013 |
| RF-09 | T011–T013 |
| RF-10 | T006–T009, T015–T016 |
| RF-11 | T006–T007, T015, T019 |
| RF-12 | T014, T016 |
| RF-13 | T020–T022 |
| RF-14 | T020–T022 |
| RF-15 | T023–T024 |
| RF-16 | T017–T018 |
| RF-17 | T016, T022, T030 |

## 10. Matriz de critérios de aceite

| Critério | Tarefas |
|---|---|
| CA-01 | T010–T013 |
| CA-02 | T010–T013 |
| CA-03 | T012–T013 |
| CA-04 | T014–T016 |
| CA-05 | T014–T016 |
| CA-06 | T020–T022 |
| CA-07 | T023–T024 |
| CA-08 | T023–T024 |
| CA-09 | T017–T018 |
| CA-10 | T011–T013 |
| CA-11 | T030–T031 |

## 11. Rastreabilidade dos casos de borda

Antes de T010 e T014, classificar CB-01 a CB-13 usando as descrições originais do `spec.md`:

- **Geração e composição:** incorporar em T010, T012 e T013, conforme aplicável.
- **Persistência e falhas:** incorporar em T014, T017 e T019, conforme aplicável.
- **Consulta e identificadores:** incorporar em T020, T023 e T024, conforme aplicável.

Registrar a associação individual de cada CB ao teste correspondente durante a execução. T031 confirma a completude e corrige eventuais lacunas; não substitui os testes das histórias.

## 12. Dependências gerais

```text
Fase 1 — Preparação
        ↓
Fase 2 — Persistência
        ↓
Fase 3 — US1: Geração
        ↓
Fase 4 — US2: Consulta
        ↓
Fase 5 — Qualidade e CI
        ↓
Fase 6 — Validação final
```

Tarefas `[P]` só podem ser paralelizadas com dependências satisfeitas, sem conflitos de arquivos e com commits independentes.

## 13. Definição de concluído

Uma tarefa está concluída quando:

1. Sua entrega estiver implementada e versionável.
2. Seus critérios individuais forem atendidos.
3. Os testes aplicáveis compilarem, executarem e apresentarem o resultado esperado para o estágio atual (falha de asserção intencional permitida apenas em etapa de teste antes da implementação).
4. A solução compilar sem erros ou warnings.
5. Nenhuma funcionalidade fora de escopo tiver sido adicionada.
6. Constitution, Specification e Plan forem respeitados.
7. Um commit rastreável com Conventional Commits for criado.

**Conclusão de história:** todos os testes da história devem passar.  
**Conclusão do projeto:** todos os portões de qualidade devem passar, com evidências de RF, CA e CB.

## 14. Instruções para implementação assistida por IA

Executar **uma tarefa por vez**. Para cada tarefa:

1. Identificar requisitos, critérios de aceite e casos de borda relacionados.
2. Consultar Constitution, Specification e Plan.
3. Implementar apenas o escopo da tarefa, incluindo esqueleto mínimo quando necessário para testes compiláveis.
4. Executar as verificações pertinentes e registrar resultado esperado e observado.
5. Informar os arquivos modificados.
6. Criar o commit correspondente.
7. Avançar somente após a conclusão.

Não antecipar funcionalidades, introduzir abstrações não previstas, alterar decisões técnicas sem atualização documental, ignorar falhas inesperadas ou marcar tarefas como concluídas sem evidência.

---

**Total:** 34 tarefas em 6 fases.  
**Revisão:** QC-09 e QC-10 incorporadas.  
**Status:** Aprovado para implementação; iniciar pela T001.
