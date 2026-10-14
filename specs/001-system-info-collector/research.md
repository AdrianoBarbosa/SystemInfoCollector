# Research: Coletor de Informações do Sistema

**Feature**: `001-system-info-collector` | **Date**: 2026-10-14

Cada item abaixo resolve uma decisão técnica em aberto no Technical Context do [plan.md](plan.md).

## R1. Tecnologia de interface

- **Decision**: WPF sobre .NET Framework 4.8, com MVVM leve escrito à mão (`ViewModelBase`,
  `RelayCommand`, `AsyncRelayCommand`), sem framework MVVM de terceiros.
- **Rationale**: WPF é a opção mais rica entre as permitidas e o binding com MVVM mantém o
  code-behind vazio, cumprindo o Princípio III (UI sem lógica de coleta/I/O). .NET Framework 4.8
  é a última versão 4.x com targeting pack instalado no ambiente (verificado em
  `Reference Assemblies\...\v4.8`) e vem com o Windows 10 1903+ e o Windows 11.
- **Alternatives considered**:
  - Windows Forms: atende aos requisitos, mas não favorece separação UI/lógica tanto quanto
    binding + comandos.
  - CommunityToolkit.Mvvm / Prism: resolvem o mesmo problema com mais dependência e
    "mágica" (source generators); três classes pequenas bastam (Princípio V).
  - .NET Framework 4.5.2: mínimo permitido, mas fora de suporte e sem vantagem prática.

## R2. Formato do projeto e build

- **Decision**: projetos no formato SDK-style (`<Project Sdk="Microsoft.NET.Sdk">`) com
  `TargetFramework=net48`, `UseWPF=true`, `PlatformTarget=AnyCPU`, `Prefer32Bit=false`.
- **Rationale**: `.csproj` curto e legível, `PackageReference` nativo, build e testes tanto pelo
  Visual Studio quanto por `dotnet build` / `dotnet test`. Continua sendo um aplicativo .NET
  Framework 4.8. `Prefer32Bit=false` garante processo 64 bits em Windows x64, o que simplifica
  o acesso ao registro e ao WMI.
- **Alternatives considered**: `.csproj` clássico (non-SDK) com `packages.config` — funcional,
  mas verboso e sem suporte a `dotnet` CLI.

## R3. Coleta via WMI

- **Decision**: `System.Management` (`ManagementObjectSearcher`) com consultas WQL que
  selecionam apenas as propriedades necessárias:
  - `SELECT Caption, Version, OSArchitecture, CSName FROM Win32_OperatingSystem`
  - `SELECT Name, Description FROM Win32_Processor`
  - `SELECT Index, Description, IPAddress, MACAddress FROM Win32_NetworkAdapterConfiguration
    WHERE IPEnabled = TRUE`
- **Rationale**: API padrão do .NET Framework para WMI; selecionar só as colunas usadas reduz o
  tempo de consulta. Todos os `ManagementObjectSearcher`, `ManagementObjectCollection` e
  `ManagementBaseObject` são liberados com `using`. Valores lidos por um helper que converte
  `null`/ausente em `null` tipado, e a apresentação mostra "Não disponível".
- **Alternatives considered**: `Microsoft.Management.Infrastructure` (CIM/MI) — mais moderno,
  porém exige pacote extra e é menos familiar; P/Invoke — desnecessário.

## R4. Nome amigável das interfaces de rede

- **Decision**: para cada `Win32_NetworkAdapterConfiguration` com IP habilitado, obter o adaptador
  relacionado via `ManagementObject.GetRelated("Win32_NetworkAdapter")` e usar
  `NetConnectionID` (ex.: "Ethernet", "Wi-Fi") como nome; se ausente, usar `Description`.
- **Rationale**: `Win32_NetworkAdapterConfiguration` não tem o nome que o usuário vê no Windows;
  a associação WMI é a forma suportada de ligar as duas classes. Guardar também a descrição do
  adaptador ajuda a conferir com o Gerenciador de Dispositivos (SC-003).
- **Alternatives considered**: `System.Net.NetworkInformation.NetworkInterface` — mais simples,
  mas o requisito pede explicitamente WMI; segunda consulta e junção manual por `Index`/`DeviceID`
  — equivalente, porém mais código.

## R5. Versão do .NET Framework pelo registro

- **Decision**: ler o DWORD `Release` em
  `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full`, abrindo
  `RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)` quando o SO for
  64 bits (`Registry32` caso contrário), e mapear pelo menor valor mínimo da tabela oficial:

  | Release mínimo | Versão |
  |---|---|
  | 533320 | 4.8.1 |
  | 528040 | 4.8 |
  | 461808 | 4.7.2 |
  | 461308 | 4.7.1 |
  | 460798 | 4.7 |
  | 394802 | 4.6.2 |
  | 394254 | 4.6.1 |
  | 393295 | 4.6 |
  | 379893 | 4.5.2 |
  | 378675 | 4.5.1 |
  | 378389 | 4.5 |

