# Gerador de Senhas Fortes — Specification

**Status:** Clarified  
**Criado em:** 2026-10-16  
**Última atualização:** 2026-10-16

## 1. Problema

É necessário disponibilizar uma maneira simples de gerar senhas fortes e recuperá-las posteriormente por meio de um identificador único.

O sistema tem finalidade educacional, servindo como base para praticar Spec-Driven Development, sem pretensão de uso em produção.

## 2. Objetivo

Permitir que um usuário gere uma senha de 16 caracteres que atenda aos critérios definidos de composição e imprevisibilidade, armazene essa senha e posteriormente a consulte utilizando o identificador recebido durante a geração.

## 3. Usuários

**Usuário consumidor:** pessoa ou sistema que solicita a geração de uma senha e utiliza seu identificador para consultá-la posteriormente.

Não haverá gerenciamento de contas ou perfis de acesso nesta versão.

## 4. Histórias de usuário

### US-01 — Gerar senha forte (P1)

Como usuário, quero solicitar a geração de uma senha forte para obter uma credencial que atenda aos critérios definidos.

**Prioridade:** P1 — Funcionalidade principal do sistema.

**Teste independente:** solicitar uma senha e verificar seu comprimento, composição, identificador e persistência.

**Cenários de aceitação:**

1. **Dado** que o serviço esteja disponível, **quando** o usuário solicitar uma senha, **então** o sistema deve gerar uma senha com exatamente 16 caracteres, contendo pelo menos uma letra maiúscula, uma minúscula, um número e um símbolo permitido.
2. **Dado** que uma senha seja gerada, **quando** sua composição for verificada, **então** ela não deve conter espaços em branco ou caracteres fora dos conjuntos permitidos.
3. **Dado** que uma senha válida tenha sido gerada e armazenada, **quando** a operação for concluída, **então** o sistema deve retornar a senha e seu GUID.
4. **Dado** que o armazenamento falhe, **quando** o usuário solicitar a geração, **então** o sistema deve retornar um erro e não confirmar o sucesso da operação.
5. **Dado** que múltiplas senhas sejam geradas, **quando** as operações forem concluídas, **então** cada registro deve possuir um GUID único.

### US-02 — Consultar senha gerada (P2)

Como usuário, quero consultar uma senha utilizando seu identificador para recuperar uma senha previamente gerada.

**Prioridade:** P2 — Complementa a geração e depende da existência de uma senha armazenada.

**Teste independente:** utilizar um identificador de um registro previamente criado e verificar se a senha original é recuperada.

**Cenários de aceitação:**

1. **Dado** que exista uma senha armazenada, **quando** o usuário consultar seu GUID, **então** o sistema deve retornar a senha originalmente associada.
2. **Dado** um GUID válido sem registro correspondente, **quando** o usuário realizar a consulta, **então** o sistema deve informar que a senha não foi encontrada.
3. **Dado** um identificador com formato inválido, **quando** o usuário realizar a consulta, **então** o sistema deve rejeitar a solicitação como inválida.

## 5. Requisitos funcionais

