---

description: "Task list for feature implementation"
---

# Tasks: Fonte única de rede e barra de status

**Input**: Design documents from `specs/002-network-source-status-bar/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ui-status-bar.md, quickstart.md

**Tests**: incluídos para a lógica sem UI, como na feature 001 (constituição, Fluxo de
Desenvolvimento). A coleta WMI real é validada pelo [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Convenções obrigatórias (constituição, Princípio I)

Iguais às da feature 001: C# 7.3 / net48; uma classe por arquivo; namespace = pasta; nomenclatura
Microsoft; XML docs em membros públicos; `using` em todo `IDisposable`; textos e strings WMI como
constantes nomeadas; nenhum `catch (Exception)` fora de `App.xaml.cs`; textos, comentários, nomes e
commits descrevem apenas o produto; build sem warnings.

---

## Phase 1: Setup

**Purpose**: confirmar a base antes das alterações

- [x] T001 Na branch `002-network-source-status-bar`, executar `dotnet build SystemInfoCollector.sln -c Release` e `dotnet test SystemInfoCollector.sln -c Release` e confirmar 0 warnings e 52/52 testes passando (linha de base)

---

## Phase 2: Foundational

Nenhuma tarefa: as duas mudanças são independentes e reaproveitam a infraestrutura existente.

---

## Phase 3: User Story 1 - Interfaces de rede iguais às que o Windows mostra como ativas (Priority: P1) 🎯 MVP

**Goal**: coleta de rede com uma única fonte WMI (`MSFT_NetAdapter` + `MSFT_NetIPAddress`), sem
mesclagem.

**Independent Test**: coletar e comparar a seção de rede com `Get-NetAdapter | Where-Object Status -eq Up`
(quickstart R1–R5).

### Tests for User Story 1

- [ ] T002 [P] [US1] Criar `tests/SystemInfoCollector.Tests/NetworkInterfaceCollectorTests.cs` com os testes de formatação movidos de `NetworkInterfaceMergerTests.cs`, agora referenciando `NetworkInterfaceCollector`: `FormatMacAddress` (`"00155D000001"` → `"00:15:5D:00:00:01"`; `null` → `null`; `"ABC"` → `"ABC"`) e `RemoveZoneIndex` (`"fe80::aaaa:bbbb:cccc:dddd%40"` → `"fe80::aaaa:bbbb:cccc:dddd"`; `"172.16.0.1"` inalterado). Apagar `tests/SystemInfoCollector.Tests/NetworkInterfaceMergerTests.cs`

### Implementation for User Story 1

- [ ] T003 [US1] Substituir o conteúdo de `src/SystemInfoCollector/Services/Collectors/NetworkInterfaceCollector.cs` pelo de `NetAdapterCollector.cs`, renomeando a classe para `NetworkInterfaceCollector` e reescrevendo o XML doc da classe para "Coleta as interfaces de rede conectadas pelas classes WMI `MSFT_NetAdapter` e `MSFT_NetIPAddress` (namespace `root\StandardCimv2`), disponíveis em todo Windows 10 ou superior" (sem mencionar fonte complementar). Manter as regras do data-model: "Somente adaptadores com `MediaConnectState = 1` (conectados)", "Somente adaptadores com ao menos um IP (junção por `InterfaceIndex`)", "`IpAddresses` nunca é `null`", "Sem deduplicação". Apagar `src/SystemInfoCollector/Services/Collectors/NetAdapterCollector.cs`
- [ ] T004 [P] [US1] Em `src/SystemInfoCollector/Services/Collectors/WmiQueries.cs`, remover as constantes `NetworkAdapterConfiguration` e `NetworkAdapterClassName` e ajustar o XML doc de `StandardCimNamespace` para "Namespace WMI das classes de rede `MSFT_*`" (sem "fonte complementar")
- [ ] T005 [P] [US1] Apagar `src/SystemInfoCollector/Services/Collectors/NetworkInterfaceMerger.cs`
- [ ] T006 [US1] Em `src/SystemInfoCollector/Services/SystemInfoService.cs`, remover o campo `_netAdapterCollector`, o parâmetro `NetAdapterCollector netAdapterCollector` do construtor e o método `CollectNetworkInterfaces`; preencher `NetworkInterfaces = TryCollect(_networkInterfaceCollector.Collect, "interfaces de rede", warnings, new List<NetworkInterfaceInfo>())` (depende de T003, T005)
- [ ] T007 [US1] Em `src/SystemInfoCollector/App.xaml.cs`, remover `new NetAdapterCollector()` da composição do `SystemInfoService` (depende de T006)
- [ ] T008 [US1] Build + testes; depois executar a coleta real por script (carregando `SystemInfoCollector.exe` no PowerShell) e comparar com `Get-NetAdapter | Where-Object Status -eq Up`: mesmas interfaces com IP, sem duplicatas, MAC `XX:XX:XX:XX:XX:XX`, IPv6 sem `%n`; medir `NetworkInterfaceCollector.Collect()` e confirmar ≤ ~335 ms (SC-002)

**Checkpoint**: rede com fonte única, resultado igual ao Windows e mais rápido

---

## Phase 4: User Story 2 - Mensagens curtas na barra de status (Priority: P2)

**Goal**: barra de status na janela principal com tipo (ícone + cor) e mensagens simples sem
diálogo.

**Independent Test**: quickstart S1–S5.

### Tests for User Story 2

- [ ] T009 [US2] Em `tests/SystemInfoCollector.Tests/Fakes/FakeSystemInfoService.cs`, adicionar `public Action OnCollect { get; set; }` invocado no início de `Collect()`. Em `tests/SystemInfoCollector.Tests/MainViewModelTests.cs`, ajustar/acrescentar: (a) na construção, `StatusMessage == "Pronto."` e `StatusKind == StatusKind.Information`; (b) durante a coleta (capturado via `OnCollect`), `StatusMessage == "Coletando dados..."` e `StatusKind.Information`; (c) coleta sem avisos: nenhum diálogo (listas `Warnings` e `Errors` do dublê vazias), `StatusKind.Information`, `StatusMessage` contém o `FilePath`; (d) exibir com arquivo apagado: nenhum diálogo, `StatusKind.Error`, `StatusMessage == "O arquivo de dados não existe mais. Colete novamente."`, `CanShowData == false`; (e) exibir com arquivo corrompido: nenhum diálogo, `StatusKind.Error`, `StatusMessage == "O arquivo de dados está corrompido. Colete novamente."`, `CanShowData == true`. Substituir os testes antigos que esperavam `ShowInformation`/`ShowError` nesses cenários

### Implementation for User Story 2

- [ ] T010 [P] [US2] Criar `src/SystemInfoCollector/ViewModels/StatusKind.cs`: `public enum StatusKind { Information, Warning, Error }` com XML doc no enum e em cada valor
- [ ] T011 [US2] Em `src/SystemInfoCollector/ViewModels/MainViewModel.cs`: adicionar propriedade `StatusKind StatusKind` (setter privado, notifica alteração); método privado `SetStatus(StatusKind kind, string message)` que altera ambos; constantes `ReadyMessage = "Pronto."` e `CollectingMessage = "Coletando dados..."`; no construtor `SetStatus(StatusKind.Information, ReadyMessage)`; no início da coleta `SetStatus(Information, CollectingMessage)`; coleta sem avisos → apenas `SetStatus(Information, $"Dados salvos em {FilePath}")` (remover a chamada a `ShowInformation`); `FileNotFoundException` e `InvalidDataException` ao exibir → apenas `SetStatus(Error, <mensagem do contrato>)`, sem diálogo. Todos os textos conforme [contracts/ui-status-bar.md](contracts/ui-status-bar.md) (depende de T010)
- [ ] T012 [US2] Em `src/SystemInfoCollector/Views/MainWindow.xaml`: trocar o `StackPanel` raiz por `DockPanel`; remover o `TextBlock` de status; adicionar `StatusBar` com `DockPanel.Dock="Bottom"` contendo um `StatusBarItem` com `DockPanel` interno: `TextBlock` de ícone (`FontFamily="Segoe MDL2 Assets"`, glifos `&#xE946;` informação, `&#xE7BA;` aviso, `&#xEA39;` erro) e `TextBlock` com `Text="{Binding StatusMessage}"`, `TextTrimming="CharacterEllipsis"` e `ToolTip="{Binding StatusMessage}"`; ícone e cor por `DataTrigger` em `StatusKind` (informação = cor padrão; aviso = `#9D5D00`; erro = `#C42B1C`); o conteúdo atual (nome, botões, progresso) fica no corpo do `DockPanel` com margem 16 (depende de T011)
- [ ] T013 [US2] Build + testes; instanciar `MainWindow` por script para validar o XAML

