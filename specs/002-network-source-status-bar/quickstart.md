# Quickstart: Fonte única de rede e barra de status

Pré-requisitos, build e execução: iguais aos da
[feature 001](../001-system-info-collector/quickstart.md).

```powershell
dotnet build SystemInfoCollector.sln -c Release
dotnet test SystemInfoCollector.sln -c Release
.\src\SystemInfoCollector\bin\Release\net48\SystemInfoCollector.exe
```

## Cenários de validação — rede

| # | Passo | Resultado esperado | Ref. |
|---|---|---|---|
| R1 | `Get-NetAdapter \| Where-Object Status -eq Up` e anotar nome, descrição e MAC | Lista de referência | — |
| R2 | Coletar e clicar "Exibir dados" | Mesmas interfaces de R1 que tenham IP, cada uma uma única vez, inclusive virtuais | FR-001, FR-002, SC-001 |
| R3 | Comparar IPs com `Get-NetIPAddress -InterfaceAlias <nome>` | Mesmos endereços, IPv6 sem `%n` | FR-003 |
| R4 | Conferir MAC de cada interface | Formato `XX:XX:XX:XX:XX:XX`; túneis sem MAC → "Não disponível" | FR-003 |
| R5 | Medir a coleta de rede isolada (script de medição abaixo) | ≤ 50% do tempo anterior (~670 ms) | SC-002 |

Medição isolada (após o build):

```powershell
$bin = (Resolve-Path "src\SystemInfoCollector\bin\Release\net48").Path
[Reflection.Assembly]::LoadFrom("$bin\SystemInfoCollector.exe") | Out-Null
Measure-Command { (New-Object SystemInfoCollector.Services.Collectors.NetworkInterfaceCollector).Collect() }
```

## Cenários de validação — barra de status

Contrato de mensagens: [contracts/ui-status-bar.md](contracts/ui-status-bar.md).

| # | Passo | Resultado esperado | Ref. |
|---|---|---|---|
| S1 | Abrir o app | Barra: ícone de informação + "Pronto." | FR-006 |
| S2 | Clicar "Coletar dados" | Barra: "Coletando dados..." e depois "Dados salvos em ..."; **nenhum** diálogo | FR-007, SC-003 |
| S3 | Reduzir a largura da janela ou observar caminho longo | Texto cortado com "..."; tooltip mostra o texto completo | FR-011 |
| S4 | Apagar o arquivo com o app aberto e clicar "Exibir dados" | Barra: erro (ícone/cor) "O arquivo de dados não existe mais..."; **nenhum** diálogo; botão desabilita | FR-007 |
| S5 | Coletar, corromper o JSON e clicar "Exibir dados" | Barra: erro "O arquivo de dados está corrompido..."; **nenhum** diálogo | FR-007 |
| S6 | Coleta com aviso (coberta por teste automatizado; manualmente, opcional) | Diálogo com a lista + barra de aviso "Coleta concluída com 1 aviso" | FR-008 |
| S7 | Após uma coleta, marcar o arquivo como somente leitura (`attrib +r "$env:LOCALAPPDATA\SystemInfoCollector\system-info.json"`) e coletar de novo; ao final, `attrib -r` | Diálogo com o motivo + barra de erro "Não foi possível salvar os dados." | FR-008 |
| S8 | Repetir a validação da feature 001 (cenários 1–10) | Comportamento inalterado, exceto as mensagens acima | FR-012 |
