# RoboCopy-X ??

**Interface Gráfica Moderna para Robocopy**

Uma aplicação WinUI 3 moderna e intuitiva que torna o poderoso utilitário Robocopy acessível através de uma interface gráfica elegante e fácil de usar.

## ? Características Principais

### ?? Design Moderno e Intuitivo
- **Interface Fluent Design** com efeito Mica (Windows 11)
- **Cards organizados** para melhor hierarquia visual
- **Expanders** para organizar conteúdo de forma eficiente
- **Drag & Drop** intuitivo para pastas de origem e destino
- **Feedback visual** em todas as interações

### ?? Funcionalidades Completas

#### Caminhos de Origem e Destino
- **Drag and Drop** - Arraste pastas diretamente do Windows Explorer
- **Clique para selecionar** - Interface de seleção de pasta integrada
- **Feedback visual** durante o arraste
- **Validação automática** de caminhos

#### Parâmetros de Cópia
- **Opções de Cópia**
  - Copiar subdiretórios incluindo vazios (/E)
  - Modo espelho com aviso de segurança (/MIR)
  - Excluir arquivos mais antigos (/XO)

- **Atributos**
  - Dados, Atributos, Timestamps
  - Segurança NTFS, Proprietário, Auditoria
  - Seleção granular via checkboxes

- **Desempenho**
  - Multi-threading com detecção automática de núcleos
  - Seleção inteligente de threads (1-128)
  - Recomendações baseadas no sistema

#### Tentativas e Log
- **Tentativas configuráveis** (1, 10-100 em intervalos de 10)
- **Tempo de espera** (1s a 10min com labels inteligentes)
- **Log automático** - Todos os logs são salvos automaticamente em `RoboCopy-X/logs/`
- **Saída detalhada** opcional (/V)
- **Nomenclatura com timestamp** - `robocopy_log_YYYYMMDD_HHmmss.txt`

### ?? Sistema de Configurações

#### Aparência
- **Temas**: Escuro, Claro, ou Sistema
- **Efeito Mica**: Ative/desative conforme necessário
- **Aplicação instantânea** de mudanças

#### Comportamento
- **Auto-close de notificações** de sucesso
- **Confirmação antes de executar** (opcional)
- **Lembrar últimos caminhos** usados

#### Valores Padrão
- **Threads padrão** personalizáveis
- **Configurações iniciais** de cópia
- **Multi-thread ativo** por padrão

#### Sobre
- **Informações do sistema** detalhadas
- **Versão da aplicação**
- **Link para documentação**

### ?? Experiência do Usuário (UX)

#### Princípios de Design Aplicados

1. **Hierarquia Visual Clara**
   - Títulos com tamanhos e pesos apropriados
   - Espaçamento consistente (8px, 12px, 16px, 20px, 24px)
   - Agrupamento lógico de funcionalidades

2. **Affordances e Feedback**
   - Botões com estados hover/press/disabled
   - Animações sutis em transições
   - Feedback visual em drag & drop
   - Tooltips informativos em todos os controles

3. **Prevenção de Erros**
   - Validação de campos obrigatórios
   - Avisos para ações destrutivas (modo espelho)
   - Confirmação opcional antes de executar
   - Mensagens de erro claras e acionáveis

4. **Consistência**
   - Design system baseado em Fluent Design
   - Ícones do Segoe MDL2 Assets
   - Cores e espaçamentos padronizados
   - Linguagem consistente

5. **Eficiência**
   - Atalhos via drag & drop
   - Valores padrão inteligentes
   - Expanders para economizar espaço
   - Cards organizados por contexto

6. **Acessibilidade**
   - Alto contraste em ambos os temas
   - Tooltips descritivos
   - Navegação por teclado
   - Labels semânticos

### ?? Tecnologias Utilizadas

