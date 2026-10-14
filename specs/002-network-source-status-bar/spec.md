# Feature Specification: Fonte única de rede e barra de status

**Feature Branch**: `002-network-source-status-bar`

**Created**: 2026-10-14

**Status**: Draft

**Input**: User description: "Duas melhorias no Coletor de Informações do Sistema: (1) coleta de
interfaces de rede com uma única fonte WMI, usando apenas MSFT_NetAdapter + MSFT_NetIPAddress no
lugar da combinação atual com Win32_NetworkAdapterConfiguration; (2) barra de status na janela
principal para mensagens curtas de informação, aviso e erro, mantendo caixas de diálogo apenas para
mensagens maiores ou com detalhes."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Interfaces de rede iguais às que o Windows mostra como ativas (Priority: P1)

O usuário coleta os dados e vê, na janela de dados e no arquivo, exatamente as interfaces de rede
que o Windows apresenta como ativas — incluindo adaptadores virtuais — com nome da conexão,
descrição do adaptador, endereços IP e endereço MAC, obtidos de uma única fonte.

**Why this priority**: é a mudança de comportamento com impacto direto na fidelidade dos dados e
na simplicidade da solução; a barra de status é uma melhoria de usabilidade independente.

**Independent Test**: coletar os dados em um computador com ao menos uma interface física ativa e
uma virtual ativa (ex.: comutador padrão do Hyper-V) e comparar a lista exibida com as interfaces
ativas listadas pelo Windows.

**Acceptance Scenarios**:

1. **Given** um computador com interfaces ativas físicas e virtuais, **When** o usuário coleta os
   dados, **Then** todas as interfaces ativas com IP aparecem uma única vez, sem duplicatas.
2. **Given** uma interface com IPv4 e IPv6 link-local, **When** os dados são exibidos, **Then**
   todos os endereços aparecem sem o sufixo de zona (ex.: `fe80::1`, não `fe80::1%17`).
3. **Given** uma interface com endereço físico, **When** os dados são exibidos, **Then** o MAC
   aparece no formato `XX:XX:XX:XX:XX:XX`; sem endereço físico (ex.: túnel VPN), aparece
   "Não disponível".
4. **Given** adaptadores desconectados ou sem IP, **When** o usuário coleta os dados, **Then**
   eles não aparecem na lista.
5. **Given** que a consulta de rede falhe, **When** o usuário coleta os dados, **Then** as demais
   informações são coletadas normalmente e a falha é informada como aviso.

---

### User Story 2 - Mensagens curtas na barra de status (Priority: P2)

O usuário acompanha o resultado das ações simples por uma pequena barra na parte inferior da janela
principal, sem precisar fechar caixas de diálogo com um único botão "OK". Cada mensagem indica
visualmente seu tipo (informação, aviso ou erro).

**Why this priority**: torna o uso mais fluido para as mensagens frequentes, sem alterar os dados.

**Independent Test**: executar uma coleta bem-sucedida, apagar o arquivo e tentar exibir, e
corromper o arquivo e tentar exibir; em todos os casos a mensagem deve aparecer na barra, com o tipo
indicado, sem abrir caixa de diálogo.

**Acceptance Scenarios**:

1. **Given** a janela principal aberta, **When** a coleta está em andamento, **Then** a barra
   exibe uma mensagem informativa de coleta em andamento.
2. **Given** uma coleta concluída sem avisos, **When** os dados são salvos, **Then** a barra exibe
   uma mensagem informativa com o caminho do arquivo e nenhuma caixa de diálogo é aberta.
3. **Given** que o arquivo de dados foi apagado, **When** o usuário clica em "Exibir dados",
   **Then** a barra exibe um erro curto informando que o arquivo não existe mais e nenhuma caixa
   de diálogo é aberta.
4. **Given** que o arquivo de dados está corrompido, **When** o usuário clica em "Exibir dados",
   **Then** a barra exibe um erro curto informando que o arquivo está corrompido e nenhuma caixa de
   diálogo é aberta.
5. **Given** uma mensagem na barra, **When** uma nova mensagem é gerada, **Then** a nova substitui
   a anterior, inclusive o indicador de tipo.

---

### User Story 3 - Diálogo apenas para mensagens detalhadas (Priority: P2)

Mensagens longas ou com detalhes técnicos continuam aparecendo em caixa de diálogo, onde há espaço
para o conteúdo completo, e a barra de status mostra um resumo correspondente.

**Why this priority**: complementa a US2, garantindo que informações importantes não sejam
truncadas na barra.

**Independent Test**: provocar uma coleta com aviso (ex.: fonte indisponível) e uma falha ao salvar
(ex.: pasta sem permissão de escrita) e verificar que cada uma abre caixa de diálogo com os
detalhes.

**Acceptance Scenarios**:

