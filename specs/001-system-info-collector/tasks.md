---

description: "Task list for feature implementation"
---

# Tasks: Coletor de Informações do Sistema

**Input**: Design documents from `specs/001-system-info-collector/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: incluídos para a lógica sem UI, conforme a constituição (Fluxo de Desenvolvimento) e
research R10. A coleta WMI real é validada manualmente pelo [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Convenções obrigatórias para TODAS as tarefas (constituição, Princípio I)

- C# 7.3 / .NET Framework 4.8. Uma classe por arquivo; namespace = `SystemInfoCollector` +
  pasta (ex.: `SystemInfoCollector.Services.Collectors`).
- `PascalCase` para tipos/métodos/propriedades/constantes, `camelCase` para locais/parâmetros,
  `_camelCase` para campos privados, interfaces com prefixo `I`. Identificadores em inglês;
  textos exibidos ao usuário em português.
- Comentário XML (`/// <summary>`) em todo tipo e membro público (o projeto gera arquivo de
  documentação e trata warnings como erro).
- Todo `IDisposable` (`ManagementObjectSearcher`, `ManagementObjectCollection`,
  `ManagementBaseObject`, `RegistryKey`, streams) dentro de `using`.
- Strings mágicas (WQL, chaves de registro, caminhos, textos repetidos) como constantes nomeadas.
- Nunca usar `catch (Exception)` genérico, exceto no handler global de `App`.
- Textos, comentários e nomes descrevem apenas o produto e seu comportamento.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solução, projetos e configuração de repositório

