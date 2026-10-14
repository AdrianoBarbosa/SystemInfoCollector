# Coletor de Informações do Sistema

Aplicativo desktop para Windows (WPF, .NET Framework 4.8) que coleta informações do computador
local via **WMI** e **registro do Windows**, grava o resultado em um arquivo **JSON** e exibe os
dados de forma organizada.

Autor: **Adriano Olivares Barbosa**

## Funcionalidades

- **Coletar dados**: consulta o computador em segundo plano (a janela continua responsiva) e grava
  o resultado em arquivo.
- **Exibir dados**: lê o arquivo e mostra as informações agrupadas por categoria. O botão só fica
  habilitado quando o arquivo existe — inclusive ao abrir o aplicativo com um arquivo gerado
  anteriormente.
- **Coleta parcial**: se uma fonte falhar (serviço WMI indisponível, permissão negada), as demais
  são coletadas normalmente e o problema é informado como aviso. Valores ausentes aparecem como
  "Não disponível".

### Dados coletados

| Categoria | Fonte | Campos |
|---|---|---|
| Sistema operacional | WMI `Win32_OperatingSystem` | Caption, Version, OSArchitecture, CSName |
| Processador(es) | WMI `Win32_Processor` | Name, Description |
| .NET Framework | Registro `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full` | Valor `Release`, convertido na versão legível (tabela oficial da Microsoft) |
| Interfaces de rede | WMI `Win32_NetworkAdapterConfiguration` (principal) + `MSFT_NetAdapter`/`MSFT_NetIPAddress` (complementar) | Nome, descrição, IP(s) e MAC |

## ⚠️ Decisão de projeto: duas fontes WMI para as interfaces de rede

A fonte **principal** das interfaces de rede é a classe `Win32_NetworkAdapterConfiguration`
(com o nome da conexão obtido do `Win32_NetworkAdapter` associado).

Durante a validação foi constatado que as classes `Win32_*` **não listam alguns adaptadores
virtuais ativos** — por exemplo, o `vEthernet (Default Switch)` do Hyper-V, que aparece no
Windows (`Get-NetAdapter`, `ipconfig`) mas não nessas classes. Para que o resultado corresponda
ao que o Windows exibe, o aplicativo usa as classes `MSFT_NetAdapter` e `MSFT_NetIPAddress`
(namespace `root\StandardCimv2`) como fonte **complementar**:

- a fonte principal é sempre preservada; da complementar entram **apenas** adaptadores
  conectados que ainda não foram listados (comparação pela descrição do adaptador ou pelo MAC);
- o formato é unificado (MAC como `00:15:5D:00:00:01`, IPv6 sem o sufixo `%n`);
- as classes `MSFT_*` também são WMI e existem desde o Windows 8, portanto estão presentes em
  qualquer Windows 10 ou superior;
- cada fonte é coletada de forma independente: se a complementar falhar, os dados da principal
  são mantidos e apenas um aviso é registrado.

Detalhes e alternativas avaliadas em
[specs/001-system-info-collector/research.md](specs/001-system-info-collector/research.md#r41-fonte-complementar-de-interfaces-de-rede).

## Arquivo de dados

Gravado em `%LocalAppData%\SystemInfoCollector\system-info.json` (pasta do usuário, com
permissão de escrita garantida), em JSON indentado e UTF-8. Cada coleta substitui a anterior; a
gravação passa por um arquivo temporário para nunca deixar o arquivo truncado.

```json
{
  "collectedAt": "2026-10-14T11:37:03.3401983-03:00",
  "operatingSystem": {
    "caption": "Microsoft Windows 11 Pro",
    "version": "10.0.26300",
    "architecture": "64 bits",
    "computerName": "DESKTOP-ABC123"
  },
  "processors": [
    { "name": "11th Gen Intel(R) Core(TM) i7-11800H @ 2.30GHz", "description": "Intel64 Family 6 Model 141 Stepping 1" }
  ],
  "netFramework": { "version": "4.8.1 ou superior", "release": 533509 },
  "networkInterfaces": [
    {
      "name": "Wi-Fi",
      "description": "Killer(R) Wi-Fi 6 AX1650i 160MHz Wireless Network Adapter (201NGW)",
      "ipAddresses": [ "192.168.0.10", "fe80::1111:2222:3333:4444" ],
      "macAddress": "00:11:22:33:44:55"
    }
  ],
  "warnings": []
}
```

Contrato completo em
[specs/001-system-info-collector/contracts/system-info-file.md](specs/001-system-info-collector/contracts/system-info-file.md).

## Pré-requisitos

- Windows 10 1903 ou superior (ou Windows 11)
- .NET Framework 4.8 (runtime) e .NET Framework 4.8 Developer Pack
- Visual Studio 2022 ou superior com a carga "Desenvolvimento para desktop com .NET",
  **ou** .NET SDK 8 ou superior para compilar pela linha de comando

## Build, testes e execução

```powershell
dotnet build SystemInfoCollector.sln -c Release
dotnet test SystemInfoCollector.sln -c Release
.\src\SystemInfoCollector\bin\Release\net48\SystemInfoCollector.exe
```

Ou abra `SystemInfoCollector.sln` no Visual Studio e execute o projeto `SystemInfoCollector`.

O roteiro de validação manual está em
[specs/001-system-info-collector/quickstart.md](specs/001-system-info-collector/quickstart.md).

## Estrutura

```text
SystemInfoCollector.sln
src/SystemInfoCollector/              Aplicativo WPF (net48, AnyCPU)
├── Models/                           Modelos tipados da coleta
├── Services/                         Orquestração, persistência JSON e diálogos
│   └── Collectors/                   Coletores WMI e registro
├── ViewModels/                       MVVM: janela principal, janela de dados e comandos
├── Views/                            Janelas XAML (sem lógica no code-behind)
└── Converters/                       Conversores de exibição
tests/SystemInfoCollector.Tests/      Testes xUnit da lógica sem interface
specs/001-system-info-collector/      Especificação, plano, pesquisa técnica e tarefas
```

## Decisões técnicas

- **WPF + MVVM leve**, sem bibliotecas de terceiros: três classes de infraestrutura
  (`ViewModelBase`, `RelayCommand`, `AsyncRelayCommand`) mantêm o code-behind vazio e permitem
  testar o estado dos botões.
- **Separação de responsabilidades**: coleta (`Services/Collectors`), persistência
  (`SystemInfoRepository`) e apresentação (ViewModels/Views) ficam em classes distintas, com
  serviços expostos por interfaces.
- **Coleta assíncrona** (`Task.Run`): a interface não trava e não é possível iniciar duas coletas
  ao mesmo tempo.
- **Registro na view correta**: a leitura usa explicitamente a view de 64 bits em sistemas
  64 bits, com o mesmo resultado em processos "Any CPU" ou "x64".
- **Newtonsoft.Json** para serialização/desserialização, com tolerância a campos ausentes ou
  nulos na leitura.
- **Qualidade**: build sem warnings (`TreatWarningsAsErrors`), documentação XML em todos os
  membros públicos e convenções de nomenclatura verificadas pelo `.editorconfig`.

A especificação completa, o plano e a pesquisa técnica estão em
[specs/001-system-info-collector/](specs/001-system-info-collector/).
