# Implementation Plan: Fonte única de rede e barra de status

**Branch**: `002-network-source-status-bar` | **Date**: 2026-10-14 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-network-source-status-bar/spec.md`

## Summary

Duas mudanças sobre a aplicação da feature 001:

1. **Rede**: a coleta passa a usar só `MSFT_NetAdapter` + `MSFT_NetIPAddress` (WMI,
   `root\StandardCimv2`). O coletor complementar atual vira o único `NetworkInterfaceCollector`.
   Saem a consulta `Win32_NetworkAdapterConfiguration`, a associação `GetRelated` e o
   `NetworkInterfaceMerger`. A coleta de rede cai de ~670 ms para ~140 ms.
2. **Barra de status**: o `MainViewModel` ganha `StatusKind` (informação/aviso/erro) ao lado do
   `StatusMessage` existente. A janela principal troca o texto de status por um `StatusBar` com
   ícone, cor e texto cortado com reticências. Mensagens simples vão só para a barra; mensagens com
   detalhes abrem diálogo e deixam um resumo na barra. `IDialogService.ShowInformation` é removido.

Decisões em [research.md](research.md).

## Technical Context

**Language/Version**: C# 7.3 / .NET Framework 4.8 (inalterado)

**Primary Dependencies**: WPF, `System.Management`, Newtonsoft.Json 13.0.3 (inalterados; nenhuma nova)

**Storage**: arquivo JSON em `%LocalAppData%` (formato inalterado)

**Testing**: xUnit 2.9.x (inalterado)

**Target Platform**: Windows 10+ / .NET Framework 4.8, AnyCPU

**Project Type**: desktop-app (WPF)

**Performance Goals**: coleta de rede ≤ 50% do tempo anterior (SC-002); medido ~140 ms

**Constraints**: build sem warnings; coleta continua via WMI; sem bibliotecas novas; fonte de
ícones disponível em todo Windows 10+ (Segoe MDL2 Assets)

**Scale/Scope**: ~10 arquivos alterados, 2 removidos, 1 novo (`StatusKind.cs`)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Como o plano atende | Status |
|---|---|---|
| I. Clareza e Convenções C# | Enum novo em arquivo próprio; constantes para WQL, ícones e textos de status; XML docs; `using` nos objetos WMI (inalterado) | ✅ |
| II. Fidelidade aos Requisitos | Rede continua via WMI com nome, IP e MAC; botão "Exibir dados" e arquivo inalterados; regras de habilitação preservadas (FR-012) | ✅ |
| III. Separação de Responsabilidades | Destino das mensagens decidido no ViewModel; View só faz binding (ícone/cor por `DataTrigger`); coletor sem lógica de UI | ✅ |
| IV. Robustez | Falha de rede continua como aviso isolado (`TryCollect`); erro inesperado continua em diálogo; textos longos não quebram o layout | ✅ |
| V. Simplicidade | Remove 2 classes (`NetworkInterfaceMerger` e a fonte `Win32` de rede), 1 método de interface e a lógica de mesclagem; acrescenta só 1 enum | ✅ |
| Restrições Técnicas | .NET Framework 4.8, AnyCPU, sem dependências novas | ✅ |
| Fluxo de Entrega | README e docs atualizados (FR-013); testes ajustados; quickstart com cenários de rede e barra | ✅ |

**Re-check pós-design (Phase 1)**: data model, contrato de mensagens e quickstart mantêm todos os
itens. Nenhuma violação; Complexity Tracking não se aplica.

### Rastreabilidade FR → componente

| FR | Componente |
|---|---|
| FR-001, FR-002, FR-003 | `NetworkInterfaceCollector` (MSFT) + `WmiQueries` |
| FR-004 | Remoção de `NetworkInterfaceMerger` e de `SystemInfoService.CollectNetworkInterfaces` |
| FR-005 | `SystemInfoService.TryCollect` (inalterado) |
| FR-006, FR-011 | `StatusKind`, `MainViewModel.StatusKind/StatusMessage`, `StatusBar` em `MainWindow.xaml` |
| FR-007, FR-008, FR-010 | `MainViewModel` (destino fixo por evento, conforme [contrato](contracts/ui-status-bar.md)) |
| FR-009 | `App.OnDispatcherUnhandledException` (inalterado) |
| FR-012 | Testes existentes de repositório, `DataViewModel` e habilitação de botões |
| FR-013 | `README.md`, docs da feature 001 (nota de substituição) |

## Project Structure

### Documentation (this feature)

```text
specs/002-network-source-status-bar/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-status-bar.md
└── tasks.md             # /speckit-tasks
```

### Source Code (alterações sobre a feature 001)

```text
src/SystemInfoCollector/
├── App.xaml.cs                                   # ALTERA: composição sem o coletor Win32 de rede
├── Services/
│   ├── IDialogService.cs                         # ALTERA: remove ShowInformation
│   ├── DialogService.cs                          # ALTERA: remove ShowInformation
│   ├── SystemInfoService.cs                      # ALTERA: volta a 4 coletores, rede com um TryCollect
│   └── Collectors/
│       ├── WmiQueries.cs                         # ALTERA: remove consultas Win32 de rede
│       ├── NetworkInterfaceCollector.cs          # SUBSTITUI: conteúdo do NetAdapterCollector (MSFT)
│       ├── NetAdapterCollector.cs                # REMOVE (renomeado para NetworkInterfaceCollector)
│       └── NetworkInterfaceMerger.cs             # REMOVE
├── ViewModels/
│   ├── StatusKind.cs                             # NOVO
│   └── MainViewModel.cs                          # ALTERA: StatusKind, SetStatus, destino das mensagens
└── Views/
    └── MainWindow.xaml                           # ALTERA: DockPanel + StatusBar

tests/SystemInfoCollector.Tests/
├── Fakes/FakeDialogService.cs                    # ALTERA: remove ShowInformation
├── Fakes/FakeSystemInfoService.cs                # ALTERA: callback durante a coleta
├── MainViewModelTests.cs                         # ALTERA: barra x diálogo
├── NetworkInterfaceCollectorTests.cs             # NOVO (testes de formatação movidos)
└── NetworkInterfaceMergerTests.cs                # REMOVE

README.md                                         # ALTERA
specs/001-system-info-collector/research.md       # ALTERA: nota "substituído pela feature 002"
```

**Structure Decision**: mesma estrutura de projeto da feature 001; mudanças localizadas.

## Complexity Tracking

Não se aplica: a feature reduz a complexidade (remove a mesclagem de fontes e um método de
interface) e não introduz violações.