- [x] T001 Criar `src/SystemInfoCollector/SystemInfoCollector.csproj` (SDK-style): `<Project Sdk="Microsoft.NET.Sdk">`, `OutputType=WinExe`, `TargetFramework=net48`, `UseWPF=true`, `LangVersion=7.3`, `PlatformTarget=AnyCPU`, `Prefer32Bit=false`, `TreatWarningsAsErrors=true`, `GenerateDocumentationFile=true`, `RootNamespace=SystemInfoCollector`, `AssemblyTitle="Coletor de Informações do Sistema"`, `Product="System Info Collector"`, `Authors="Adriano Olivares Barbosa"`; `<Reference Include="System.Management" />`; `<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />`; `<InternalsVisibleTo Include="SystemInfoCollector.Tests" />`. Criar `App.xaml`/`App.xaml.cs` mínimos (classe `App : Application`, namespace `SystemInfoCollector`, sem `StartupUri`) para o projeto compilar.
- [x] T002 [P] Criar `tests/SystemInfoCollector.Tests/SystemInfoCollector.Tests.csproj` (SDK-style): `TargetFramework=net48`, `LangVersion=7.3`, `IsPackable=false`, `TreatWarningsAsErrors=true`; pacotes `Microsoft.NET.Test.Sdk` 17.11.1, `xunit` 2.9.2, `xunit.runner.visualstudio` 2.8.2; `<ProjectReference Include="..\..\src\SystemInfoCollector\SystemInfoCollector.csproj" />`
- [x] T003 Criar `SystemInfoCollector.sln` na raiz com `dotnet new sln -n SystemInfoCollector` e adicionar os dois projetos (`dotnet sln add`), com pastas de solução `src` e `tests`; validar com `dotnet build SystemInfoCollector.sln` (depende de T001, T002)
- [x] T004 [P] Criar `.gitignore` na raiz com o conteúdo do template padrão "VisualStudio" (inclui `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, `[Tt]est[Rr]esult*/`, `packages/`) e, ao final, uma seção com `.claude/` (configuração local do assistente, não publicada). `specs/` e `.specify/` SÃO versionados
- [x] T005 [P] Criar `.editorconfig` na raiz: `root = true`; `charset = utf-8`, `indent_style = space`, `indent_size = 4` para `*.cs`, `2` para `*.xaml`/`*.csproj`/`*.json`; `end_of_line = crlf`; `insert_final_newline = true`; regras `dotnet_naming_rule` para interfaces (`I` + PascalCase), tipos e membros públicos (PascalCase) e campos privados (`_camelCase`) com severidade `warning`; `csharp_style_var_when_type_is_apparent = true`; `dotnet_sort_system_directives_first = true`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Modelos, infraestrutura MVVM e utilitários usados por todas as histórias

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 [P] Criar `src/SystemInfoCollector/Models/OperatingSystemInfo.cs`: propriedades `string Caption`, `string Version`, `string Architecture`, `string ComputerName` (`get; set;`, podem ser `null` = "Não disponível")
- [x] T007 [P] Criar `src/SystemInfoCollector/Models/ProcessorInfo.cs`: `string Name`, `string Description`
- [x] T008 [P] Criar `src/SystemInfoCollector/Models/NetFrameworkInfo.cs`: `string Version`, `int? Release`
- [x] T009 [P] Criar `src/SystemInfoCollector/Models/NetworkInterfaceInfo.cs`: `string Name`, `string Description`, `List<string> IpAddresses` ("nunca é `null` (vazia se ausente)": inicializar com `new List<string>()` e, em método `[OnDeserialized]` privado, substituir `null` por lista vazia), `string MacAddress`
- [x] T010 Criar `src/SystemInfoCollector/Models/SystemInfo.cs`: `DateTimeOffset CollectedAt`, `OperatingSystemInfo OperatingSystem`, `List<ProcessorInfo> Processors`, `NetFrameworkInfo NetFramework`, `List<NetworkInterfaceInfo> NetworkInterfaces`, `List<string> Warnings`. Regra: "listas nunca são `null` (inicializadas vazias no construtor e após desserializar)" — inicializar objetos e listas no construtor e, em método `[OnDeserialized]` privado, substituir listas `null` por listas vazias **e** `OperatingSystem`/`NetFramework` `null` por `new OperatingSystemInfo()`/`new NetFrameworkInfo()`, garantindo que nenhum consumidor receba objeto aninhado nulo (constituição IV: nunca gerar `NullReferenceException`) (depende de T006–T009)
- [x] T011 [P] Criar `src/SystemInfoCollector/AppPaths.cs`: `internal static class AppPaths` com constantes `ApplicationFolderName = "SystemInfoCollector"`, `DataFileName = "system-info.json"` e propriedades estáticas `DataDirectory` (`Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationFolderName)`) e `DataFilePath` (`Path.Combine(DataDirectory, DataFileName)`)
- [x] T012 [P] Criar `src/SystemInfoCollector/ViewModels/ViewModelBase.cs`: classe abstrata implementando `INotifyPropertyChanged` com `protected void OnPropertyChanged([CallerMemberName] string propertyName = null)` e `protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)` (retorna `false` se `EqualityComparer<T>.Default.Equals`)
- [x] T013 [P] Criar `src/SystemInfoCollector/ViewModels/RelayCommand.cs`: `ICommand` com construtor `(Action execute, Func<bool> canExecute = null)`, validação `ArgumentNullException` para `execute`, e método público `RaiseCanExecuteChanged()` que dispara `CanExecuteChanged`
- [x] T014 [P] Criar `src/SystemInfoCollector/ViewModels/AsyncRelayCommand.cs`: `ICommand` com construtor `(Func<Task> execute, Func<bool> canExecute = null)`; propriedade `IsExecuting`; `CanExecute` retorna `false` enquanto `IsExecuting`; `async void Execute` faz `IsExecuting = true; RaiseCanExecuteChanged(); try { await _execute(); } finally { IsExecuting = false; RaiseCanExecuteChanged(); }`; método `public Task ExecuteAsync()` reutilizado por `Execute`; comentário XML deixando claro que o delegate é responsável por tratar suas exceções
- [x] T015 [P] Criar `src/SystemInfoCollector/Services/IDialogService.cs`: `void ShowInformation(string message)`, `void ShowWarning(string message)`, `void ShowError(string message)` (o método `ShowData` é adicionado em T034)
- [x] T016 [P] Criar `src/SystemInfoCollector/Converters/NullToNotAvailableConverter.cs`: `IValueConverter`; `public const string NotAvailableText = "Não disponível";`; `Convert` retorna `NotAvailableText` quando o valor é `null` ou string vazia/espaços, senão o próprio valor; `ConvertBack` lança `NotSupportedException`
- [x] T017 [P] Criar `src/SystemInfoCollector/Services/Collectors/WmiQueries.cs`: `internal static class` com as constantes exatas do research R3: `OperatingSystem = "SELECT Caption, Version, OSArchitecture, CSName FROM Win32_OperatingSystem"`, `Processor = "SELECT Name, Description FROM Win32_Processor"`, `NetworkAdapterConfiguration = "SELECT Index, Description, IPAddress, MACAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE"` e `NetworkAdapterClassName = "Win32_NetworkAdapter"`; comentário em `NetworkAdapterConfiguration` explicando que `Index` (propriedade-chave) precisa permanecer na projeção para que o WMI preencha `__PATH` e `GetRelated` funcione
- [x] T018 [P] Criar `src/SystemInfoCollector/Services/Collectors/WmiExtensions.cs`: `internal static class` com `string GetString(this ManagementBaseObject managementObject, string propertyName)` (retorna valor `Trim()`ado ou `null` se nulo/vazio) e `string[] GetStringArray(this ManagementBaseObject managementObject, string propertyName)` (retorna array vazio se nulo); ambos tratam `ManagementException` com `ErrorCode == ManagementStatus.NotFound` retornando `null`/vazio
- [x] T019 Atualizar `src/SystemInfoCollector/App.xaml.cs`: registrar handler de `DispatcherUnhandledException` que mostra `MessageBox` de erro ("Ocorreu um erro inesperado: {mensagem}") com título "Coletor de Informações do Sistema" e define `e.Handled = true` (rede de segurança para SC-005; único `catch` genérico permitido)

**Checkpoint**: `dotnet build` sem warnings — base pronta para as histórias

---

## Phase 3: User Story 1 - Coletar e salvar as informações do computador (Priority: P1) 🎯 MVP

**Goal**: tela principal com nome do autor e botão "Coletar dados" que coleta SO, processadores e
.NET Framework e grava o arquivo JSON, informando o caminho.

**Independent Test**: abrir o app, clicar "Coletar dados" e verificar que
`%LocalAppData%\SystemInfoCollector\system-info.json` foi criado com SO, processador(es) e .NET
(quickstart cenários 1–3, 5 e 6).

### Tests for User Story 1

- [x] T020 [P] [US1] Criar `tests/SystemInfoCollector.Tests/NetFrameworkVersionMapperTests.cs` (xUnit `[Theory]`): `null` → "Não detectado (anterior à 4.5)"; `378388` → "Não detectado (anterior à 4.5)"; `378389` → "4.5"; `378675` → "4.5.1"; `379893` → "4.5.2"; `393295` → "4.6"; `394254` → "4.6.1"; `394802` → "4.6.2"; `460798` → "4.7"; `461308` → "4.7.1"; `461808` → "4.7.2"; `528040` → "4.8"; `528449` → "4.8"; `533320` → "4.8.1 ou superior"; `533509` → "4.8.1 ou superior"
- [x] T021 [P] [US1] Criar dublês em `tests/SystemInfoCollector.Tests/Fakes/FakeSystemInfoService.cs` (retorna `SystemInfo` configurável ou lança exceção configurada), `tests/SystemInfoCollector.Tests/Fakes/FakeSystemInfoRepository.cs` (armazena em memória; `FileExists` configurável; `Save` pode lançar `IOException` configurada; conta chamadas) e `tests/SystemInfoCollector.Tests/Fakes/FakeDialogService.cs` (registra as mensagens recebidas por tipo em listas públicas)
- [x] T022 [US1] Criar `tests/SystemInfoCollector.Tests/MainViewModelTests.cs` com testes de coleta usando `await viewModel.CollectDataCommand.ExecuteAsync()`: (a) salva o resultado no repositório e `StatusMessage` contém o `FilePath`; (b) com `Warnings` não vazio chama `ShowWarning` listando os avisos; (c) sem avisos chama `ShowInformation`; (d) `Save` lançando `IOException` chama `ShowError` e não propaga; (e) `IsBusy` é `false` ao final e `CollectDataCommand.CanExecute(null)` volta a `true` (depende de T021)

### Implementation for User Story 1

- [x] T023 [P] [US1] Criar `src/SystemInfoCollector/Services/Collectors/NetFrameworkVersionMapper.cs`: `internal static class` com `public const string NotDetectedText = "Não detectado (anterior à 4.5)";` e tabela privada ordenada de forma decrescente de pares (release mínimo, versão) exatamente como research R5 (533320 → "4.8.1 ou superior", 528040 → "4.8", 461808 → "4.7.2", 461308 → "4.7.1", 460798 → "4.7", 394802 → "4.6.2", 394254 → "4.6.1", 393295 → "4.6", 379893 → "4.5.2", 378675 → "4.5.1", 378389 → "4.5"); método `string GetVersionName(int? release)` retorna a primeira versão cujo mínimo `<= release`, ou `NotDetectedText`
- [x] T024 [P] [US1] Criar `src/SystemInfoCollector/Services/Collectors/NetFrameworkCollector.cs`: constantes `NdpSubKeyPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"` e `ReleaseValueName = "Release"`; método `NetFrameworkInfo Collect()` abre `RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)` e a subchave com `using`, lê `Release` como `int?` (`null` se chave/valor ausente ou não-int) e retorna `new NetFrameworkInfo { Release = release, Version = NetFrameworkVersionMapper.GetVersionName(release) }`
- [x] T025 [P] [US1] Criar `src/SystemInfoCollector/Services/Collectors/OperatingSystemCollector.cs`: `OperatingSystemInfo Collect()` executa `WmiQueries.OperatingSystem` com `ManagementObjectSearcher` + `ManagementObjectCollection` em `using`, lê o primeiro objeto (também em `using`) mapeando `Caption`→`Caption`, `Version`→`Version`, `OSArchitecture`→`Architecture`, `CSName`→`ComputerName` via `GetString`; sem resultados retorna `OperatingSystemInfo` com campos `null`
- [x] T026 [P] [US1] Criar `src/SystemInfoCollector/Services/Collectors/ProcessorCollector.cs`: `List<ProcessorInfo> Collect()` executa `WmiQueries.Processor` e retorna um `ProcessorInfo` por objeto (`Name` e `Description` via `GetString`), todos os objetos WMI em `using`
- [x] T027 [US1] Criar `src/SystemInfoCollector/Services/ISystemInfoService.cs` (`SystemInfo Collect()`) e `src/SystemInfoCollector/Services/SystemInfoService.cs`: construtor recebe `OperatingSystemCollector`, `ProcessorCollector`, `NetFrameworkCollector` (validar `null` com `ArgumentNullException`); `Collect()` preenche `CollectedAt = DateTimeOffset.Now` e cada seção via método privado genérico `TryCollect<T>(Func<T> collect, string sectionName, List<string> warnings, T fallback)` que captura apenas `ManagementException`, `COMException`, `UnauthorizedAccessException` e `SecurityException`, adiciona aviso `$"Não foi possível coletar {sectionName}: {ex.Message}"` e retorna `fallback`; seções e fallbacks: "sistema operacional" → `new OperatingSystemInfo()`, "processador" → `new List<ProcessorInfo>()`, ".NET Framework" → `new NetFrameworkInfo()` (`Version = null`, `Release = null`, conforme data-model) (depende de T024–T026)
- [x] T028 [US1] Criar `src/SystemInfoCollector/Services/ISystemInfoRepository.cs` (`string FilePath { get; }`, `bool Exists()`, `void Save(SystemInfo systemInfo)`) e `src/SystemInfoCollector/Services/SystemInfoRepository.cs`: construtor `(string filePath)`; `JsonSerializerSettings` privado estático com `Formatting.Indented`, `ContractResolver = new CamelCasePropertyNamesContractResolver()`, `NullValueHandling.Include`, `DateFormatHandling.IsoDateFormat`, `MissingMemberHandling.Ignore`; `Save` cria o diretório, grava em `FilePath + ".tmp"` com `new UTF8Encoding(false)` e então usa `File.Replace(tmp, FilePath, null)` se o arquivo existir ou `File.Move(tmp, FilePath)` caso contrário; `Exists()` = `File.Exists(FilePath)`
- [x] T029 [US1] Criar `src/SystemInfoCollector/ViewModels/MainViewModel.cs`: construtor `(ISystemInfoService, ISystemInfoRepository, IDialogService)` com validação de `null`; propriedades `bool IsBusy`, `string StatusMessage`; `AsyncRelayCommand CollectDataCommand` com `CanExecute = !IsBusy` (`IsBusy` é o estado exposto à View e também bloqueia `ShowDataCommand`; `IsExecuting` do comando é apenas proteção interna contra reentrada — documentar isso em comentário); método privado `async Task CollectDataAsync()`: `IsBusy = true`; `try { var systemInfo = await Task.Run(() => _systemInfoService.Collect()); await Task.Run(() => _repository.Save(systemInfo)); StatusMessage = $"Dados salvos em {_repository.FilePath}"; se houver `Warnings` → `ShowWarning("Coleta concluída com avisos:" + lista)`, senão `ShowInformation($"Coleta concluída. Dados salvos em {_repository.FilePath}")` } catch (IOException/UnauthorizedAccessException ex) { ShowError($"Não foi possível salvar os dados em {_repository.FilePath}: {ex.Message}") } finally { IsBusy = false; }`
- [x] T030 [P] [US1] Criar `src/SystemInfoCollector/Services/DialogService.cs`: implementa `IDialogService` com `MessageBox.Show(Application.Current.MainWindow, message, Caption, MessageBoxButton.OK, MessageBoxImage.Information/Warning/Error)`, `private const string Caption = "Coletor de Informações do Sistema"`
- [x] T031 [US1] Criar `src/SystemInfoCollector/Views/MainWindow.xaml` e `MainWindow.xaml.cs` (namespace `SystemInfoCollector.Views`; code-behind apenas `InitializeComponent()`): `Title="Coletor de Informações do Sistema"`, `Width=480`, `SizeToContent=Height`, `WindowStartupLocation=CenterScreen`, `ResizeMode=CanMinimize`; `StackPanel` com margem 16 contendo `TextBlock` "Adriano Olivares Barbosa" (FontSize 18, SemiBold), `Button` "Coletar dados" (`Command="{Binding CollectDataCommand}"`), `ProgressBar IsIndeterminate="True" Height="6"` com `Visibility` ligada a `IsBusy` via `BooleanToVisibilityConverter` e `TextBlock` com `Text="{Binding StatusMessage}"` e `TextWrapping=Wrap` (layout conforme contracts/ui-contract.md)
- [x] T032 [US1] Atualizar `src/SystemInfoCollector/App.xaml.cs`: sobrescrever `OnStartup` como composition root — criar `SystemInfoRepository(AppPaths.DataFilePath)`, `SystemInfoService` com os coletores, `DialogService`, `MainViewModel` e `MainWindow { DataContext = viewModel }`, definir `MainWindow` e chamar `Show()` (depende de T027–T031)