- **Rationale**: método documentado pela Microsoft ("How to: Determine which .NET Framework
  versions are installed"). A comparação `>=` em ordem decrescente cobre valores específicos
  de cada edição do Windows. O mapeamento fica numa classe estática pura
  (`NetFrameworkVersionMapper`), testável sem registro. Seguindo o exemplo oficial, a faixa
  mais alta (`>= 533320`) é exibida como "4.8.1 ou superior", pois versões futuras não podem
  ser distinguidas pelo número; `Release` abaixo de 378389 ou chave ausente → "Não detectado
  (anterior à 4.5)". Ambiente atual: `Release = 533509` → "4.8.1 ou superior".
- **Alternatives considered**: `Environment.Version` / `RuntimeInformation.FrameworkDescription`
  — retornam a versão do CLR em execução, não a instalada, e não leem o registro como exigido.

## R6. Serialização JSON

- **Decision**: Newtonsoft.Json 13.0.3 com `Formatting.Indented`, `NullValueHandling.Include` e
  nomes de propriedade em camelCase (`CamelCasePropertyNamesContractResolver`). Arquivo gravado
  em UTF-8 sem BOM.
- **Rationale**: biblioteca padrão de fato no .NET Framework, estável, sem dependências
  transitivas, com controle simples de indentação e nomes. A desserialização reconstrói os
  modelos tipados (FR-008).
- **Alternatives considered**: `System.Text.Json` via NuGet — funciona em net48, mas arrasta
  vários pacotes (`System.Memory`, `System.Buffers`...) e exige binding redirects;
  `DataContractJsonSerializer` — embutido, porém exige atributos em tudo e formatação de datas
  pouco legível.

## R7. Local e nome do arquivo

- **Decision**: `%LocalAppData%\SystemInfoCollector\system-info.json`, caminho montado em um
  único lugar (`AppPaths`) com `Environment.GetFolderPath(SpecialFolder.LocalApplicationData)`.
  O diretório é criado se não existir. Gravação atômica: escreve em `system-info.json.tmp` e
  substitui o arquivo final, evitando arquivo truncado em caso de falha.
- **Rationale**: pasta do executável pode ficar em `Program Files` (sem permissão de escrita);
  `LocalAppData` é o local recomendado para dados por usuário.
- **Alternatives considered**: pasta do executável — quebra quando instalado; Documentos —
  polui pasta do usuário.

## R8. Responsividade e concorrência

- **Decision**: a coleta roda em `Task.Run` dentro de um `AsyncRelayCommand` que fica
  `CanExecute = false` enquanto executa (desabilita o botão e bloqueia cliques duplicados). Um
  `IsBusy` no ViewModel mostra indicador de progresso.
- **Rationale**: atende FR-012 com o mínimo de código; WMI é síncrono e pode levar segundos.
- **Alternatives considered**: `BackgroundWorker` — legado; `Dispatcher.BeginInvoke` manual —
  mais verboso.

## R9. Coleta parcial e tratamento de erros

- **Decision**: o `SystemInfoService` chama cada coletor isoladamente; se um lançar
  `ManagementException`, `UnauthorizedAccessException`, `SecurityException` ou
  `COMException`, a seção afetada fica vazia/"Não disponível" e uma mensagem é adicionada a
  `SystemInfo.Warnings`. Erros de I/O na gravação/leitura são capturados no ViewModel e
  exibidos por `IDialogService`. JSON inválido gera `InvalidDataException` traduzida em mensagem
  amigável.
- **Rationale**: cumpre FR-013/FR-014 e o Princípio IV sem `catch (Exception)` genérico espalhado.
  Um único `DispatcherUnhandledException` em `App` serve de rede de segurança para SC-005.

## R10. Testes

- **Decision**: projeto `SystemInfoCollector.Tests` (net48) com xUnit 2.9.x,
  `xunit.runner.visualstudio` 2.8.x e `Microsoft.NET.Test.Sdk` 17.x. Cobertura: mapeamento de
  `Release` → versão, ida e volta JSON (`SystemInfoRepository` com caminho temporário), lógica do
  `MainViewModel` (estado do botão "Exibir dados") com dublês das interfaces.
- **Rationale**: a lógica pura é onde estão as regras; a coleta WMI real é validada no
  quickstart contra o próprio Windows (SC-003).
- **Alternatives considered**: MSTest — equivalente; testes automatizados de UI — custo alto
  para o tamanho do projeto (Princípio V).
