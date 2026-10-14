# Data Model: Fonte única de rede e barra de status

**Feature**: `002-network-source-status-bar` | **Date**: 2026-10-14

Os modelos persistidos da feature 001 ([data-model](../001-system-info-collector/data-model.md))
**não mudam de estrutura**; o arquivo JSON mantém o mesmo formato
([contrato](../001-system-info-collector/contracts/system-info-file.md)).

## NetworkInterfaceInfo — nova origem dos campos

| Propriedade | Origem (WMI `root\StandardCimv2`) | Exemplo |
|---|---|---|
| `Name` | `MSFT_NetAdapter.Name` | "Wi-Fi" |
| `Description` | `MSFT_NetAdapter.InterfaceDescription` | "Killer(R) Wi-Fi 6 AX1650i 160MHz Wireless Network Adapter (201NGW)" |
| `IpAddresses` | `MSFT_NetIPAddress.IPAddress` com o mesmo `InterfaceIndex`, sem sufixo `%n` | ["fe80::1111:2222:3333:4444", "192.168.0.10"] |
| `MacAddress` | `MSFT_NetAdapter.PermanentAddress` formatado `XX:XX:XX:XX:XX:XX`; vazio → `null` | "00:11:22:33:44:55" |

**Regras**:
- Somente adaptadores com `MediaConnectState = 1` (conectados).
- Somente adaptadores com ao menos um IP (junção por `InterfaceIndex`).
- `IpAddresses` nunca é `null`.
- Sem deduplicação: cada adaptador é uma interface distinta.

## StatusKind (novo, somente em memória)

Enum em `SystemInfoCollector.ViewModels`:

| Valor | Uso | Ícone (Segoe MDL2) | Cor |
|---|---|---|---|
| `Information` | Estado normal, sucesso | `` | padrão do sistema |
| `Warning` | Coleta concluída com avisos | `` | `#9D5D00` |
| `Error` | Falhas de arquivo | `` | `#C42B1C` |

## Estado da barra de status (MainViewModel)

| Propriedade | Tipo | Regra |
|---|---|---|
| `StatusMessage` | `string` | Última mensagem; inicial "Pronto." |
| `StatusKind` | `StatusKind` | Tipo da última mensagem; inicial `Information` |

Ambas mudam juntas (método `SetStatus`), sempre substituindo os valores anteriores. Não há
histórico nem expiração.