**Checkpoint**: US1 funcional — `dotnet test` passa T020/T022 e o app coleta e grava o JSON

---

## Phase 4: User Story 2 - Exibir os dados salvos (Priority: P2)

**Goal**: botão "Exibir dados" que só fica habilitado quando o arquivo existe e abre uma janela
com os dados lidos do arquivo, agrupados por categoria.

**Independent Test**: com o arquivo presente, abrir o app (botão já habilitado), clicar
"Exibir dados" e conferir que a janela mostra os valores do arquivo (quickstart cenários 1, 4, 7, 8 e 9).

### Tests for User Story 2

- [ ] T033 [US2] Adicionar a `tests/SystemInfoCollector.Tests/MainViewModelTests.cs` testes de exibição: (a) `CanShowData` é `false` na construção quando o repositório não tem arquivo e `true` quando tem; (b) após coleta bem-sucedida `CanShowData` passa a `true`; (c) `ShowDataCommand` com `Load` lançando `FileNotFoundException` chama `ShowError("O arquivo de dados não existe mais. Colete novamente.")` e `CanShowData` vira `false`; (d) `Load` lançando `InvalidDataException` chama `ShowError("O arquivo de dados está corrompido. Colete novamente.")` e `CanShowData` continua `true`; (e) carga bem-sucedida chama `ShowData` com o objeto carregado. Atualizar os dublês de T021 com `Load` configurável e `ShowData` registrado

