# Data Model: Coletor de Informações do Sistema

**Feature**: `001-system-info-collector` | **Date**: 2026-10-14

Modelos tipados em `SystemInfoCollector.Models`. Todos são classes simples (POCO) com
propriedades públicas `get; set;` para permitir a desserialização. Strings ausentes são `null`
no modelo e no arquivo; a apresentação converte `null` em "Não disponível" (FR-014).

## SystemInfo (raiz)

| Propriedade | Tipo | Obrigatória | Descrição |
|---|---|---|---|
| `CollectedAt` | `DateTimeOffset` (horário local; ISO 8601 com offset no arquivo) | Sim | Momento da coleta |
| `OperatingSystem` | `OperatingSystemInfo` | Sim (pode ter campos nulos) | Dados do SO |
| `Processors` | `List<ProcessorInfo>` | Sim (pode ser vazia) | Um item por processador físico |
| `NetFramework` | `NetFrameworkInfo` | Sim | Versão do .NET Framework |
| `NetworkInterfaces` | `List<NetworkInterfaceInfo>` | Sim (pode ser vazia) | Interfaces com IP configurado |
| `Warnings` | `List<string>` | Sim (normalmente vazia) | Falhas parciais da coleta (R9) |

**Regras**: listas nunca são `null` (inicializadas vazias no construtor e após desserializar).

## OperatingSystemInfo

| Propriedade | Origem (WMI `Win32_OperatingSystem`) | Exemplo |
|---|---|---|
| `Caption` | `Caption` | "Microsoft Windows 11 Pro" |
| `Version` | `Version` | "10.0.26300" |
| `Architecture` | `OSArchitecture` | "64 bits" |
| `ComputerName` | `CSName` | "DESKTOP-ABC123" |

## ProcessorInfo

| Propriedade | Origem (WMI `Win32_Processor`) | Exemplo |
|---|---|---|
| `Name` | `Name` (trim) | "AMD Ryzen 7 5800X 8-Core Processor" |
| `Description` | `Description` | "AMD64 Family 25 Model 33 Stepping 0" |

## NetFrameworkInfo

| Propriedade | Origem | Exemplo |
|---|---|---|
| `Version` | Mapeamento de `Release` (ver research R5) | "4.8.1 ou superior" |
| `Release` | Registro `NDP\v4\Full\Release` (`int?`) | 533509 |

**Regras**:
- `Release == null` ou `Release < 378389` → `Version = "Não detectado (anterior à 4.5)"`.
- `Release >= 533320` (faixa mais alta conhecida) → `Version = "4.8.1 ou superior"` (o valor
  bruto fica em `Release` e é exibido junto).
- Falha de acesso ao registro → `Version = null`, `Release = null`, aviso em `Warnings`.

## NetworkInterfaceInfo

| Propriedade | Origem | Exemplo |
|---|---|---|
| `Name` | `Win32_NetworkAdapter.NetConnectionID`, fallback `Description` | "Ethernet" |
| `Description` | `Win32_NetworkAdapterConfiguration.Description` | "Intel(R) Ethernet Connection I219-V" |
| `IpAddresses` | `Win32_NetworkAdapterConfiguration.IPAddress` (`string[]`) | ["192.168.0.10", "fe80::1c2b:..."] |
| `MacAddress` | `Win32_NetworkAdapterConfiguration.MACAddress` | "00:1A:2B:3C:4D:5E" |

**Regras**: somente configurações com `IPEnabled = TRUE`; `IpAddresses` nunca é `null`
(vazia se ausente).

**Fonte complementar** (research R4.1): adaptadores conectados que não constam acima são
acrescentados a partir de `MSFT_NetAdapter` (`Name` → `Name`, `InterfaceDescription` →
`Description`, `PermanentAddress` → `MacAddress` formatado com `:`) e `MSFT_NetIPAddress`
(`IPAddress`, sem o índice de zona IPv6). Somente adaptadores com ao menos um IP são incluídos.

## Relacionamentos

```text
SystemInfo 1 ── 1 OperatingSystemInfo
           1 ── * ProcessorInfo
           1 ── 1 NetFrameworkInfo
           1 ── * NetworkInterfaceInfo
```

## Estados do arquivo de coleta (controla o botão "Exibir dados")

```text
[Inexistente] --coleta OK--> [Existente] --nova coleta OK--> [Existente (substituído)]
[Existente] --apagado externamente / detectado ao exibir--> [Inexistente]
[Existente] --conteúdo inválido ao exibir--> [Existente, ilegível] (mensagem; botão continua habilitado)
```

- `CanShowData = repository.Exists()`, reavaliado: na abertura da janela, após cada coleta e
  após cada tentativa de exibição.
