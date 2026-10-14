# System Info Collector Constitution

## Core Principles

### I. Clareza e Convenções C# (INEGOCIÁVEL)

Clareza do código e uso correto das convenções de codificação em C# são a prioridade máxima
do projeto. Portanto:

- Todo código MUST seguir as convenções de nomenclatura da Microsoft: `PascalCase` para tipos,
  métodos, propriedades e constantes; `camelCase` para variáveis locais e parâmetros;
  `_camelCase` para campos privados; interfaces com prefixo `I`.
- Nomes MUST ser descritivos e em inglês para identificadores de código. Textos exibidos ao
  usuário podem ser em português.
- Cada classe MUST ficar em seu próprio arquivo, com namespace coerente com a pasta.
- Métodos MUST ter uma única responsabilidade e ser curtos. Números e strings mágicos (nomes de
  classes WMI, chaves de registro, caminho do arquivo) MUST virar constantes nomeadas.
- Objetos `IDisposable` (`ManagementObjectSearcher`, `ManagementObject`, `RegistryKey`, streams)
  MUST ser liberados com `using`.
- Comentários MUST explicar o *porquê*, não o *o quê*. Membros públicos SHOULD ter
  comentários XML (`///`).
- O projeto MUST compilar sem warnings.

### II. Fidelidade aos Requisitos

- Todos os requisitos obrigatórios MUST ser atendidos integralmente: label com o nome do
  autor; botão "Coletar dados"; WMI `Win32_OperatingSystem` (Caption, Version,
  OSArchitecture, CSName); WMI `Win32_Processor` (Name, Description); versão do .NET Framework
  pelo registro; gravação em arquivo; botão "Exibir dados" habilitado/visível somente quando o
  arquivo existir; publicação no GitHub.
- Funcionalidades opcionais (WPF, interfaces de rede via WMI com nome/IP/MAC, serialização
  JSON) MAY ser implementadas, mas MUST NOT comprometer nem substituir silenciosamente um
  requisito obrigatório.
- O estado do botão "Exibir dados" MUST refletir a existência real do arquivo, inclusive ao
  abrir a aplicação com um arquivo já gerado anteriormente.

### III. Separação de Responsabilidades

- A coleta de dados (WMI e registro), a persistência (gravar/ler arquivo) e a apresentação
  (janelas/forms) MUST ficar em classes distintas.
- O code-behind da UI MUST NOT conter consultas WMI, acesso ao registro ou I/O de arquivo
  diretamente; ele delega a serviços.
- Os dados coletados MUST ser representados por classes de modelo tipadas (ex.:
  `OperatingSystemInfo`, `ProcessorInfo`, `NetworkInterfaceInfo`, `SystemInfo`), não por
  strings soltas ou dicionários.
- Serviços SHOULD ser expostos por interfaces para permitir substituição e testes.

**Justificativa**: mantém o código legível, coeso e fácil de manter.

### IV. Robustez na Coleta de Dados

- Falhas de WMI (`ManagementException`), de registro (chave ausente, acesso negado) e de I/O
  MUST ser tratadas e comunicadas ao usuário com mensagem clara, sem derrubar a aplicação.
- Valores ausentes ou nulos MUST ser tratados explicitamente (ex.: "Não disponível"), nunca
  gerar `NullReferenceException`.
- A detecção da versão do .NET Framework MUST usar a chave
  `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full` (valor `Release`) mapeada para a
  versão legível conforme a tabela oficial da Microsoft, abrindo o registro na view de 64 bits
  quando aplicável para funcionar tanto em "Any CPU" quanto em "x64".
- Operações potencialmente lentas (consultas WMI) SHOULD rodar fora da thread de UI
  (`async`/`await` com `Task.Run`), mantendo a interface responsiva e evitando cliques
  duplicados durante a coleta.

### V. Simplicidade

- A solução MUST ter o tamanho do problema: sem camadas, frameworks ou abstrações que não
  resolvam uma necessidade concreta dos requisitos (YAGNI).
- Dependências externas MUST ser mínimas e justificadas (ex.: Newtonsoft.Json via NuGet para
  a serialização JSON é aceitável).
- Qualquer complexidade adicional MUST ser justificada no plano de implementação.

## Restrições Técnicas

- Framework: .NET Framework 4.5.2 ou superior (recomendado 4.8), projeto no formato
  compatível com Visual Studio.
- UI: Windows Forms ou WPF (.NET Framework).
- Plataforma de destino: "Any CPU" ou "x64".
- APIs: `System.Management` para WMI; `Microsoft.Win32.Registry` para o registro.
- Arquivo de saída: caminho definido em um único ponto (constante ou configuração), gravado em
  local com permissão de escrita garantida (ex.: pasta do executável ou `%LocalAppData%`), com
  codificação UTF-8.
- Formato: texto puro ou JSON. Se JSON for adotado, a leitura MUST desserializar para os
  modelos tipados e exibir os dados formatados.

## Fluxo de Desenvolvimento e Entrega

- O repositório MUST conter um `.gitignore` adequado a Visual Studio/.NET (sem `bin/`, `obj/`,
  `.vs/`).
- O repositório MUST conter um `README.md` com: objetivo, funcionalidades, pré-requisitos e
  instruções de build e execução.
- Commits SHOULD ser pequenos, com mensagens descritivas.
- Antes da entrega, MUST ser verificado: build limpo em Release, execução manual do fluxo
  completo (coletar → arquivo criado → botão habilitado → exibir dados) e conferência dos
  dados contra o sistema real.
- Testes unitários SHOULD cobrir a lógica não dependente de UI (mapeamento da versão do .NET
  pelo valor `Release`, serialização/desserialização, formatação).

## Governance

- Esta constituição prevalece sobre outras práticas no escopo deste projeto. Especificações,
  planos e tarefas gerados pelo Spec Kit MUST estar em conformidade com ela.
- Emendas MUST ser feitas via `/speckit-constitution`, com Sync Impact Report e incremento de
  versão semântico: MAJOR para remoção/redefinição de princípios; MINOR para novo princípio ou
  seção; PATCH para ajustes de redação.
- Toda revisão de código e o `/speckit-analyze` MUST verificar conformidade com os princípios;
  violações MUST ser corrigidas ou justificadas explicitamente no plano.

**Version**: 1.0.1 | **Ratified**: 2026-10-14 | **Last Amended**: 2026-10-14
