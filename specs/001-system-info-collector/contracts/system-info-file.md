# Contract: Arquivo de coleta (`system-info.json`)

- **Local**: `%LocalAppData%\SystemInfoCollector\system-info.json`
- **Codificação**: UTF-8 sem BOM
- **Formato**: JSON indentado (2 espaços), propriedades em camelCase
- **Produtor**: `SystemInfoRepository.Save` | **Consumidor**: `SystemInfoRepository.Load`
- **Compatibilidade**: propriedades desconhecidas são ignoradas na leitura; propriedades ausentes
  assumem `null` / lista vazia.

## Exemplo

```json
{
  "collectedAt": "2026-10-14T10:32:05.123-03:00",
  "operatingSystem": {
    "caption": "Microsoft Windows 11 Pro",
    "version": "10.0.26300",
    "architecture": "64 bits",
    "computerName": "DESKTOP-ABC123"
  },
  "processors": [
    {
      "name": "AMD Ryzen 7 5800X 8-Core Processor",
      "description": "AMD64 Family 25 Model 33 Stepping 0"
    }
  ],
  "netFramework": {
    "version": "4.8.1 ou superior",
    "release": 533509
  },
  "networkInterfaces": [
    {
      "name": "Ethernet",
      "description": "Intel(R) Ethernet Connection I219-V",
      "ipAddresses": [ "192.168.0.10", "fe80::1c2b:3d4e:5f60:7182" ],
      "macAddress": "00:1A:2B:3C:4D:5E"
    }
  ],
  "warnings": []
}
```

## Regras de validação na leitura

| Situação | Resultado |
|---|---|
| Arquivo não existe | `FileNotFoundException` → mensagem "O arquivo de dados não existe mais. Colete novamente." e botão desabilitado |
| JSON malformado ou raiz não-objeto | `InvalidDataException` → mensagem "O arquivo de dados está corrompido. Colete novamente." |
| Sem permissão de leitura | `UnauthorizedAccessException` → mensagem com o caminho |
| Campo string `null` | Exibido como "Não disponível" |