### Implementation for User Story 2

- [ ] T034 [US2] Adicionar `void ShowData(SystemInfo systemInfo)` a `src/SystemInfoCollector/Services/IDialogService.cs` e `SystemInfo Load()` a `src/SystemInfoCollector/Services/ISystemInfoRepository.cs`
- [ ] T035 [US2] Implementar `Load()` em `src/SystemInfoCollector/Services/SystemInfoRepository.cs`: se o arquivo não existir lança `FileNotFoundException` com o caminho; lê o texto em UTF-8; desserializa com as mesmas `JsonSerializerSettings`; converte `JsonException` em `InvalidDataException("O arquivo de dados está corrompido.", ex)` e resultado `null` (arquivo vazio ou "null") em `InvalidDataException` (depende de T034)
- [ ] T036 [P] [US2] Criar `src/SystemInfoCollector/ViewModels/DataViewModel.cs`: construtor `(SystemInfo systemInfo)`; propriedades somente leitura `string CollectedAtText` (`CollectedAt.ToString("dd/MM/yyyy HH:mm:ss")`), `OperatingSystemInfo OperatingSystem`, `IReadOnlyList<ProcessorInfo> Processors`, `bool HasProcessors`, `string NetFrameworkText` (`Version` ou "Não disponível", acrescido de `$" (release {Release})"` quando `Release` tiver valor), `IReadOnlyList<string> Warnings`, `bool HasWarnings`
- [ ] T037 [US2] Criar `src/SystemInfoCollector/Views/DataWindow.xaml` e `DataWindow.xaml.cs` (code-behind apenas `InitializeComponent()` e handler do botão Fechar que chama `Close()`): `Title="Dados Coletados"`, `Width=560`, `Height=640`, `WindowStartupLocation=CenterOwner`; `ScrollViewer` vertical com `GroupBox` para "Coleta" (data/hora), "Sistema Operacional" (Nome, Versão, Arquitetura, Nome do computador), "Processador(es)" (`ItemsControl` com Nome e Descrição; `TextBlock` "Não disponível" visível quando `HasProcessors` for `false`), ".NET Framework" e "Avisos" (visível só se `HasWarnings`); valores em `TextBox IsReadOnly=True BorderThickness=0 Background=Transparent` (selecionáveis) usando `NullToNotAvailableConverter` declarado em `Window.Resources`; rótulos em `Grid` de duas colunas; botão "Fechar" no rodapé (depende de T036)
- [ ] T038 [US2] Implementar `ShowData` em `src/SystemInfoCollector/Services/DialogService.cs`: `new DataWindow { Owner = Application.Current.MainWindow, DataContext = new DataViewModel(systemInfo) }.ShowDialog()` (depende de T034, T037)
- [ ] T039 [US2] Atualizar `src/SystemInfoCollector/ViewModels/MainViewModel.cs`: propriedade `bool CanShowData`; método `private void RefreshCanShowData()` (`CanShowData = _repository.Exists()` e `ShowDataCommand.RaiseCanExecuteChanged()`), chamado no construtor e no `finally` da coleta; `RelayCommand ShowDataCommand` com `CanExecute = CanShowData && !IsBusy`; método `ShowData()`: `try { _dialogService.ShowData(_repository.Load()); } catch (FileNotFoundException) {...} catch (InvalidDataException) {...} catch (IOException/UnauthorizedAccessException ex) { ShowError($"Não foi possível ler {FilePath}: {ex.Message}") } finally { RefreshCanShowData(); }` com as mensagens de contracts/system-info-file.md; disparar `RaiseCanExecuteChanged` de ambos os comandos quando `IsBusy` mudar (depende de T034, T035)
- [ ] T040 [US2] Adicionar `Button` "Exibir dados" (`Command="{Binding ShowDataCommand}"`) logo após "Coletar dados" em `src/SystemInfoCollector/Views/MainWindow.xaml` (depende de T039)