1. **Given** uma coleta concluída com avisos, **When** os dados são salvos, **Then** uma caixa de
   diálogo lista os avisos e a barra exibe um resumo do tipo aviso (ex.: "Coleta concluída com 1
   aviso").
2. **Given** uma falha ao salvar ou ler o arquivo por motivo técnico (permissão, disco), **When** a
   falha ocorre, **Then** uma caixa de diálogo exibe o motivo e a barra exibe um erro curto.
3. **Given** um erro inesperado, **When** ele ocorre, **Then** uma caixa de diálogo exibe a
   mensagem e a aplicação continua aberta.

---

### Edge Cases

- **Nenhuma interface ativa com IP**: a seção de rede informa "Nenhuma interface encontrada".
- **Interface virtual sem endereço físico** (ex.: túnel VPN): MAC exibido como "Não disponível".
- **Duas interfaces com a mesma descrição de adaptador** (ex.: dois adaptadores virtuais iguais):
  ambas aparecem, pois cada uma é uma interface distinta do Windows.
- **Mensagem longa na barra** (ex.: caminho extenso do arquivo): o texto não quebra o layout; a
  parte excedente é cortada com reticências e o texto completo fica disponível ao passar o mouse.
- **Mensagens em sequência rápida** (ex.: "coletando" seguido de "concluída"): a barra sempre
  reflete a última.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A coleta de interfaces de rede MUST usar uma única fonte de dados do Windows, que
  liste todas as interfaces que o próprio Windows apresenta como ativas, inclusive adaptadores
  virtuais.
- **FR-002**: Devem ser listadas somente interfaces conectadas com ao menos um endereço IP.
- **FR-003**: Para cada interface, o sistema MUST registrar o nome da conexão (ex.: "Wi-Fi"), a
  descrição do adaptador, todos os endereços IP (IPv4 e IPv6, sem sufixo de zona) e o endereço MAC
  no formato `XX:XX:XX:XX:XX:XX`.
- **FR-004**: A mesclagem de fontes e a eliminação de duplicatas MUST ser removidas, por não serem
  mais necessárias com uma única fonte.
- **FR-005**: Falha na coleta de rede MUST gerar aviso sem impedir a coleta das demais informações.
- **FR-006**: A janela principal MUST ter uma barra de status na parte inferior que exibe a última
  mensagem e indica visualmente seu tipo: informação, aviso ou erro.
- **FR-007**: As seguintes mensagens MUST aparecer somente na barra, sem caixa de diálogo: coleta em
  andamento (informação), coleta concluída com o caminho do arquivo (informação), arquivo de dados
  ausente (erro) e arquivo de dados corrompido (erro).
- **FR-008**: As seguintes mensagens MUST aparecer em caixa de diálogo, com um resumo na barra: coleta
  concluída com avisos (lista de avisos; resumo do tipo aviso), falha ao salvar ou ler o arquivo
  (motivo técnico; resumo do tipo erro).
- **FR-009**: Erros inesperados MUST continuar sendo exibidos em caixa de diálogo, sem encerrar a
  aplicação.
- **FR-010**: O destino de cada mensagem (barra ou diálogo) MUST ser fixo por tipo de mensagem, sem
  depender do tamanho do texto em tempo de execução.
- **FR-011**: Textos que não cabem na barra MUST ser cortados com reticências, com o texto completo
  disponível ao passar o mouse.
- **FR-012**: O formato do arquivo de dados, as regras de habilitação dos botões, a janela "Dados
  Coletados" e a coleta de sistema operacional, processador e .NET Framework MUST permanecer
  inalterados.
- **FR-013**: O README e a documentação do projeto MUST refletir a fonte única de rede e o novo
  comportamento das mensagens.

### Key Entities

- **Interface de Rede**: sem mudança de estrutura (nome, descrição, lista de IPs, MAC); muda apenas
  a origem dos dados.
- **Mensagem de status**: texto curto exibido na barra e seu tipo (informação, aviso ou erro).
  Existe somente em memória; não é gravada no arquivo.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das interfaces ativas com IP listadas pelo Windows aparecem na coleta, sem
  duplicatas, no computador de validação.
- **SC-002**: A coleta de interfaces de rede fica pelo menos 50% mais rápida que a versão anterior
  no mesmo computador (referência medida: ~670 ms com as duas fontes).
- **SC-003**: Uma coleta bem-sucedida sem avisos exige 1 clique (o próprio "Coletar dados"), sem
  nenhum clique adicional para dispensar mensagens.
- **SC-004**: Em 100% dos cenários de FR-007, nenhuma caixa de diálogo é aberta; em 100% dos
  cenários de FR-008 e FR-009, a caixa de diálogo é aberta com os detalhes.
- **SC-005**: Todos os testes automatizados existentes que não tratam da mesclagem de fontes
  continuam passando, e o arquivo gerado mantém o mesmo formato.

## Assumptions

- A fonte única de rede é composta pelas classes WMI `MSFT_NetAdapter` e `MSFT_NetIPAddress`
  (namespace `root\StandardCimv2`), disponíveis em qualquer Windows 10 ou superior; continua sendo
  coleta via WMI.
- "Interface ativa" corresponde ao que o Windows exibe com status conectado (ex.: `Get-NetAdapter`
  com status "Up").
- A barra exibe apenas a mensagem mais recente; não há histórico de mensagens nem expiração
  automática.
- O indicador de tipo usa cor e/ou ícone simples, sem dependência de bibliotecas de terceiros.
- O texto atual de status da janela principal é substituído pela barra de status.
- A janela "Dados Coletados" não recebe barra de status.
