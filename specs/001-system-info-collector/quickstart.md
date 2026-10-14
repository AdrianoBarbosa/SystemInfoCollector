# Quickstart: Coletor de Informações do Sistema

## Pré-requisitos

- Windows 10 1903+ ou Windows 11
- .NET Framework 4.8 (runtime) e Developer Pack 4.8 (targeting pack)
- Visual Studio 2022+ com a carga "Desenvolvimento para desktop com .NET" **ou** .NET SDK 8+

## Build e testes

```powershell
dotnet restore SystemInfoCollector.sln
dotnet build SystemInfoCollector.sln -c Release
dotnet test SystemInfoCollector.sln -c Release
```

Esperado: build sem warnings (Princípio I) e todos os testes passando.

## Executar

```powershell
.\src\SystemInfoCollector\bin\Release\net48\SystemInfoCollector.exe
```

## Cenários de validação

Antes de começar, remova coletas anteriores:

```powershell
Remove-Item "$env:LOCALAPPDATA\SystemInfoCollector" -Recurse -ErrorAction SilentlyContinue
```

| # | Passo | Resultado esperado | Ref. |
|---|---|---|---|
| 1 | Abrir o app | Nome do autor visível; "Exibir dados" desabilitado | FR-001, FR-010 |
| 2 | Clicar "Coletar dados" | Progresso aparece, botão de coleta desabilita, termina em ≤ 5 s; status mostra caminho do arquivo; "Exibir dados" habilita | FR-009, FR-012, SC-001 |
| 3 | Abrir o `system-info.json` no editor | JSON indentado conforme [contracts/system-info-file.md](contracts/system-info-file.md) | FR-008 |
| 4 | Clicar "Exibir dados" | Janela com seções conforme [contracts/ui-contract.md](contracts/ui-contract.md) e valores iguais ao arquivo | FR-011 |
| 5 | Conferir com o Windows: `winver`, Configurações > Sistema > Sobre, `ipconfig /all`, `Get-NetAdapter` (interfaces com status Up), Gerenciador de Dispositivos | Valores coincidem | SC-003 |
| 6 | Conferir .NET: `(Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full').Release` | Release igual ao exibido; versão conforme tabela em [research.md](research.md#r5-versão-do-net-framework-pelo-registro) | FR-005 |
| 7 | Fechar e reabrir o app | "Exibir dados" já habilitado | FR-010, SC-004 |
| 8 | Com o app aberto, apagar o arquivo e clicar "Exibir dados" | Mensagem amigável; botão desabilita; app não fecha | Edge case, SC-004 |
| 9 | Coletar, corromper o JSON (apagar uma `}`) e clicar "Exibir dados" | Mensagem de arquivo corrompido; app não fecha | Edge case, SC-005 |
| 10 | Desativar todos os adaptadores de rede e coletar | Seção de rede mostra "Nenhuma interface encontrada" | US3 |