**Checkpoint**: US1 e US2 funcionais — ciclo coletar → habilitar → exibir completo

---

## Phase 5: User Story 3 - Incluir as interfaces de rede (Priority: P3)

**Goal**: a coleta inclui as interfaces de rede com IP configurado (nome, IPs e MAC), exibidas na
janela de dados.

**Independent Test**: coletar em um computador com rede ativa e conferir nome, IPs e MAC com
`ipconfig /all` (quickstart cenários 4, 5 e 10).

### Tests for User Story 3

- [ ] T041 [P] [US3] Criar `tests/SystemInfoCollector.Tests/DataViewModelTests.cs`: `HasNetworkInterfaces` é `false` com lista vazia e `true` com uma interface; `HasProcessors` é `false` com lista vazia; `NetFrameworkText` inclui "(release 533509)" quando `Release = 533509`; `HasWarnings` reflete `Warnings`

### Implementation for User Story 3

- [ ] T042 [P] [US3] Criar `src/SystemInfoCollector/Services/Collectors/NetworkInterfaceCollector.cs`: `List<NetworkInterfaceInfo> Collect()` executa `WmiQueries.NetworkAdapterConfiguration`; para cada `ManagementObject` (em `using`) obtém o adaptador via `GetRelated(WmiQueries.NetworkAdapterClassName)` (coleção e itens em `using`) e lê `NetConnectionID`; monta `NetworkInterfaceInfo` com `Name = NetConnectionID ?? Description`, `Description`, `IpAddresses = GetStringArray("IPAddress").ToList()` e `MacAddress = GetString("MACAddress")`. Regra: "somente configurações com `IPEnabled = TRUE`; `IpAddresses` nunca é `null` (vazia se ausente)"
- [ ] T043 [US3] Atualizar `src/SystemInfoCollector/Services/SystemInfoService.cs`: receber `NetworkInterfaceCollector` no construtor e preencher `NetworkInterfaces` via `TryCollect` com nome de seção "interfaces de rede" e fallback lista vazia; atualizar a composição em `src/SystemInfoCollector/App.xaml.cs` (depende de T042)
- [ ] T044 [US3] Atualizar `src/SystemInfoCollector/ViewModels/DataViewModel.cs` com `IReadOnlyList<NetworkInterfaceInfo> NetworkInterfaces` e `bool HasNetworkInterfaces`
- [ ] T045 [US3] Adicionar `GroupBox` "Interfaces de Rede" em `src/SystemInfoCollector/Views/DataWindow.xaml` (antes de "Avisos"): `ItemsControl` com Nome, Descrição, IP(s) (um por linha via `ItemsControl` aninhado) e MAC (com `NullToNotAvailableConverter`), separados por `Separator`; `TextBlock` "Nenhuma interface encontrada" visível quando `HasNetworkInterfaces` for `false` (depende de T044)

