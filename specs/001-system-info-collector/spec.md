# Feature Specification: Coletor de Informações do Sistema

**Feature Branch**: `001-system-info-collector`

**Created**: 2026-10-14

**Status**: Draft

**Input**: User description: "Aplicação desktop para Windows que identifica o autor, coleta informações
do computador local (sistema operacional, processador, versão do .NET Framework e, opcionalmente,
interfaces de rede), salva o resultado em um arquivo e permite exibir os dados salvos em tela."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Coletar e salvar as informações do computador (Priority: P1)

O usuário abre a aplicação, vê o nome do autor na tela e clica em "Coletar dados". A aplicação
levanta as informações do computador local (sistema operacional, processador e versão do .NET
Framework instalada) e as grava em um arquivo. Ao final, o usuário recebe a confirmação de que a
coleta foi concluída e onde o arquivo foi salvo.

**Why this priority**: é o núcleo da aplicação; sem a coleta e o arquivo não existe nada para
exibir.

**Independent Test**: abrir a aplicação, clicar em "Coletar dados" e verificar que o arquivo foi
criado contendo todos os campos esperados com valores condizentes com o computador.

**Acceptance Scenarios**:

1. **Given** a aplicação aberta, **When** o usuário olha a tela principal, **Then** vê um texto
   com o nome do autor e o botão "Coletar dados".
2. **Given** a aplicação aberta, **When** o usuário clica em "Coletar dados", **Then** o arquivo
   é criado contendo: nome, versão e arquitetura do sistema operacional; nome do computador; nome
   e descrição do processador; versão do .NET Framework instalada.
3. **Given** um arquivo já existente de coleta anterior, **When** o usuário coleta novamente,
   **Then** o arquivo é substituído pelos dados atuais.
4. **Given** uma coleta em andamento, **When** o usuário tenta clicar novamente em "Coletar
   dados", **Then** uma nova coleta não é iniciada até a atual terminar e a tela continua
   responsiva.

---

### User Story 2 - Exibir os dados salvos (Priority: P2)

Depois que o arquivo existe, o usuário clica em "Exibir dados" e a aplicação lê o arquivo e
mostra as informações de forma organizada e legível, separadas por categoria.

**Why this priority**: completa o ciclo coleta → persistência → consulta, mas depende de o
arquivo existir.

**Independent Test**: com um arquivo de coleta presente, abrir a aplicação, clicar em "Exibir
dados" e conferir que o conteúdo exibido corresponde ao arquivo.

**Acceptance Scenarios**:

1. **Given** que o arquivo de coleta não existe, **When** a aplicação é aberta, **Then** o botão
   "Exibir dados" está desabilitado.
2. **Given** que a coleta acabou de criar o arquivo, **When** a coleta termina, **Then** o botão
   "Exibir dados" fica habilitado imediatamente, sem reiniciar a aplicação.
3. **Given** que o arquivo já existia de uma execução anterior, **When** a aplicação é aberta,
   **Then** o botão "Exibir dados" já está habilitado.
4. **Given** o botão habilitado, **When** o usuário clica em "Exibir dados", **Then** uma tela
   mostra os dados lidos do arquivo, agrupados em Sistema Operacional, Processador e .NET Framework,
   com rótulos legíveis (a seção de Interfaces de Rede é acrescentada pela User Story 3).

---

### User Story 3 - Incluir as interfaces de rede (Priority: P3)

Na mesma coleta, a aplicação também levanta as interfaces de rede do computador, com nome,
endereço(s) IP e endereço MAC de cada uma, e os exibe junto com os demais dados.

**Why this priority**: funcionalidade opcional que enriquece o resultado, sem ser essencial para
o fluxo principal.

**Independent Test**: executar a coleta em um computador com ao menos uma interface de rede ativa
e verificar que nome, IP e MAC aparecem no arquivo e na exibição, conferindo com as configurações
de rede do Windows.

**Acceptance Scenarios**:

1. **Given** um computador com interfaces de rede ativas, **When** o usuário coleta os dados,
   **Then** cada interface com IP configurado aparece com nome, endereço(s) IP e endereço MAC.
2. **Given** uma interface com mais de um IP (ex.: IPv4 e IPv6), **When** os dados são exibidos,
   **Then** todos os endereços daquela interface aparecem.
3. **Given** um computador sem nenhuma interface com IP configurado, **When** os dados são
   exibidos, **Then** a seção de rede informa que nenhuma interface foi encontrada.

---

### User Story 4 - Arquivo em formato estruturado (Priority: P3)

Em vez de texto livre, os dados são gravados em um formato estruturado e padronizado, legível por
pessoas e por outros programas. Na exibição, a aplicação interpreta o arquivo de volta para os
dados originais e os apresenta formatados.

**Why this priority**: funcionalidade opcional que melhora a interoperabilidade e garante que a
leitura reconstrói exatamente o que foi coletado.

**Independent Test**: coletar, abrir o arquivo em um editor de texto e confirmar que é um
documento estruturado válido e legível; depois exibir pela aplicação e confirmar que os valores
batem com o arquivo.

**Acceptance Scenarios**:

1. **Given** uma coleta concluída, **When** o arquivo é aberto em um editor de texto, **Then** o
   conteúdo está em formato estruturado padrão, indentado e com nomes de campos descritivos.
2. **Given** um arquivo estruturado válido, **When** o usuário clica em "Exibir dados", **Then**
   todos os valores exibidos são idênticos aos gravados.

---

### Edge Cases

- **Arquivo corrompido ou editado manualmente**: ao exibir, a aplicação informa que o arquivo não
  pôde ser lido e sugere coletar novamente, sem fechar.