**Checkpoint**: barra funcional; mensagens simples sem diálogo

---

## Phase 5: User Story 3 - Diálogo apenas para mensagens detalhadas (Priority: P2)

**Goal**: mensagens com detalhes continuam em diálogo e deixam um resumo na barra; remover
`ShowInformation`.

**Independent Test**: quickstart S6–S7 e testes automatizados.

### Tests for User Story 3

- [ ] T014 [US3] Em `tests/SystemInfoCollector.Tests/MainViewModelTests.cs`: (a) coleta com 1 aviso → `ShowWarning` chamado com a lista e barra `StatusKind.Warning` + `"Coleta concluída com 1 aviso"`; (b) com 2 avisos → `"Coleta concluída com 2 avisos"`; (c) falha ao salvar (`IOException`) → `ShowError` com motivo e caminho e barra `StatusKind.Error` + `"Não foi possível salvar os dados."`; (d) falha ao ler (`Load` lançando `IOException` genérica) → `ShowError` com motivo e caminho e barra `StatusKind.Error` + `"Não foi possível ler os dados."` (depende de T011)

### Implementation for User Story 3

- [ ] T015 [US3] Em `src/SystemInfoCollector/ViewModels/MainViewModel.cs`: coleta com avisos → manter `ShowWarning` com a lista e `SetStatus(Warning, FormatWarningSummary(count))` ("Coleta concluída com 1 aviso" / "Coleta concluída com N avisos"); `ReportSaveFailure` → `SetStatus(Error, "Não foi possível salvar os dados.")` + `ShowError` existente; `ReportReadFailure` → `SetStatus(Error, "Não foi possível ler os dados.")` + `ShowError` existente; textos como constantes (depende de T014)
- [ ] T016 [US3] Remover `ShowInformation` de `src/SystemInfoCollector/Services/IDialogService.cs`, `src/SystemInfoCollector/Services/DialogService.cs` e `tests/SystemInfoCollector.Tests/Fakes/FakeDialogService.cs` (com a lista `Informations`), conforme [contracts/ui-status-bar.md](contracts/ui-status-bar.md#idialogservice-após-a-feature) (depende de T015)
- [ ] T017 [US3] Build + testes (0 warnings, 100% passando)

**Checkpoint**: todas as histórias funcionais

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T018 [P] Atualizar `README.md`: na tabela "Dados coletados", linha de rede com fonte `MSFT_NetAdapter` + `MSFT_NetIPAddress` (WMI, `root\StandardCimv2`); substituir a seção "Decisão de projeto: duas fontes WMI..." por "Decisão de projeto: interfaces de rede via MSFT_NetAdapter" explicando por que essa classe WMI foi escolhida em vez de `Win32_NetworkAdapterConfiguration` (lista adaptadores virtuais ativos, ~4x mais rápida, disponível em todo Windows 10+), com link para `specs/002-network-source-status-bar/research.md#r1-fonte-única-de-interfaces-de-rede`; em "Funcionalidades", descrever a barra de status (mensagens curtas com ícone/cor; diálogo só para mensagens com detalhes); atualizar o exemplo de saída se necessário; na estrutura de pastas, mencionar as duas features em `specs/`
- [ ] T019 [P] Em `specs/001-system-info-collector/research.md` (seção R4.1) e `specs/001-system-info-collector/contracts/ui-contract.md` (seção "Mensagens"), acrescentar nota: "Substituído pela feature 002 — ver `specs/002-network-source-status-bar/`"
- [ ] T020 Revisar os arquivos alterados contra a constituição (convenções, constantes, XML docs, ausência de referências a `NetAdapterCollector`/`NetworkInterfaceMerger`/`ShowInformation` em `src/` e `tests/`) e executar build Release + testes finais
- [ ] T021 Validação manual pelo usuário: cenários R1–R5 e S1–S8 de [quickstart.md](quickstart.md)
- [ ] T022 Após confirmação do usuário: merge da branch `002-network-source-status-bar` na `main` e push das duas branches para o GitHub (depende de T021)

---

## Dependencies & Execution Order

- **Setup (T001)** → base para todas as fases
- **US1 (Phase 3)**: independente de US2/US3
- **US2 (Phase 4)**: independente de US1
- **US3 (Phase 5)**: depende de US2 (usa `StatusKind` e `SetStatus`)
- **Polish (Phase 6)**: depende de todas as histórias; T021 e T022 dependem do usuário

### Parallel Opportunities

- US1 e US2 podem ser feitas em paralelo (arquivos distintos)
- Em US1: T002, T004 e T005 em paralelo
- Em US2: T010 em paralelo com T009
- Polish: T018 e T019 em paralelo

---

## Parallel Example: User Story 1

```text
Task: "NetworkInterfaceCollectorTests em tests/SystemInfoCollector.Tests/NetworkInterfaceCollectorTests.cs"
Task: "Remover consultas Win32 de rede em src/SystemInfoCollector/Services/Collectors/WmiQueries.cs"
Task: "Apagar src/SystemInfoCollector/Services/Collectors/NetworkInterfaceMerger.cs"
```

---

## Implementation Strategy

1. T001 (linha de base)
2. US1 → validar rede contra o Windows → commit
3. US2 → validar barra → commit
4. US3 → validar diálogos e resumos → commit
5. Polish → README/docs, revisão, build/testes → commit
6. Validação manual do usuário → merge e push

## Notes

- Commit por fase concluída, mensagens em português descrevendo o produto
- [P] = arquivos diferentes, sem dependências pendentes