**Checkpoint**: US1–US3 funcionais

---

## Phase 6: User Story 4 - Arquivo em formato estruturado (Priority: P3)

**Goal**: garantir que o arquivo é JSON padrão, indentado, com nomes descritivos, e que a leitura
reconstrói exatamente os dados gravados (o JSON já é usado desde US1; esta fase fecha o contrato).

**Independent Test**: abrir o arquivo num editor (JSON válido e indentado conforme
contracts/system-info-file.md) e comparar com a janela de exibição (quickstart cenários 3 e 4).

### Tests for User Story 4

- [ ] T046 [US4] Criar `tests/SystemInfoCollector.Tests/SystemInfoRepositoryTests.cs` (cada teste usa caminho em `Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "system-info.json")` e apaga o diretório em `Dispose`): (a) ida e volta com `SystemInfo` totalmente preenchido (incluindo duas interfaces e múltiplos IPs) preserva todos os valores; (b) campo string `null` é gravado como `null` e lido como `null`; (c) arquivo gravado não começa com BOM (`EF BB BF`), contém `"operatingSystem"` e `"networkInterfaces"` em camelCase e é indentado com 2 espaços; (d) JSON com `"processors": null` e propriedade desconhecida carrega com lista vazia e sem erro; (e) `Load` sem arquivo lança `FileNotFoundException`; (f) JSON malformado lança `InvalidDataException`; (g) dois `Save` seguidos substituem o conteúdo e não deixam `.tmp`; (h) `Exists()` reflete a presença do arquivo; (i) JSON com `"operatingSystem": null` e `"netFramework": null` carrega com instâncias vazias (não nulas) nessas propriedades

