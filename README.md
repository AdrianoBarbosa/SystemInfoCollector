# Coletor de Informações do Sistema

Aplicativo desktop para Windows (WPF, .NET Framework 4.8) que coleta informações do computador
local via **WMI** e **registro do Windows**, grava o resultado em um arquivo **JSON** e exibe os
dados de forma organizada.

Autor: **Adriano Olivares Barbosa**

> Projeto desenvolvido como avaliação técnica de desenvolvimento C#.

## Funcionalidades

- **Coletar dados**: consulta o computador em segundo plano (a janela continua responsiva) e grava
  o resultado em arquivo.
- **Exibir dados**: lê o arquivo e mostra as informações agrupadas por categoria. O botão só fica
  habilitado quando o arquivo existe — inclusive ao abrir o aplicativo com um arquivo gerado
  anteriormente.
- **Barra de status**: mensagens curtas (coleta em andamento, coleta concluída, arquivo ausente ou
  corrompido) aparecem na parte inferior da janela, com ícone e cor indicando informação, aviso ou
  erro, sem exigir clique em "OK". Caixas de diálogo ficam reservadas para mensagens com detalhes
  (lista de avisos da coleta, motivo de falha ao salvar ou ler o arquivo, erro inesperado), e a
  barra exibe um resumo correspondente.
- **Coleta parcial**: se uma fonte falhar (serviço WMI indisponível, permissão negada), as demais
  são coletadas normalmente e o problema é informado como aviso. Valores ausentes aparecem como
  "Não disponível".

### Dados coletados

| Categoria | Fonte | Campos |
|---|---|---|
| Sistema operacional | WMI `Win32_OperatingSystem` | Caption, Version, OSArchitecture, CSName |
| Processador(es) | WMI `Win32_Processor` | Name, Description |
| .NET Framework | Registro `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full` | Valor `Release`, convertido na versão legível (tabela oficial da Microsoft) |
| Interfaces de rede | WMI `MSFT_NetAdapter` + `MSFT_NetIPAddress` (namespace `root\StandardCimv2`) | Nome da conexão, descrição, IP(s) e MAC |

## ⚠️ Decisão de projeto: interfaces de rede via `MSFT_NetAdapter`

As interfaces de rede são coletadas pelas classes WMI `MSFT_NetAdapter` (adaptadores conectados) e
`MSFT_NetIPAddress` (endereços IP), em vez da classe mais tradicional
`Win32_NetworkAdapterConfiguration`. A coleta continua sendo feita via WMI.

Motivos:

- as classes `Win32_*` **não listam alguns adaptadores virtuais ativos** — por exemplo, o
  `vEthernet (Default Switch)` do Hyper-V, que aparece no Windows (`Get-NetAdapter`, `ipconfig`) mas
  não em `Win32_NetworkAdapterConfiguration`. Com `MSFT_NetAdapter`, o resultado corresponde
  exatamente às interfaces que o Windows apresenta como ativas;
- `MSFT_NetAdapter` já traz o nome da conexão ("Wi-Fi", "Ethernet"), sem consultas de associação
  adicionais, o que deixa o código mais simples;
- a coleta de rede ficou cerca de 4 vezes mais rápida no computador de validação;
- as classes `MSFT_*` existem desde o Windows 8, portanto estão presentes em qualquer Windows 10 ou
  superior, e não exigem privilégios de administrador.

O formato é normalizado para manter o arquivo legível: MAC como `XX:XX:XX:XX:XX:XX` e IPv6 sem o
sufixo de zona (`%n`).

Histórico da decisão e alternativas avaliadas em
[specs/002-network-source-status-bar/research.md](specs/002-network-source-status-bar/research.md#r1-fonte-única-de-interfaces-de-rede).

## Arquivo de dados

Gravado em `%LocalAppData%\SystemInfoCollector\system-info.json` (pasta do usuário, com
permissão de escrita garantida), em JSON indentado e UTF-8. Cada coleta substitui a anterior; a
gravação passa por um arquivo temporário para nunca deixar o arquivo truncado.

Exemplo (endereços IP e MAC mascarados):

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
      "ipAddresses": [ "fe80::xxxx:xxxx:xxxx:xxxx", "192.168.xxx.xxx" ],
      "macAddress": "xx:xx:xx:xx:xx:xx"
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

Os roteiros de validação manual estão em
[specs/001-system-info-collector/quickstart.md](specs/001-system-info-collector/quickstart.md) e
[specs/002-network-source-status-bar/quickstart.md](specs/002-network-source-status-bar/quickstart.md).

## Estrutura

```text
SystemInfoCollector.sln
src/SystemInfoCollector/              Aplicativo WPF (net48, AnyCPU)
├── Models/                           Modelos tipados da coleta
├── Services/                         Orquestração, persistência JSON e diálogos
│   └── Collectors/                   Coletores WMI e registro
├── ViewModels/                       MVVM: janela principal, janela de dados, comandos e status
├── Views/                            Janelas XAML (sem lógica no code-behind)
└── Converters/                       Conversores de exibição
tests/SystemInfoCollector.Tests/      Testes xUnit da lógica sem interface
specs/
├── 001-system-info-collector/        Especificação, plano, pesquisa e tarefas da versão inicial
└── 002-network-source-status-bar/    Fonte única de rede e barra de status
```

## Decisões técnicas

- **WPF + MVVM leve**, sem bibliotecas de terceiros: três classes de infraestrutura
  (`ViewModelBase`, `RelayCommand`, `AsyncRelayCommand`) mantêm o code-behind vazio e permitem
  testar o estado dos botões e da barra de status.
- **Separação de responsabilidades**: coleta (`Services/Collectors`), persistência
  (`SystemInfoRepository`) e apresentação (ViewModels/Views) ficam em classes distintas, com
  serviços expostos por interfaces.
- **Coleta assíncrona** (`Task.Run`): a interface não trava e não é possível iniciar duas coletas
  ao mesmo tempo.
- **Mensagens com destino fixo**: cada mensagem tem um destino definido (barra de status ou
  diálogo) no ponto em que é gerada, sem regras dependentes do tamanho do texto.
- **Registro na view correta**: a leitura usa explicitamente a view de 64 bits em sistemas
  64 bits, com o mesmo resultado em processos "Any CPU" ou "x64".
- **Newtonsoft.Json** para serialização/desserialização, com tolerância a campos ausentes ou
  nulos na leitura.
- **Qualidade**: build sem warnings (`TreatWarningsAsErrors`), documentação XML em todos os
  membros públicos e convenções de nomenclatura verificadas pelo `.editorconfig`.

A especificação completa, os planos e as pesquisas técnicas estão em [specs/](specs/).
