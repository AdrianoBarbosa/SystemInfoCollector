# Contract: Interface do usuário

## MainWindow — "Coletor de Informações do Sistema"

| Elemento | Tipo | Binding / Comando | Comportamento |
|---|---|---|---|
| Nome do autor | `TextBlock` (label) | texto fixo "Adriano Olivares Barbosa" | Sempre visível (FR-001) |
| Coletar dados | `Button` | `CollectDataCommand` | Desabilitado enquanto `IsBusy` (FR-012) |
| Exibir dados | `Button` | `ShowDataCommand` | Habilitado somente se `CanShowData` (FR-010) |
| Indicador de progresso | `ProgressBar` indeterminada | `IsBusy` | Visível só durante a coleta |
| Status | `TextBlock` | `StatusMessage` | Ex.: "Dados salvos em C:\...\system-info.json" (FR-009) |

**Mensagens (via `IDialogService`)**:

> **Substituído pela feature 002**: mensagens curtas passaram para a barra de status e o diálogo
> ficou reservado para mensagens com detalhes. Ver
> [002/contracts/ui-status-bar.md](../../002-network-source-status-bar/contracts/ui-status-bar.md).

- Coleta com avisos (`Warnings` não vazio): aviso listando as fontes que falharam.
- Falha ao gravar: erro com motivo e caminho.
- Falha ao ler: ver [system-info-file.md](system-info-file.md).

## DataWindow — "Dados Coletados"

Janela modal aberta por `ShowDataCommand`, alimentada pelo `SystemInfo` desserializado.

| Seção (`GroupBox`) | Conteúdo |
|---|---|
| Coleta | Data/hora da coleta |
| Sistema Operacional | Nome, Versão, Arquitetura, Nome do computador |
| Processador(es) | Para cada um: Nome, Descrição |
| .NET Framework | Versão (e release numérico) |
| Interfaces de Rede | Para cada uma: Nome, Descrição, IP(s) (um por linha), MAC; ou "Nenhuma interface encontrada" |
| Avisos | Exibida apenas se houver avisos |

- Valores `null` aparecem como "Não disponível".
- Botão "Fechar"; conteúdo com rolagem vertical; textos selecionáveis para cópia.

## Interfaces de serviço (contratos internos usados pelo ViewModel)

```text
ISystemInfoService      SystemInfo Collect()
ISystemInfoRepository   string FilePath { get; }
                        bool Exists()
                        void Save(SystemInfo info)
                        SystemInfo Load()
IDialogService          void ShowInformation(string message)
                        void ShowWarning(string message)
                        void ShowError(string message)
                        void ShowData(SystemInfo info)
```
