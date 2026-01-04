# RoboCopy-X 🚀

<div align="center">

![RoboCopy-X](https://img.shields.io/badge/RoboCopy-X-blue?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=.net)
![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?style=for-the-badge&logo=windows)
![License](https://img.shields.io/badge/license-MIT-green?style=for-the-badge)

**Interface Gráfica Moderna para Robocopy**

Uma aplicação WinUI 3 moderna e intuitiva que torna o poderoso utilitário Robocopy do Windows acessível através de uma interface gráfica elegante e fácil de usar.

[Recursos](#-recursos) • [Instalação](#-instalação) • [Uso](#-uso-rápido) • [Documentação](#-documentação) • [Contribuir](#-contribuindo)

</div>

---

## 📖 Sobre

**RoboCopy-X** é uma interface gráfica moderna para o Robocopy, a ferramenta de linha de comando do Windows para cópia robusta de arquivos. Desenvolvido com WinUI 3 e .NET 8, oferece uma experiência de usuário fluida com design Fluent do Windows 11.

### 🎯 Por que usar RoboCopy-X?

- ✅ **Interface Intuitiva**: Esqueça comandos complexos - use drag & drop e checkboxes
- ✅ **Design Moderno**: Fluent Design com suporte a temas e efeito Mica
- ✅ **Seguro**: Validações avançadas previnem erros e perda de dados
- ✅ **Poderoso**: Acesso total aos recursos do Robocopy com visualização em tempo real
- ✅ **Transparente**: Veja o comando gerado e logs detalhados de cada operação
- ✅ **Open Source**: Código aberto e gratuito

---

## ✨ Recursos

### 🎨 Interface Moderna

<table>
<tr>
<td width="50%">

**Design Fluent**
- Interface WinUI 3 nativa
- Efeito Mica para transparência
- Suporte a temas claro/escuro/automático
- Cards organizados e Expanders
- Animações fluidas

</td>
<td width="50%">

**Responsiva e Acessível**
- Alto contraste em ambos os temas
- Tooltips descritivos
- Navegação por teclado
- Labels semânticos
- Design adaptativo

</td>
</tr>
</table>

### 📁 Seleção de Arquivos

- 🎯 **Drag & Drop**: Arraste pastas direto do Explorer
- 🖱️ **Seletor Visual**: Navegador de pastas integrado
- ✔️ **Validação Inteligente**: Previne erros comuns automaticamente
- 🔍 **Preview de Caminhos**: Veja os caminhos completos antes de executar

### ⚙️ Opções Avançadas

<details>
<summary><b>📋 Opções de Cópia</b></summary>

- Copiar subdiretórios incluindo vazios (`/E`)
- Modo espelho com aviso de segurança (`/MIR`)
- Excluir arquivos mais antigos (`/XO`)
- Escolha de comportamento em conflitos

</details>

<details>
<summary><b>🏷️ Atributos Personalizados</b></summary>

Escolha exatamente o que copiar:
- 📄 Dados dos arquivos
- 🔖 Atributos (somente leitura, oculto, etc.)
- 🕒 Timestamps (criação, modificação)
- 🔒 Segurança NTFS (ACLs)
- 👤 Informações de proprietário
- 📊 Informações de auditoria

</details>

<details>
<summary><b>⚡ Desempenho</b></summary>

- **Multi-threading**: Até 128 threads
- **Detecção Automática**: Recomendações baseadas no hardware
- **Otimização Inteligente**: Configurações ideais para cada cenário
- **Retry Configurável**: 1-100 tentativas em caso de falha
- **Wait Time**: 1s a 10min entre tentativas

</details>

### 📊 Monitoramento em Tempo Real

- 📈 **Barra de Progresso**: Acompanhe o andamento da operação
- ⏱️ **Tempo Restante**: Estimativa baseada em desempenho real
- 📄 **Arquivo Atual**: Veja qual arquivo está sendo copiado
- 📋 **Logs Detalhados**: Todos os eventos registrados automaticamente
- 💾 **Espaço em Disco**: Verificação automática antes de iniciar

### 🛡️ Validações de Segurança

O RoboCopy-X implementa múltiplas camadas de validação:

1. ✅ **Caminhos Diferentes**: Origem e destino não podem ser iguais
2. ✅ **Hierarquia Válida**: Destino não pode estar dentro da origem
3. ✅ **Permissões**: Verifica leitura (origem) e escrita (destino)
4. ✅ **Pastas do Sistema**: Aviso ao usar pastas críticas do Windows
5. ✅ **Conflitos**: Detecta arquivos existentes e permite escolha
6. ✅ **Espaço em Disco**: Verifica disponibilidade antes de iniciar

### 🔐 Modo Administrador

- 🔓 **Ativação Simples**: Toggle para reiniciar com privilégios elevados
- 🔒 **Indicação Visual**: Badge mostra quando está ativo
- ⚠️ **Segurança**: Confirmação obrigatória antes de elevar privilégios

---

## 💾 Instalação

### Requisitos

| Componente | Versão Mínima | Recomendado |
|------------|---------------|-------------|
| **Sistema Operacional** | Windows 10 1809 (17763) | Windows 11 22H2 |
| **Runtime** | .NET 8.0 | .NET 8.0 (latest) |
| **Memória RAM** | 2 GB | 4 GB+ |
| **Espaço em Disco** | 100 MB | 200 MB |

### Método 1: Download Direto (Recomendado)

1. Acesse a página de [Releases](https://github.com/FM0Ura/RoboCopy-X/releases)
2. Baixe a versão mais recente (`RoboCopy-X-v1.0.0.zip`)
3. Extraia o arquivo ZIP
4. Execute `RoboCopy-X.exe`

### Método 2: Compilar do Código-Fonte

```powershell
# Clone o repositório
git clone https://github.com/FM0Ura/RoboCopy-X.git
cd RoboCopy-X

# Restaurar dependências
dotnet restore

# Compilar
dotnet build -c Release

# Executar
dotnet run --project RoboCopy-X/RoboCopy-X.csproj
```

### Método 3: Publicar como Executável Único

```powershell
# Publicar para Windows 64-bit (standalone)
dotnet publish RoboCopy-X/RoboCopy-X.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true

# O executável estará em:
# bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\RoboCopy-X.exe
```

---

## 🚀 Uso Rápido

### 1️⃣ Selecionar Pastas

**Opção A: Drag & Drop**
- Arraste a pasta de origem para a área "Pasta de Origem"
- Arraste a pasta de destino para a área "Pasta de Destino"

**Opção B: Seletor de Pastas**
- Clique na área de origem/destino
- Navegue até a pasta desejada
- Confirme a seleção

### 2️⃣ Configurar Opções

```
📂 Opções de Cópia
  ☐ Copiar subdiretórios incluindo vazios
  ☐ Espelhar árvore de diretórios (⚠️ Cuidado!)
  ☐ Excluir arquivos mais antigos

⚡ Desempenho
  ☑ Multi-threading (8 threads recomendado)
  
🔄 Tentativas
  • 10 tentativas
  • 5 segundos entre tentativas
```

### 3️⃣ Executar

1. Clique em **"Executar"**
2. Acompanhe o progresso em tempo real
3. Revise os logs após conclusão
4. Opcionalmente, abra a pasta de destino

### 💡 Dicas de Uso

| Cenário | Configuração Recomendada |
|---------|-------------------------|
| **Backup Completo** | ✅ Subdiretórios + Multi-thread |
| **Sincronização** | ✅ Modo Espelho (com cuidado!) |
| **Muitos Arquivos Pequenos** | ✅ Multi-thread (16+ threads) |
| **Arquivos Grandes** | ✅ Multi-thread (4-8 threads) |
| **Rede Lenta** | ✅ Mais tentativas + Maior wait time |
| **Primeiro Uso** | ✅ Confirmação antes de executar |

---

## 📚 Documentação

### Arquitetura do Projeto

```
RoboCopy-X/
├── 📁 Services/              # Lógica de negócios
│   ├── RobocopyExecutor.cs       # Execução do Robocopy
│   ├── RobocopyCommandBuilder.cs # Construção de comandos
│   ├── UIService.cs              # Gerenciamento de UI
│   ├── ProgressTracker.cs        # Rastreamento de progresso
│   ├── DragDropHandler.cs        # Drag & drop
│   ├── LogService.cs             # Sistema de logs
│   ├── DiskSpaceService.cs       # Verificação de espaço
│   └── SettingsService.cs        # Configurações
├── 📁 Helpers/               # Utilitários
│   ├── PathValidator.cs          # Validação de caminhos
│   ├── AdminPrivilegeHelper.cs   # Privilégios admin
│   ├── StyledDialogHelper.cs     # Diálogos estilizados
│   └── ThreadOptionsHelper.cs    # Opções de threads
├── 📁 Models/                # Modelos de dados
│   ├── RobocopyOptions.cs        # Opções do Robocopy
│   └── AppSettings.cs            # Configurações da app
├── 📁 Docs/                  # Documentação técnica
│   ├── LoggingSystem.md          # Sistema de logging
│   ├── ProgressSystem.md         # Sistema de progresso
│   ├── AdminMode.md              # Modo administrador
│   └── REFACTORING_SUMMARY.md    # Resumo da refatoração
├── 📄 MainWindow.xaml       # Interface principal
├── 📄 MainWindow.xaml.cs    # Code-behind
├── 📄 App.xaml              # Configuração da aplicação
├── 📁 Assets/               # Recursos visuais
└── 📁 logs/                 # Logs automáticos (ignorado no Git)
```

### Documentação Detalhada

| Documento | Descrição |
|-----------|-----------|
| [📋 Sistema de Logging](RoboCopy-X/Docs/LoggingSystem.md) | Como funcionam os logs automáticos |
| [📊 Sistema de Progresso](RoboCopy-X/Docs/ProgressSystem.md) | Rastreamento de operações |
| [🔐 Modo Administrador](RoboCopy-X/Docs/AdminMode.md) | Privilégios elevados |
| [🎨 Guia de Contribuição](CONTRIBUTING.md) | Padrões de código e estilo |

### Códigos de Saída do Robocopy

| Código | Significado |
|--------|-------------|
| 0 | Nenhum arquivo copiado, nenhum erro |
| 1 | ✅ Todos os arquivos copiados com sucesso |
| 2 | Arquivos adicionais no destino |
| 3 | Alguns arquivos copiados + adicionais |
| 4-7 | Alguns arquivos incompatíveis |
| 8 | ❌ Várias falhas durante cópia |
| 16+ | ❌ Erro fatal |

---

## 🛠️ Tecnologias

<table>
<tr>
<td align="center" width="25%">
<img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=.net&logoColor=white" />
<br /><b>.NET 8</b>
<br />Framework moderno
</td>
<td align="center" width="25%">
<img src="https://img.shields.io/badge/WinUI-3-0078D4?style=for-the-badge&logo=windows&logoColor=white" />
<br /><b>WinUI 3</b>
<br />UI nativa do Windows
</td>
<td align="center" width="25%">
<img src="https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=c-sharp&logoColor=white" />
<br /><b>C# 12</b>
<br />Linguagem moderna
</td>
<td align="center" width="25%">
<img src="https://img.shields.io/badge/Fluent-Design-0078D4?style=for-the-badge&logo=microsoft&logoColor=white" />
<br /><b>Fluent Design</b>
<br />Design System
</td>
</tr>
</table>

### Principais Bibliotecas

- **Microsoft.WindowsAppSDK** - APIs modernas do Windows
- **Microsoft.UI.Xaml** - Framework de UI
- **System.Text.RegularExpressions** - Parsing de saída do Robocopy
- **Windows.Storage** - Acesso a arquivos e pastas

---

## 🤝 Contribuindo

Contribuições são muito bem-vindas! 🎉

### Como Contribuir

1. **Fork** o projeto
2. Crie uma **branch** para sua feature
   ```bash
   git checkout -b feature/MinhaNovaFeature
   ```
3. **Commit** suas mudanças seguindo o padrão semântico
   ```bash
   git commit -m "Add: Nova funcionalidade X"
   ```
4. **Push** para a branch
   ```bash
   git push origin feature/MinhaNovaFeature
   ```
5. Abra um **Pull Request**

### Diretrizes de Contribuição

📖 **Leia primeiro**: [CONTRIBUTING.md](CONTRIBUTING.md)

**Commits Semânticos:**
- `Add:` - Nova funcionalidade
- `Fix:` - Correção de bug
- `Docs:` - Documentação
- `Style:` - Formatação
- `Refactor:` - Refatoração
- `Test:` - Testes
- `Chore:` - Manutenção

**Código:**
- ✅ Siga o estilo de código do projeto
- ✅ Adicione comentários XML em métodos públicos
- ✅ Teste suas mudanças localmente
- ✅ Atualize a documentação se necessário

---

## 🗺️ Roadmap

### 📅 Versão 1.1 (Em Breve)

- [ ] 🔐 **Validação HASH**: Verificação de integr
- [ ] 💾 **Perfis Salvos**: Salve configurações favoritas
- [ ] 📜 **Histórico**: Veja operações anteriores
- [ ] 📊 **Estatísticas**: Gráficos de desempenho
- [ ] 🎨 **Temas Customizados**: Crie seu próprio tema

### 💡 Ideias em Consideração

- Integração com OneDrive/Google Drive
- Comparação de pastas antes de copiar
- Gerador de scripts batch
- Modo portable (sem instalação)

**Tem uma ideia?** [Abra uma discussão](https://github.com/FM0Ura/RoboCopy-X/discussions)!

---

## 📄 Licença

Este projeto está licenciado sob a **Licença MIT** - veja o arquivo [LICENSE](LICENSE) para detalhes.

```
MIT License

Copyright (c) 2025 RoboCopy-X Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software")...
```

---

## 📞 Suporte e Contato

### 🐛 Encontrou um Bug?

[Abra uma issue](https://github.com/FM0Ura/RoboCopy-X/issues/new?template=bug_report.md) com:
- Descrição do problema
- Passos para reproduzir
- Screenshots (se aplicável)
- Informações do sistema

### 💡 Tem uma Sugestão?

[Abra uma issue](https://github.com/FM0Ura/RoboCopy-X/issues/new?template=feature_request.md) ou [discussão](https://github.com/FM0Ura/RoboCopy-X/discussions) com sua ideia!

### 💬 Precisa de Ajuda?

- 📖 Consulte a [Wiki](https://github.com/FM0Ura/RoboCopy-X/wiki)
- 💬 Participe das [Discussões](https://github.com/FM0Ura/RoboCopy-X/discussions)
- 📧 Entre em contato: [Issues](https://github.com/FM0Ura/RoboCopy-X/issues)

---

## ⭐ Mostre seu Apoio

Se este projeto foi útil para você, considere:

- ⭐ Dar uma **estrela** no GitHub
- 🐦 **Compartilhar** nas redes sociais
- 🤝 **Contribuir** com código ou documentação
- 💬 **Reportar bugs** ou sugerir melhorias

---

<div align="center">

**Desenvolvido com ❤️ usando WinUI 3 e .NET 8**

[![GitHub](https://img.shields.io/badge/GitHub-FM0Ura-181717?style=for-the-badge&logo=github)](https://github.com/FM0Ura/RoboCopy-X)

[⬆ Voltar ao topo](#robocopy-x-)

</div>
