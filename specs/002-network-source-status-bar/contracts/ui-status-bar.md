# Contract: Mensagens da janela principal

Substitui a seção "Mensagens" e a linha "Status" de
[001/contracts/ui-contract.md](../../001-system-info-collector/contracts/ui-contract.md).
Os demais elementos da janela principal e a janela "Dados Coletados" não mudam.

## Layout

```text
┌ Coletor de Informações do Sistema ─────────────┐
│ Adriano Olivares Barbosa                        │
│ [ Coletar dados ]                               │
│ [ Exibir dados  ]                               │
│ ▬▬▬▬▬▬ (progresso, só durante a coleta)         │
├─────────────────────────────────────────────────┤
│ ⓘ Dados salvos em C:\Users\...\system-info.json │  ← StatusBar (DockPanel.Dock=Bottom)
└─────────────────────────────────────────────────┘
```

- Ícone + texto com `TextTrimming="CharacterEllipsis"` e `ToolTip` com o texto completo.
- Cor e ícone conforme `StatusKind` ([data-model.md](../data-model.md#statuskind-novo-somente-em-memória)).

## Destino de cada mensagem

| Evento | Barra (tipo: texto) | Diálogo |
|---|---|---|
| Abertura da janela | Information: "Pronto." | — |
| Coleta iniciada | Information: "Coletando dados..." | — |
| Coleta concluída sem avisos | Information: "Dados salvos em {caminho}" | — |
| Coleta concluída com avisos | Warning: "Coleta concluída com 1 aviso" / "com N avisos" | Aviso: "Coleta concluída com avisos:" + lista |
| Falha ao salvar (`IOException`, `UnauthorizedAccessException`) | Error: "Não foi possível salvar os dados." | Erro: "Não foi possível salvar os dados em {caminho}: {motivo}" |
| Exibir: arquivo ausente | Error: "O arquivo de dados não existe mais. Colete novamente." | — |
| Exibir: arquivo corrompido | Error: "O arquivo de dados está corrompido. Colete novamente." | — |
| Exibir: falha de leitura (`IOException`, `UnauthorizedAccessException`) | Error: "Não foi possível ler os dados." | Erro: "Não foi possível ler {caminho}: {motivo}" |
| Exibir: sucesso | (inalterada) | Janela "Dados Coletados" |
| Erro inesperado (handler global) | (inalterada) | Erro: "Ocorreu um erro inesperado: {mensagem}" |

## IDialogService (após a feature)

```text
IDialogService   void ShowWarning(string message)
                 void ShowError(string message)
                 void ShowData(SystemInfo systemInfo)
```

`ShowInformation` é removido: nenhuma mensagem informativa usa mais diálogo.
