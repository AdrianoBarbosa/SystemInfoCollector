# Implementation Plan: Coletor de Informações do Sistema

**Branch**: `001-system-info-collector` | **Date**: 2026-10-14 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-system-info-collector/spec.md`

## Summary

Aplicação desktop WPF (.NET Framework 4.8) que exibe o nome do autor, coleta dados do computador
local via WMI (sistema operacional, processadores, interfaces de rede) e via registro (versão do
.NET Framework), grava o resultado em JSON indentado em `%LocalAppData%` e exibe os dados
desserializados em uma janela formatada. O botão "Exibir dados" acompanha a existência do
arquivo. Arquitetura MVVM leve, com serviços de coleta e persistência atrás de interfaces e
coleta assíncrona para manter a UI responsiva. Detalhes das decisões em [research.md](research.md).

## Technical Context

**Language/Version**: C# 7.3 (padrão do .NET Framework 4.8)

**Primary Dependencies**: WPF, `System.Management` (WMI), `Microsoft.Win32` (registro),
Newtonsoft.Json 13.0.3

**Storage**: arquivo JSON local em `%LocalAppData%\SystemInfoCollector\system-info.json`

**Testing**: xUnit 2.9.x + `xunit.runner.visualstudio` + `Microsoft.NET.Test.Sdk`

**Target Platform**: Windows 10 1903+ / Windows 11 com .NET Framework 4.8; AnyCPU, `Prefer32Bit=false`

**Project Type**: desktop-app (WPF)

**Performance Goals**: coleta completa ≤ 5 s (SC-001); UI sem travamentos durante a coleta

**Constraints**: build sem warnings; nenhuma exceção não tratada encerra o app (SC-005);
sem privilégios de administrador

**Scale/Scope**: 2 janelas, ~20 classes, 1 arquivo de dados

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Como o plano atende | Status |
|---|---|---|
| I. Clareza e Convenções C# | Convenções Microsoft, uma classe por arquivo, namespaces = pastas, constantes para WQL/chaves/caminhos (`WmiQueries`, `RegistryPaths`, `AppPaths`), `using` em todo `IDisposable`, XML docs em membros públicos, `TreatWarningsAsErrors=true` | ✅ |
| II. Fidelidade aos Requisitos | Todos os FR mapeados para componentes (tabela abaixo); opcionais (WPF, rede, JSON) somam sem substituir obrigatórios; `CanShowData` reavaliado na abertura | ✅ |
| III. Separação de Responsabilidades | Coletores (`Services/Collectors`), persistência (`SystemInfoRepository`), apresentação (ViewModels + Views); code-behind vazio; modelos tipados; serviços por interface | ✅ |
| IV. Robustez | Coleta parcial com `Warnings`; exceções específicas tratadas; null → "Não disponível"; registro com view 64/32 bits correta; coleta em `Task.Run`; gravação atômica | ✅ |
| V. Simplicidade | Um projeto de app + um de testes; MVVM com 3 classes de infraestrutura, sem framework; única dependência externa: Newtonsoft.Json | ✅ (ver Complexity Tracking) |
| Restrições Técnicas | .NET Framework 4.8 ≥ 4.5.2; AnyCPU; UTF-8; caminho em um único ponto | ✅ |
| Fluxo de Entrega | `.gitignore` VS (ignora `.claude/`), `README.md`, testes de lógica pura, quickstart com verificação manual, publicação no GitHub com `specs/` e `.specify/` versionados | ✅ |

**Re-check pós-design (Phase 1)**: data model, contratos e quickstart mantêm todos os itens
acima. Nenhuma violação nova.

### Rastreabilidade FR → componente

| FR | Componente |
|---|---|
| FR-001 | `MainWindow.xaml` (TextBlock do autor) |
| FR-002, FR-012 | `MainViewModel.CollectDataCommand` (`AsyncRelayCommand`), `IsBusy` |
| FR-003 | `OperatingSystemCollector` |
| FR-004 | `ProcessorCollector` |
| FR-005 | `NetFrameworkCollector` + `NetFrameworkVersionMapper` |
| FR-006 | `NetworkInterfaceCollector` (principal) + `NetAdapterCollector` (complementar) + `NetworkInterfaceMerger` |
| FR-007, FR-008 | `SystemInfoRepository` (JSON, gravação atômica) |
| FR-009, FR-013 | `MainViewModel` + `IDialogService` |
| FR-010 | `MainViewModel.CanShowData` / `ShowDataCommand.CanExecute` |
| FR-011 | `DataWindow` + `DataViewModel` |
| FR-014 | `WmiExtensions` (leitura segura) + `NullToNotAvailableConverter` |

## Project Structure

### Documentation (this feature)

```text
specs/001-system-info-collector/
├── plan.md              # Este arquivo
├── research.md          # Decisões técnicas (Phase 0)
├── data-model.md        # Modelos e estados (Phase 1)
├── quickstart.md        # Guia de build e validação (Phase 1)
├── contracts/
│   ├── system-info-file.md   # Formato do arquivo JSON
│   └── ui-contract.md        # Janelas, comandos e interfaces de serviço
└── tasks.md             # Phase 2 (/speckit-tasks)
```

### Source Code (repository root)

```text
SystemInfoCollector.sln
README.md
.gitignore
.editorconfig