- **RF-01:** O sistema DEVE permitir solicitar a geração de uma senha.
- **RF-02:** Toda senha gerada DEVE possuir exatamente 16 caracteres.
- **RF-03:** Toda senha gerada DEVE conter pelo menos uma letra maiúscula (`A-Z`).
- **RF-04:** Toda senha gerada DEVE conter pelo menos uma letra minúscula (`a-z`).
- **RF-05:** Toda senha gerada DEVE conter pelo menos um número (`0-9`).
- **RF-06:** Toda senha gerada DEVE conter pelo menos um caractere especial do conjunto `!@#$%&*`.
- **RF-07:** Toda senha gerada DEVE utilizar exclusivamente letras latinas não acentuadas, números e os caracteres especiais permitidos.
- **RF-08:** Nenhuma senha gerada PODE conter espaços em branco.
- **RF-09:** A geração DEVE utilizar um processo imprevisível e adequado à criação de senhas.
- **RF-10:** Toda senha gerada com sucesso DEVE ser armazenada.
- **RF-11:** Toda senha armazenada DEVE possuir um identificador único no formato GUID.
- **RF-12:** Uma geração bem-sucedida DEVE retornar o GUID e a senha gerada.
- **RF-13:** O sistema DEVE permitir consultar uma senha armazenada por meio de seu GUID.
- **RF-14:** Uma consulta bem-sucedida DEVE retornar a senha originalmente associada ao GUID.
- **RF-15:** O sistema DEVE tratar consultas com identificadores inválidos ou inexistentes.
- **RF-16:** Uma falha de armazenamento DEVE resultar em erro, sem confirmação de sucesso.
- **RF-17:** O sistema DEVE disponibilizar somente duas operações funcionais: geração e consulta de senha.

## 6. Regras de negócio

- **RN-01:** O comprimento da senha deve ser exatamente 16 caracteres.
- **RN-02:** A senha deve conter pelo menos um caractere de cada categoria obrigatória: maiúscula, minúscula, número e símbolo.
- **RN-03:** Os únicos símbolos especiais permitidos são `!@#$%&*`.
- **RN-04:** Não são permitidos espaços em branco, letras acentuadas ou caracteres fora dos conjuntos definidos.
- **RN-05:** A geração deve ser imprevisível. Não existem restrições adicionais de sequências ou repetição de caracteres.
- **RN-06:** Cada registro armazenado deve possuir um GUID único.
- **RN-07:** Uma geração somente pode ser considerada concluída com sucesso após o armazenamento da senha.
- **RN-08:** A consulta deve recuperar o valor original da senha, sem modificações.

## 7. Entidades conceituais

### Senha gerada

Representa uma senha produzida pelo sistema e disponível para consulta posterior.

| Atributo | Descrição |
|---|---|
| Identificador | Referência única no formato GUID |
| Senha | Valor gerado de 16 caracteres conforme as regras de composição |

O modelo de persistência será definido no Plan.

## 8. Casos de borda

| ID | Situação | Comportamento esperado |
|---|---|---|
| CB-01 | Senha com exatamente 16 caracteres e composição válida | Aceitar |
| CB-02 | Senha com comprimento diferente de 16 caracteres | Rejeitar |
| CB-03 | Senha sem letra maiúscula | Rejeitar |
| CB-04 | Senha sem letra minúscula | Rejeitar |
| CB-05 | Senha sem número | Rejeitar |
| CB-06 | Senha sem símbolo permitido | Rejeitar |
| CB-07 | Senha com espaço em branco | Rejeitar |
| CB-08 | Senha com caractere não permitido | Rejeitar |
| CB-09 | Senha com caracteres repetidos ou sequenciais, mas composição válida | Aceitar |
| CB-10 | GUID válido sem registro correspondente | Informar que a senha não foi encontrada |
| CB-11 | Identificador com formato inválido | Rejeitar a solicitação |
| CB-12 | Falha no armazenamento | Retornar erro, sem confirmação de sucesso |
| CB-13 | Geração de múltiplas senhas | Cada registro deve possuir GUID único |

Os casos CB-02 a CB-09 estabelecem condições para validação do resultado da geração, não uma funcionalidade de cadastro manual de senhas.

## 9. Fora de escopo

- Autenticação e autorização de usuários.
- Gerenciamento de contas e perfis.
- Criptografia ou hashing das senhas armazenadas, conforme o escopo educacional do desafio.
- Exclusão, atualização ou expiração de senhas.
- Personalização do comprimento ou das regras de composição.
- Interface gráfica.
- Compartilhamento de senhas.
- Histórico de consultas e auditoria de acessos.
- Integrações com serviços externos.
- Regras adicionais de bloqueio de sequências ou caracteres repetidos.
- Utilização da aplicação em ambiente de produção.

