# Constituição do Projeto

**Versão:** 1.0.0  
**Ratificação:** 2026-10-16  
**Última alteração:** 2026-10-16

Esta Constituição estabelece os princípios obrigatórios que orientam a especificação, o planejamento, a implementação e a validação do projeto.

As regras descritas são inegociáveis, salvo alteração formal desta Constituição. Em caso de conflito, os documentos e decisões posteriores devem ser revisados para garantir conformidade.

Os termos **DEVE**, **NÃO DEVE** e **PODE** indicam, respectivamente, obrigação, proibição e permissão.

## Stack

### I. Tecnologias e dependências

- A stack tecnológica DEVE ser definida e justificada no `plan.md`, respeitando as restrições do desafio.
- NÃO DEVEM ser adicionados frameworks, bibliotecas ou ferramentas sem uma necessidade identificada.
- Recursos nativos da stack escolhida DEVEM ser priorizados antes da introdução de dependências externas.
- Toda dependência adicional DEVE possuir finalidade documentada e ser compatível com as tecnologias existentes.
- NÃO DEVEM ser introduzidas tecnologias exclusivamente para atender a necessidades futuras hipotéticas.
- Dependências DEVEM utilizar versões explícitas e compatíveis, sem atualizações automáticas não controladas.

**Justificativa:** garantir uma stack coerente, reproduzível e proporcional ao escopo do projeto.

## Arquitetura

### II. Simplicidade e separação de responsabilidades

- A arquitetura DEVE priorizar a solução mais simples que atenda integralmente aos requisitos.
- NÃO DEVEM ser introduzidas camadas, abstrações, interfaces ou padrões arquiteturais sem necessidade concreta.
- A camada de apresentação DEVE ser responsável pelo recebimento das requisições e pela construção das respostas.
- Regras de negócio NÃO DEVEM ser implementadas diretamente nos endpoints.
- O acesso à persistência DEVE permanecer separado das regras de negócio.
- As regras de negócio DEVEM poder ser testadas independentemente da infraestrutura.
- NÃO DEVEM existir dependências circulares entre componentes.
- A estrutura do projeto DEVE ser definida no Plan e mantida consistente durante a implementação.

### III. Escopo e evolução

- A implementação DEVE atender exclusivamente aos requisitos aprovados na Specification.
- NÃO DEVEM ser adicionadas funcionalidades, abstrações ou integrações não especificadas.
- Refatorações DEVEM preservar o comportamento estabelecido nos requisitos.
- Qualquer aumento significativo de complexidade DEVE possuir justificativa documentada no Plan.

**Justificativa:** preservar a clareza arquitetural e evitar overengineering, mantendo o foco no aprendizado de SDD.

## Qualidade

### IV. Testes automatizados

- Toda regra de negócio DEVE possuir testes unitários.
- Todo endpoint DEVE possuir testes de integração cobrindo seu fluxo principal.
- Casos de borda e cenários de erro identificados na Specification DEVEM possuir testes correspondentes.
- A cobertura mínima de código DEVE ser de 80% das linhas, sem substituir a obrigatoriedade de testar os comportamentos especificados.
- Testes DEVEM ser determinísticos, independentes e reproduzíveis.
- Testes NÃO DEVEM depender da ordem de execução.
- Dependências externas DEVEM ser isoladas nos testes unitários.
- Nenhuma tarefa DEVE ser concluída com testes falhando.

### V. Qualidade estática e integração contínua

- O projeto DEVE compilar sem erros ou warnings.
- O código DEVE respeitar as regras de formatação e análise estática configuradas para a stack.
- Um arquivo `.editorconfig` DEVE estabelecer as convenções de formatação aplicáveis.
- A integração contínua DEVE executar, no mínimo, restauração de dependências, compilação, testes automatizados e verificação de cobertura.
- Alterações que falharem em qualquer verificação obrigatória NÃO DEVEM ser integradas à branch principal.
- Código morto, dependências não utilizadas e warnings ignorados NÃO DEVEM permanecer na versão final.

### VI. Segurança e tratamento de dados

- Toda entrada externa DEVE ser validada antes de ser processada.
- Entradas inválidas DEVEM produzir respostas controladas e consistentes com os contratos definidos no Plan.
- Exceções NÃO DEVEM ser silenciosamente ignoradas.
- Respostas de erro NÃO DEVEM expor stack traces, credenciais ou informações internas sensíveis.
- Segredos e credenciais NÃO DEVEM ser armazenados diretamente no código-fonte.
- Dados sensíveis NÃO DEVEM ser registrados em logs.
- Operações que envolvam geração de valores sensíveis ou imprevisíveis DEVEM utilizar mecanismos criptograficamente seguros.
- Quando houver persistência de datas e horários, os valores DEVEM ser armazenados em UTC.
- Dados persistidos DEVEM respeitar as restrições de integridade estabelecidas na Specification e no Plan.

**Justificativa:** assegurar que a implementação seja funcional, verificável e segura, mesmo em um projeto educacional.

## Convenções

### VII. Código e nomenclatura