src/SystemInfoCollector/
├── SystemInfoCollector.csproj        # net48, UseWPF, WinExe, AnyCPU
├── App.xaml / App.xaml.cs            # composição manual das dependências; handler global de exceções
├── AppPaths.cs                       # caminho do arquivo de dados
├── Models/
│   ├── SystemInfo.cs
│   ├── OperatingSystemInfo.cs
│   ├── ProcessorInfo.cs
│   ├── NetFrameworkInfo.cs
│   └── NetworkInterfaceInfo.cs
├── Services/
│   ├── ISystemInfoService.cs
│   ├── SystemInfoService.cs          # orquestra coletores, acumula Warnings
│   ├── ISystemInfoRepository.cs
│   ├── SystemInfoRepository.cs       # Save/Load/Exists em JSON
│   ├── IDialogService.cs
│   ├── DialogService.cs              # MessageBox + abre DataWindow
│   └── Collectors/
│       ├── WmiQueries.cs             # constantes WQL
│       ├── WmiExtensions.cs          # leitura segura de propriedades
│       ├── OperatingSystemCollector.cs
│       ├── ProcessorCollector.cs
│       ├── NetworkInterfaceCollector.cs  # Win32_NetworkAdapterConfiguration (principal)
│       ├── NetAdapterCollector.cs        # MSFT_NetAdapter + MSFT_NetIPAddress (complementar)
│       ├── NetworkInterfaceMerger.cs     # combina as duas fontes sem duplicatas
│       ├── NetFrameworkCollector.cs  # registro
│       └── NetFrameworkVersionMapper.cs
├── ViewModels/
│   ├── ViewModelBase.cs              # INotifyPropertyChanged
│   ├── RelayCommand.cs
│   ├── AsyncRelayCommand.cs
│   ├── MainViewModel.cs
│   └── DataViewModel.cs
├── Views/
│   ├── MainWindow.xaml(.cs)
│   └── DataWindow.xaml(.cs)
└── Converters/
    └── NullToNotAvailableConverter.cs

tests/SystemInfoCollector.Tests/
├── SystemInfoCollector.Tests.csproj  # net48, xUnit
├── Fakes/                            # dublês de ISystemInfoService, ISystemInfoRepository, IDialogService
├── NetFrameworkVersionMapperTests.cs
├── SystemInfoRepositoryTests.cs
├── MainViewModelTests.cs
└── DataViewModelTests.cs
```

**Structure Decision**: projeto único de aplicação com pastas por responsabilidade (Models,
Services, ViewModels, Views) mais um projeto de testes. A separação em várias bibliotecas
(Core/Infrastructure/UI) foi descartada por ser desproporcional ao escopo (Princípio V).

## Complexity Tracking

| Item | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| MVVM (ViewModelBase, RelayCommand, AsyncRelayCommand) | Padrão idiomático do WPF; mantém code-behind vazio e permite testar o estado dos botões | Lógica no code-behind violaria o Princípio III e não seria testável |
| Interfaces para serviços (`ISystemInfoService`, `ISystemInfoRepository`, `IDialogService`) | Permitem testar `MainViewModel` com dublês | Classes concretas acoplariam testes a WMI, disco e MessageBox |
| Newtonsoft.Json | Serialização JSON indentada com camelCase em uma linha | `DataContractJsonSerializer` exige atributos em todo modelo e gera datas ilegíveis |
| Fonte complementar de rede (`MSFT_NetAdapter`) | `Win32_*` não lista adaptadores virtuais ativos (ex.: Hyper-V), violando SC-003 | Usar só `Win32_*` omite interfaces visíveis no Windows; usar só `MSFT_*` se afasta da classe de referência (research R4.1) |