- **.NET 8** - Framework moderno e performático
- **WinUI 3** - UI framework nativo do Windows
- **Mica Backdrop** - Efeito moderno do Windows 11
- **Fluent Design** - Sistema de design da Microsoft
- **C# 12** - Linguagem moderna com recursos avançados

### ?? Requisitos

- Windows 10 version 1809 (build 17763) ou superior
- Windows 11 (recomendado para melhor experiência com Mica)
- .NET 8 Runtime

### ?? Capturas de Tela

#### Tema Claro
- Interface limpa e moderna
- Cards com sombras sutis
- Ícones bem definidos

#### Tema Escuro
- Redução de fadiga visual
- Efeito Mica integrado
- Contraste otimizado

### ?? Melhorias de UX/UI Implementadas

#### Layout
- ? Barra superior com título e botões de ação
- ? Uso de Expanders para melhor organização
- ? Cards com bordas arredondadas (8px)
- ? Padding e spacing consistentes
- ? Largura máxima de conteúdo (1200px)

#### Interatividade
- ? Drag & Drop com feedback visual
- ? Botões com tamanhos adequados (min 44px)
- ? Hover states bem definidos
- ? Progress bar integrado
- ? InfoBar para notificações contextuais

#### Tipografia
- ? Hierarquia clara (12-24px)
- ? Fonte Consolas para caminhos
- ? Pesos variados (Regular, SemiBold, Bold)
- ? Cores secundárias para texto auxiliar

#### Cores
- ? Accent color em ações primárias
- ? Cards com background diferenciado
- ? Bordas sutis em elementos
- ? Estados de erro em vermelho

### ?? Estrutura do Projeto

```
RoboCopy-X/
??? RoboCopy-X/
?   ??? Models/              # Modelos de dados
?   ?   ??? RobocopyOptions.cs
?   ?   ??? AppSettings.cs
?   ??? Services/            # Lógica de negócio
?   ?   ??? RobocopyCommandBuilder.cs
?   ?   ??? RobocopyExecutor.cs
?   ?   ??? LogService.cs
?   ?   ??? SettingsService.cs
?   ??? Helpers/             # Utilitários
?   ?   ??? ThreadOptionsHelper.cs
?   ?   ??? RetryOptionsHelper.cs
?   ?   ??? WaitTimeOptionsHelper.cs
?   ?   ??? PathValidator.cs
?   ??? MainWindow.xaml      # Interface principal
?   ??? MainWindow.xaml.cs   # Code-behind
?   ??? App.xaml             # Configuração da aplicação
?   ??? Assets/              # Recursos visuais
?   ??? logs/                # Logs automáticos das operações
?       ??? .gitignore       # Ignora arquivos de log no Git
??? ARCHITECTURE.md          # Documentação da arquitetura
??? README.md                # Este arquivo
??? .gitignore              # Arquivos ignorados no Git
```

### ??? Arquitetura

O projeto segue uma arquitetura modular bem definida com separação clara de responsabilidades:

- **Models**: Objetos de dados (POCO)
- **Services**: Lógica de negócio e operações
- **Helpers**: Funções utilitárias reutilizáveis
- **Views**: Interface do usuário (XAML)

Para mais detalhes sobre a arquitetura, consulte [ARCHITECTURE.md](ARCHITECTURE.md).

### ?? Roadmap Futuro

- [ ] Perfis salvos de configurações
- [ ] Histórico de operações
- [ ] Agendamento de tarefas
- [ ] Monitoramento em tempo real
- [ ] Estatísticas de transferência
- [ ] Suporte a múltiplos idiomas
- [ ] Tema personalizado

### ?? Licença

Este projeto é de código aberto. Sinta-se livre para usar, modificar e distribuir.

### ?? Contribuindo

Contribuições são bem-vindas! Por favor:
1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

### ?? Suporte

Para bugs, sugestões ou dúvidas, abra uma issue no GitHub.

---

**Desenvolvido com ?? usando WinUI 3 e .NET 8**