## 10. Critérios de aceite

- **CA-01:** Toda senha gerada com sucesso possui exatamente 16 caracteres.
- **CA-02:** Toda senha gerada contém pelo menos uma letra maiúscula, uma minúscula, um número e um símbolo permitido.
- **CA-03:** Nenhuma senha gerada contém espaços em branco ou caracteres fora dos conjuntos permitidos.
- **CA-04:** Toda senha gerada com sucesso possui um GUID único e está armazenada.
- **CA-05:** A resposta de geração contém o GUID e a senha correspondente.
- **CA-06:** Uma senha armazenada pode ser recuperada utilizando seu GUID.
- **CA-07:** Uma consulta não altera a senha armazenada.
- **CA-08:** Identificadores inválidos e inexistentes recebem tratamentos de erro apropriados.
- **CA-09:** Uma falha de armazenamento não produz uma confirmação falsa de sucesso.
- **CA-10:** A geração utiliza um processo imprevisível, sem exigir restrições adicionais de repetição ou sequência.
- **CA-11:** O sistema disponibiliza somente as duas operações funcionais previstas no desafio.

## 11. Critérios de sucesso mensuráveis

- **SC-01:** 100% das senhas geradas com sucesso possuem exatamente 16 caracteres.
- **SC-02:** 100% das senhas geradas com sucesso atendem aos critérios obrigatórios de composição.
- **SC-03:** 100% das gerações confirmadas possuem registro persistido e GUID único.
- **SC-04:** 100% das consultas de identificadores existentes retornam a senha originalmente associada.
- **SC-05:** Todos os casos de borda documentados possuem comportamento verificável.
- **SC-06:** As duas histórias de usuário podem ser validadas por cenários de aceitação independentes.

## 12. Clarifications

### Sessão de 2026-10-16

**CL-01 — Composição da senha**

- **Pergunta:** A senha deve conter obrigatoriamente letras maiúsculas, minúsculas e números?
- **Decisão:** Sim. Toda senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um símbolo especial.
- **Impacto:** US-01, RF-03 a RF-06, RN-02, CB-03 a CB-06, CA-02.

**CL-02 — Comprimento da senha**

- **Pergunta:** O comprimento deve ser fixo ou variável?
- **Decisão:** Exatamente 16 caracteres.
- **Impacto:** US-01, RF-02, RN-01, CB-01 e CB-02, CA-01.

**CL-03 — Caracteres especiais**

- **Pergunta:** Quais caracteres especiais serão permitidos?
- **Decisão:** Somente os símbolos `!@#$%&*`.
- **Impacto:** US-01, RF-06 a RF-08, RN-03 e RN-04, CB-06 a CB-08, CA-02 e CA-03.

**CL-04 — Resposta da geração**

- **Pergunta:** A geração deve retornar somente o GUID ou também a senha?
- **Decisão:** Retornar o GUID e a senha gerada.
- **Impacto:** US-01, RF-12, CA-05.

**CL-05 — Falha no armazenamento**

- **Pergunta:** O que deve acontecer quando o armazenamento falhar?
- **Decisão:** Retornar erro e não confirmar a geração.
- **Impacto:** US-01, RF-10 e RF-16, RN-07, CB-12, CA-09.

**CL-06 — Regras adicionais de segurança**

- **Pergunta:** Devem existir restrições para sequências ou caracteres repetidos?
- **Decisão:** Não. A geração deve ser imprevisível e cumprir os critérios de composição, sem restrições adicionais.
- **Impacto:** RF-09, RN-05, CB-09, CA-10 e Fora de Escopo.

## 13. Estado da especificação

As seis ambiguidades identificadas na primeira versão foram resolvidas e incorporadas aos requisitos, regras de negócio, cenários de aceitação e critérios de sucesso.

**Status: Clarified — pronta para revisão de consistência e posterior planejamento técnico.**