### Implementation for User Story 4

- [ ] T047 [US4] Ajustar `src/SystemInfoCollector/Services/SystemInfoRepository.cs` e os modelos em `src/SystemInfoCollector/Models/` até todos os testes de T046 passarem, sem mudar o contrato de contracts/system-info-file.md (depende de T046)

**Checkpoint**: todas as histórias funcionais e o contrato do arquivo coberto por testes

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: documentação, conformidade com a constituição e validação final

- [ ] T048 [P] Criar `README.md` na raiz (português): objetivo do aplicativo; funcionalidades (dados coletados, arquivo JSON, exibição, interfaces de rede); local do arquivo (`%LocalAppData%\SystemInfoCollector\system-info.json`) com exemplo resumido; pré-requisitos e comandos de build/teste/execução de [quickstart.md](quickstart.md); estrutura de pastas; decisões técnicas resumidas de [research.md](research.md); autor "Adriano Olivares Barbosa". O texto descreve apenas o produto
- [ ] T049 Revisar todos os arquivos em `src/` e `tests/` contra a constituição: uma classe por arquivo, namespaces = pastas, nomenclatura, `using` em todo `IDisposable`, sem strings mágicas, XML docs em membros públicos, nenhum `catch (Exception)` fora de `App.xaml.cs`, nenhuma lógica de WMI/registro/I/O em code-behind
- [ ] T050 Executar `dotnet build SystemInfoCollector.sln -c Release` e corrigir até zero warnings e zero erros
- [ ] T051 Executar `dotnet test SystemInfoCollector.sln -c Release` e garantir 100% dos testes passando
- [ ] T052 Executar manualmente os cenários 1–10 de [quickstart.md](quickstart.md) e registrar qualquer divergência (SC-001 a SC-006)
- [ ] T053 Preparar a publicação: confirmar que a raiz contém apenas `src/`, `tests/`, `specs/`, `.specify/`, `SystemInfoCollector.sln`, `README.md`, `.gitignore`, `.editorconfig` (e `.claude/`, ignorado). Se houver qualquer outro arquivo (ex.: textos de requisitos originais), PARAR e pedir ao usuário que o mova para fora da pasta — não citá-lo em nenhum arquivo versionado. Remover o comentário "Sync Impact Report" do topo de `.specify/memory/constitution.md`
- [ ] T054 Inicializar o repositório git na raiz (`git init -b main`), criar a branch `001-system-info-collector`, conferir com `git status --ignored` que `bin/`, `obj/`, `.vs/` e `.claude/` estão ignorados, e fazer commits descritivos agrupados (ex.: especificação; estrutura da solução; coleta e gravação; exibição; interfaces de rede; testes e README), fazendo merge na `main` ao final (depende de T053)
- [ ] T055 Publicar no GitHub: com confirmação explícita do usuário (nome e visibilidade do repositório), criar o repositório remoto (`gh repo create <nome> --source . --push` ou remoto criado pelo usuário + `git push -u origin main`) e informar a URL ao usuário (depende de T054; constituição, Princípio II)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependências
- **Foundational (Phase 2)**: depende do Setup — BLOQUEIA todas as histórias
- **US1 (Phase 3)**: depende da Foundational
- **US2 (Phase 4)**: depende da US1 (usa `MainViewModel`, `SystemInfoRepository` e `MainWindow`)
- **US3 (Phase 5)**: coleta (T042–T043) depende só da US1; exibição (T044–T045) depende da US2
- **US4 (Phase 6)**: depende da US2 (`Load`); idealmente após a US3 para cobrir rede na ida e volta
- **Polish (Phase 7)**: depende de todas as histórias