- Identificadores de código DEVEM utilizar inglês.
- Classes, métodos, propriedades e variáveis DEVEM seguir as convenções oficiais da linguagem escolhida.
- Nomes DEVEM representar claramente suas responsabilidades.
- Abreviações ambíguas e nomes genéricos DEVEM ser evitados.
- Métodos DEVEM possuir responsabilidades claramente delimitadas.
- Comentários DEVEM explicar decisões ou comportamentos não evidentes, sem apenas repetir o código.
- NÃO DEVE permanecer código comentado, obsoleto ou não utilizado.
- A duplicação de lógica de negócio DEVE ser evitada, sem introduzir abstrações desnecessárias.

### VIII. Documentação e versionamento

- Os documentos de SDD DEVEM ser escritos em português.
- Os documentos DEVEM utilizar Markdown e manter nomenclatura consistente.
- Requisitos funcionais DEVEM possuir identificadores únicos e rastreáveis.
- Decisões arquiteturais relevantes DEVEM ser registradas no Plan, acompanhadas de justificativa.
- A estrutura de diretórios e as convenções de arquivos DEVEM ser documentadas no Plan.
- Commits DEVEM seguir o padrão Conventional Commits.
- Cada tarefa implementada DEVE gerar um commit próprio, contendo apenas alterações relacionadas à tarefa.
- Mensagens de commit DEVEM descrever objetivamente a alteração realizada.

**Justificativa:** garantir legibilidade, padronização e rastreabilidade entre documentação, código e histórico de desenvolvimento.

## Governança

### IX. Processo de Spec-Driven Development

O desenvolvimento DEVE seguir o fluxo:

1. **Constitution:** definição dos princípios e restrições obrigatórias.
2. **Specification:** descrição dos requisitos, histórias de usuário, critérios de aceite e limites de escopo, sem decisões de implementação.
3. **Clarification:** identificação e resolução de ambiguidades, com atualização da Specification.
4. **Plan:** definição das tecnologias, arquitetura, contratos, persistência e decisões técnicas justificadas.
5. **Tasks:** decomposição do planejamento em tarefas pequenas, ordenadas e verificáveis.
6. **Quality Gates:** análise cruzada dos documentos e validação de consistência antes da implementação.
7. **Implementation:** execução incremental das tarefas, com validação e commits rastreáveis.

### X. Rastreabilidade e consistência

- Todo requisito funcional DEVE possuir identificador único.
- Toda tarefa DEVE possuir origem identificável em um requisito ou decisão técnica documentada.
- Todo requisito aprovado DEVE estar contemplado por uma ou mais tarefas.
- Toda tarefa DEVE possuir critérios objetivos de conclusão (Definition of Done).
- Dependências entre tarefas DEVEM ser explicitadas.
- A IA NÃO DEVE assumir requisitos ou decisões ausentes dos documentos.
- Ambiguidades DEVEM ser esclarecidas antes da implementação.
- Alterações de escopo DEVEM atualizar os documentos afetados antes de modificar o código.

### XI. Portões de qualidade

Antes da implementação, DEVE ser realizada uma análise de consistência entre Constitution, Specification, Plan e Tasks.

A análise DEVE verificar:

- Contradições entre os documentos.
- Requisitos sem tarefas correspondentes.
- Tarefas sem origem documentada.
- Critérios de aceite ausentes ou não verificáveis.
- Violações dos princípios arquiteturais.
- Riscos de segurança e integridade de dados não tratados.
- Dependências e decisões técnicas sem justificativa.

Inconsistências críticas DEVEM ser resolvidas antes de iniciar a implementação.

### XII. Execução e validação

- A implementação DEVE ocorrer uma tarefa por vez, respeitando a ordem e as dependências definidas.
- A IA NÃO DEVE realizar alterações fora do escopo da tarefa em execução.
- Cada tarefa DEVE ser validada conforme sua Definition of Done.
- A validação DEVE incluir os testes e verificações de qualidade aplicáveis.
- Uma tarefa com falha DEVE ser corrigida e revalidada antes de avançar.
- A conclusão de cada tarefa DEVE registrar os requisitos atendidos, os arquivos modificados e os resultados das verificações.
- A implementação final DEVE ser conferida contra os critérios de aceite da Specification.

### XIII. Alterações da Constituição

- Esta Constituição possui precedência sobre Specification, Plan, Tasks e decisões de implementação.
- Alterações DEVEM ser justificadas e registradas antes de entrarem em vigor.
- Toda alteração DEVE incluir uma revisão de impacto sobre os documentos existentes.
- O versionamento DEVE seguir Semantic Versioning:
  - **MAJOR:** remoção ou alteração incompatível de princípios existentes.
  - **MINOR:** inclusão de princípios ou ampliação significativa de regras.
  - **PATCH:** correções de redação ou esclarecimentos sem mudança de significado.
- Toda alteração DEVE atualizar a versão e a data da última modificação.
- O Plan DEVE incluir uma verificação explícita de conformidade com esta Constituição.
- Violações não justificadas DEVEM bloquear o avanço para a implementação.

**Justificativa:** manter o processo auditável, impedir decisões arbitrárias e assegurar que o código implementado corresponda ao que foi especificado e planejado.

---

**Versão:** 1.0.0 | **Ratificada:** 2026-10-16 | **Última alteração:** 2026-10-16