- **Arquivo apagado após a abertura da aplicação**: ao clicar em "Exibir dados", a aplicação
  informa que o arquivo não existe mais e desabilita o botão.
- **Falha ao consultar uma das fontes de informação** (serviço indisponível, permissão negada):
  a coleta continua com as demais fontes; o campo afetado é registrado como "Não disponível" e o
  usuário é avisado.
- **.NET Framework 4.5 ou superior não detectado**: o campo indica "Não detectado (anterior à
  4.5)".
- **Versão do .NET Framework mais nova que as conhecidas**: o campo mostra a versão conhecida mais
  alta acompanhada do identificador numérico bruto (ex.: "4.8.1 ou superior (release 533509)").
- **Sem permissão de escrita no local do arquivo**: a aplicação informa o erro e o botão
  "Exibir dados" permanece no estado correspondente à existência do arquivo.
- **Computador com múltiplos processadores físicos**: todos são listados.
- **Interface de rede sem endereço MAC** (ex.: adaptadores virtuais): o campo aparece como
  "Não disponível".

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A tela principal MUST exibir um texto com o nome do autor da aplicação.
- **FR-002**: A tela principal MUST ter um botão "Coletar dados".
- **FR-003**: Ao acionar "Coletar dados", o sistema MUST coletar do sistema operacional: nome
  (ex.: "Microsoft Windows 11 Pro"), versão, arquitetura (32/64 bits) e nome do computador.
- **FR-004**: Ao acionar "Coletar dados", o sistema MUST coletar de cada processador: nome e
  descrição.
- **FR-005**: Ao acionar "Coletar dados", o sistema MUST identificar a versão do .NET Framework
  instalada no Windows, apresentada em forma legível (ex.: "4.8"), a partir das configurações
  do próprio sistema.
- **FR-006**: Ao acionar "Coletar dados", o sistema MUST coletar as interfaces de rede com
  endereço IP configurado, com nome, endereço(s) IP e endereço MAC de cada uma.
- **FR-007**: O sistema MUST gravar o resultado da coleta em um arquivo de texto em local fixo e
  previsível, substituindo coletas anteriores.
- **FR-008**: O arquivo MUST usar um formato estruturado padrão, indentado e legível, que permita
  reconstruir os dados coletados integralmente na leitura.
- **FR-009**: Após uma coleta bem-sucedida, o sistema MUST informar ao usuário a conclusão e o
  caminho do arquivo.
- **FR-010**: A tela principal MUST ter um botão "Exibir dados" que fica desabilitado enquanto o
  arquivo não existir e habilitado assim que ele existir, inclusive ao iniciar a aplicação.
- **FR-011**: Ao acionar "Exibir dados", o sistema MUST ler o arquivo e mostrar os dados em uma
  tela própria, agrupados por categoria e com rótulos legíveis.
- **FR-012**: Durante a coleta, a interface MUST continuar responsiva e MUST impedir que uma
  segunda coleta seja iniciada em paralelo.
- **FR-013**: Falhas na coleta, gravação ou leitura MUST ser comunicadas com mensagem
  compreensível, sem encerrar a aplicação.
- **FR-014**: Valores que não puderem ser obtidos MUST aparecer como "Não disponível", sem
  impedir a coleta dos demais.

### Key Entities

- **Informações do Sistema**: o resultado completo de uma coleta; agrega as demais entidades e a
  data/hora em que a coleta foi feita.
- **Sistema Operacional**: nome, versão, arquitetura e nome do computador.
- **Processador**: nome e descrição; uma coleta pode ter um ou mais.
- **.NET Framework**: versão legível instalada e o identificador numérico de origem.
- **Interface de Rede**: nome, lista de endereços IP e endereço MAC; uma coleta pode ter zero ou
  mais.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em um computador comum, a coleta completa termina em até 5 segundos após o clique.
- **SC-002**: 100% dos campos exigidos (FR-003 a FR-006) aparecem no arquivo e na exibição,
  preenchidos ou marcados como "Não disponível".
- **SC-003**: Os valores exibidos coincidem com os que o Windows mostra em suas próprias telas
  (Configurações > Sistema > Sobre, Gerenciador de Dispositivos, configurações de rede) em 100%
  dos campos verificados.
- **SC-004**: O estado do botão "Exibir dados" está correto em 100% dos cenários: sem arquivo,
  arquivo recém-criado, arquivo pré-existente na abertura e arquivo apagado durante o uso.
- **SC-005**: Nenhum cenário de erro listado nos Edge Cases encerra a aplicação.
- **SC-006**: Um usuário sem treinamento consegue coletar e visualizar os dados em até 3 cliques
  e menos de 30 segundos.

## Assumptions

- O nome do autor exibido é "Adriano Olivares Barbosa".
- A aplicação roda em Windows com .NET Framework 4.5.2 ou superior; computadores sem esse
  requisito estão fora do escopo.
- A coleta é sempre do computador local; coleta remota está fora do escopo.
- O arquivo fica na pasta de dados do usuário da aplicação (ex.: `%LocalAppData%`), onde há
  permissão de escrita garantida, com nome fixo. Apenas a coleta mais recente é mantida, sem
  histórico.
- A versão do .NET Framework considerada é a da família 4.x, que é a relevante para a aplicação;
  versões 1.x a 3.5 não são listadas.
- Interfaces de rede sem nenhum endereço IP configurado (desconectadas ou desativadas) são
  ignoradas.
- A interface é em português; não há requisito de internacionalização.
- As funcionalidades opcionais (User Stories 3 e 4) fazem parte do escopo desta entrega.
- O código-fonte será publicado em um repositório no GitHub.
