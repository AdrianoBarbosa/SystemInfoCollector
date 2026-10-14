# Research: Fonte única de rede e barra de status

**Feature**: `002-network-source-status-bar` | **Date**: 2026-10-14

Decisões técnicas da feature. As decisões da feature 001 continuam válidas, exceto onde indicado.

## R1. Fonte única de interfaces de rede

- **Decision**: usar somente `MSFT_NetAdapter` + `MSFT_NetIPAddress` (namespace
  `root\StandardCimv2`) para as interfaces de rede, substituindo a combinação
  `Win32_NetworkAdapterConfiguration` + fonte complementar da feature 001 (research R3/R4/R4.1).
  - Adaptadores: `SELECT Name, InterfaceDescription, InterfaceIndex, PermanentAddress FROM
    MSFT_NetAdapter WHERE MediaConnectState = 1` (somente conectados).
  - Endereços: `SELECT InterfaceIndex, IPAddress FROM MSFT_NetIPAddress`, agrupados por
    `InterfaceIndex`; adaptadores sem IP são descartados.
  - `Name` já é o nome da conexão ("Wi-Fi", "Ethernet"); não há associação adicional.
- **Rationale**: medição no computador de validação — a fonte `MSFT_*` sozinha retornou todas as
  interfaces da fonte `Win32_*` (mesmos IPs e MACs) **mais** o adaptador virtual do Hyper-V, em
  ~140 ms, contra ~530 ms da `Win32_*` e ~670 ms das duas combinadas (redução de ~79%, atende
  SC-002). Continua sendo WMI via `System.Management`. As classes existem desde o Windows 8 /
  Server 2012, portanto em todo Windows 10+, e são legíveis por usuário comum (sem elevação).
  Elimina `NetworkInterfaceMerger`, a associação `GetRelated` e duas constantes WQL.
- **Alternatives considered**: manter as duas fontes (feature 001) — resultado igual, porém mais
  código e mais lento; manter só `Win32_*` — omite adaptadores virtuais ativos.

## R2. Normalização dos dados de rede

- **Decision**: manter as funções já existentes e testadas: `FormatMacAddress`
  (`00155D000001` → `00:15:5D:00:00:01`; valor nulo ou fora de 12 caracteres é mantido como está)
  e `RemoveZoneIndex` (`fe80::1%17` → `fe80::1`). MAC vazio (ex.: túneis VPN) resulta em `null`,
  exibido como "Não disponível".
- **Rationale**: preserva o formato do arquivo e da exibição (FR-003, FR-012).
- **Ordem dos IPs**: a ordem retornada pelo WMI é preservada (geralmente IPv6 antes de IPv4); não
  há requisito de ordenação.

## R3. Duplicatas

- **Decision**: não há deduplicação. Cada `MSFT_NetAdapter` corresponde a uma interface distinta do
  Windows (chave `InterfaceIndex`), portanto não há duplicatas na fonte única.
- **Rationale**: atende FR-004 e o caso de borda de adaptadores com a mesma descrição.

## R4. Modelo da barra de status

- **Decision**: o `MainViewModel` expõe `StatusMessage` (string) e `StatusKind` (enum
  `StatusKind { Information, Warning, Error }`), alterados juntos por um método privado
  `SetStatus(kind, message)`. A View exibe a barra com binding nessas duas propriedades.
- **Rationale**: o texto de status já existia no ViewModel; acrescentar o tipo é a menor mudança
  possível e mantém tudo testável sem interface gráfica (Princípio III). Não há serviço de
  notificação separado nem histórico (spec: só a mensagem mais recente).
- **Alternatives considered**: `IStatusService` injetado — abstração sem ganho para uma única
  janela (Princípio V); fila/histórico de mensagens — fora do escopo.

## R5. Destino de cada mensagem

- **Decision**: o destino é decidido no ponto em que a mensagem é gerada no `MainViewModel`
  (FR-010). Tabela completa em [contracts/ui-status-bar.md](contracts/ui-status-bar.md).
  - Somente barra: coleta em andamento, coleta concluída, arquivo ausente, arquivo corrompido.
  - Diálogo + resumo na barra: coleta com avisos, falha ao salvar, falha ao ler.
  - Somente diálogo: erro inesperado (handler global em `App`, inalterado — FR-009).
- **Consequência**: `IDialogService.ShowInformation` deixa de ser usado e é removido (YAGNI),
  junto com sua implementação e o dublê de teste.

## R6. Apresentação da barra

- **Decision**: controle `StatusBar` do WPF encaixado na parte inferior (`DockPanel.Dock="Bottom"`)
  da janela principal, com um `StatusBarItem` contendo:
  - ícone da fonte **Segoe MDL2 Assets** (presente em todo Windows 10+): informação ``,
    aviso ``, erro ``;
  - texto com `TextTrimming="CharacterEllipsis"` e `ToolTip` com o texto completo (FR-011).
  - Cor do ícone e do texto por `DataTrigger` em `StatusKind`: informação = cor padrão; aviso =
    `#9D5D00`; erro = `#C42B1C` (tons legíveis sobre fundo claro).
- **Rationale**: tipo indicado por ícone **e** cor (não depende só de cor); sem bibliotecas
  externas; textos longos não quebram o layout.
- **Estado inicial**: "Pronto." (informação), para a barra nunca aparecer vazia.
- **Alternatives considered**: `TextBlock` simples com fundo colorido — funciona, mas `StatusBar`
  é o controle semântico do WPF para isso; ícones em imagem — exigiria recursos extras.

## R7. Resumos na barra para mensagens em diálogo

- **Decision**: textos fixos e curtos:
  - coleta com avisos → "Coleta concluída com N aviso(s)" (singular/plural correto), tipo aviso;
  - falha ao salvar → "Não foi possível salvar os dados.", tipo erro;
  - falha ao ler (permissão/disco) → "Não foi possível ler os dados.", tipo erro.
- **Rationale**: mantém a barra coerente com o último evento sem duplicar o detalhe do diálogo.

## R8. Testes

- **Decision**: ajustar `MainViewModelTests` para verificar `StatusKind`/`StatusMessage` e a
  ausência/presença de diálogo em cada cenário de FR-007 e FR-008; incluir o cenário "coleta em
  andamento" capturando o status de dentro do dublê do serviço durante a coleta. Remover
  `NetworkInterfaceMergerTests` e mover os testes de `FormatMacAddress`/`RemoveZoneIndex` para
  `NetworkInterfaceCollectorTests`. A coleta WMI real continua validada pelo quickstart.