### Within Each User Story

- Testes antes da implementação correspondente (devem falhar primeiro)
- Modelos → coletores/repositório → serviços → ViewModels → Views → composição em `App`

### Parallel Opportunities

- Setup: T002, T004, T005 em paralelo após T001
- Foundational: T006–T009 e T011–T018 em paralelo; T010 após T006–T009
- US1: T020, T021 em paralelo; T023–T026 e T030 em paralelo
- US2: T036 em paralelo com T034/T035
- US3: T041 e T042 em paralelo
- Polish: T048 em paralelo com T049; T053–T055 são sequenciais e vêm por último

---

## Parallel Example: User Story 1

```text
# Testes:
Task: "NetFrameworkVersionMapperTests em tests/SystemInfoCollector.Tests/NetFrameworkVersionMapperTests.cs"
Task: "Dublês em tests/SystemInfoCollector.Tests/Fakes/"

# Coletores (arquivos independentes):
Task: "NetFrameworkVersionMapper em src/SystemInfoCollector/Services/Collectors/NetFrameworkVersionMapper.cs"
Task: "NetFrameworkCollector em src/SystemInfoCollector/Services/Collectors/NetFrameworkCollector.cs"
Task: "OperatingSystemCollector em src/SystemInfoCollector/Services/Collectors/OperatingSystemCollector.cs"
Task: "ProcessorCollector em src/SystemInfoCollector/Services/Collectors/ProcessorCollector.cs"
Task: "DialogService em src/SystemInfoCollector/Services/DialogService.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1: Setup
2. Phase 2: Foundational
3. Phase 3: US1
4. **STOP and VALIDATE**: arquivo criado com SO, processador e .NET corretos

### Incremental Delivery

1. Setup + Foundational → base compilando
2. US1 → coleta e gravação (MVP)
3. US2 → exibição com botão condicionado ao arquivo (requisitos obrigatórios completos)
4. US3 → interfaces de rede
5. US4 → contrato do arquivo coberto por testes
6. Polish → README, revisão, build/testes, quickstart, publicação no GitHub

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências pendentes
- Commit após cada tarefa ou grupo lógico, com mensagens descritivas
- Parar em cada checkpoint para validar a história isoladamente
