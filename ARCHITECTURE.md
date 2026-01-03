# Arquitetura do RoboCopy-X

## ?? Estrutura de Pastas

```
RoboCopy-X/
??? Models/                    # Modelos de dados (POCO)
?   ??? RobocopyOptions.cs    # Opções de configuração do Robocopy
?   ??? AppSettings.cs        # Configurações da aplicação
?
??? Services/                  # Camada de serviços (lógica de negócio)
?   ??? RobocopyCommandBuilder.cs  # Construtor de comandos Robocopy
?   ??? RobocopyExecutor.cs        # Executor de processos Robocopy
?   ??? LogService.cs               # Gerenciamento de logs
?   ??? SettingsService.cs          # Persistência de configurações
?
??? Helpers/                   # Utilitários e funções auxiliares
?   ??? ThreadOptionsHelper.cs     # Gerador de opções de threads
?   ??? RetryOptionsHelper.cs      # Gerador de opções de tentativas
?   ??? WaitTimeOptionsHelper.cs   # Gerador de opções de tempo de espera
?   ??? PathValidator.cs            # Validador de caminhos
?
??? Views/                     # XAML Views (futuro)
?   ??? MainWindow.xaml
?
??? ViewModels/                # ViewModels MVVM (futuro)
?   ??? MainViewModel.cs
?
??? Converters/                # Value Converters (futuro)
?
??? logs/                      # Diretório de logs
    ??? .gitignore
```

## ??? Arquitetura

### Camadas

#### 1. **Models** (Camada de Dados)
Objetos simples de transferência de dados (DTO/POCO).

**Responsabilidades:**
- Definir estrutura de dados
- Sem lógica de negócio
- Propriedades públicas simples

**Arquivos:**
- `RobocopyOptions.cs` - Todas as opções do Robocopy
- `AppSettings.cs` - Configurações do usuário

#### 2. **Services** (Camada de Negócio)
Lógica de negócio e operações complexas.

**Responsabilidades:**
- Lógica de negócio
- Operações de I/O
- Comunicação com APIs externas
- Gerenciamento de estado

**Arquivos:**

##### `RobocopyCommandBuilder.cs`
```csharp
// Responsável por construir comandos Robocopy
BuildCommand(RobocopyOptions) -> string
```

##### `RobocopyExecutor.cs`
```csharp
// Executa processos Robocopy
ExecuteAsync(arguments, cancellationToken) -> Task<int>
Cancel()
GetExitCodeDescription(exitCode) -> string

// Eventos
OutputReceived
ErrorReceived
ExecutionCompleted
```

##### `LogService.cs`
```csharp
// Gerencia logs da aplicação
GetLogFilePath() -> string
GetLogsDirectory() -> string
EnsureLogsDirectoryExists()
```

##### `SettingsService.cs`
```csharp
// Persiste configurações
LoadSettings() -> AppSettings
SaveSettings(AppSettings)
```

#### 3. **Helpers** (Camada de Utilitários)
Funções auxiliares e utilitários reutilizáveis.

**Responsabilidades:**
- Funções puras
- Conversões
- Validações
- Geração de dados

**Arquivos:**

##### `ThreadOptionsHelper.cs`
```csharp
GenerateThreadOptions(systemThreadCount) -> List<ComboBoxItem>
GetDefaultThreadSelection(options, systemThreadCount) -> int
```

##### `RetryOptionsHelper.cs`
```csharp
GenerateRetryOptions() -> List<ComboBoxItem>
```

##### `WaitTimeOptionsHelper.cs`
```csharp
GenerateWaitTimeOptions() -> List<ComboBoxItem>
```

##### `PathValidator.cs`
```csharp
IsValidPath(path) -> bool
DirectoryExists(path) -> bool
ValidateSourcePath(path) -> (bool, string?)
ValidateDestinationPath(path) -> (bool, string?)
```

## ?? Fluxo de Dados

```
???????????????
?    View     ? (MainWindow.xaml)
?  (UI Layer) ?
???????????????
       ? User Actions
       ?
???????????????
?  ViewModel  ? (Futuro - MVVM)
?   (Logic)   ?
???????????????
       ?
       ?
???????????????????????????????????
?         Services Layer          ?
???????????????????????????????????
? • RobocopyCommandBuilder        ?
? • RobocopyExecutor              ?
? • LogService                    ?
? • SettingsService               ?
???????????????????????????????????
       ?
       ?
???????????????????????????????????
?        Helpers Layer            ?
???????????????????????????????????
? • PathValidator                 ?
? • ThreadOptionsHelper           ?
? • RetryOptionsHelper            ?
? • WaitTimeOptionsHelper         ?
???????????????????????????????????
       ?
       ?
???????????????
?   Models    ?
?   (Data)    ?
???????????????
```

## ?? Dependências

### Models
- ? Não depende de nada
- ? POCO puro

### Services
- ? Depende de: Models
- ? Não conhece: Views, ViewModels

### Helpers
- ? Depende de: nada ou Models
- ? Não conhece: Services, Views

### Views
- ? Depende de: ViewModels (futuro)
- ? Pode usar: Services diretamente (atual)

## ?? Princípios Aplicados

### 1. **Single Responsibility Principle (SRP)**
Cada classe tem uma única responsabilidade:
- `RobocopyCommandBuilder` ? Apenas constrói comandos
- `RobocopyExecutor` ? Apenas executa processos
- `LogService` ? Apenas gerencia logs
- `SettingsService` ? Apenas persiste configurações

### 2. **Separation of Concerns (SoC)**
Separação clara entre:
- **Dados** (Models)
- **Lógica** (Services)
- **Utilitários** (Helpers)
- **Interface** (Views)

### 3. **Dependency Injection (DI)** - Preparado
Serviços podem ser facilmente injetados:
```csharp
public MainWindow(
    RobocopyExecutor executor,
    LogService logService,
    SettingsService settingsService)
{
    // ...
}
```

### 4. **Open/Closed Principle**
Classes abertas para extensão, fechadas para modificação:
- Novos helpers podem ser adicionados sem modificar existentes
- Novos services podem ser adicionados independentemente

### 5. **Testability**
Cada camada pode ser testada isoladamente:
```csharp
[Test]
public void BuildCommand_WithMultiThread_AddsCorrectFlag()
{
    var builder = new RobocopyCommandBuilder();
    var options = new RobocopyOptions { UseMultiThread = true, ThreadCount = 8 };
    
    var command = builder.BuildCommand(options);
    
    Assert.Contains("/MT:8", command);
}
```

## ?? Benefícios da Arquitetura

### Manutenibilidade
- ? Código organizado por responsabilidade
- ? Fácil encontrar funcionalidades
- ? Mudanças isoladas em camadas específicas

### Testabilidade
- ? Cada serviço pode ser testado isoladamente
- ? Mocks fáceis de criar
- ? Testes unitários independentes

### Escalabilidade
- ? Novos serviços sem afetar existentes
- ? Novos helpers reutilizáveis
- ? Fácil adicionar features

### Reutilização
- ? Services podem ser usados em outros projetos
- ? Helpers são agnósticos de UI
- ? Models podem ser serializados/deserializados

### Legibilidade
- ? Estrutura clara e intuitiva
- ? Nomes descritivos
- ? Responsabilidades bem definidas

## ?? Próximos Passos (Roadmap)

### Fase 1: ViewModels (MVVM) ? Preparado
```csharp
MainViewModel
??? Commands (ICommand)
??? Properties (INotifyPropertyChanged)
??? Injeção de Services
```

### Fase 2: Dependency Injection
```csharp
// App.xaml.cs
services.AddSingleton<IRobocopyExecutor, RobocopyExecutor>();
services.AddSingleton<ILogService, LogService>();
services.AddSingleton<ISettingsService, SettingsService>();
```

### Fase 3: Unit Tests
```
Tests/
??? Services.Tests/
??? Helpers.Tests/
??? ViewModels.Tests/
```

### Fase 4: Value Converters
```csharp
Converters/
??? BooleanToVisibilityConverter.cs
??? ThreadCountToStringConverter.cs
??? PathToDisplayNameConverter.cs
```

## ?? Como Usar

### Exemplo: Criar e Executar Comando

```csharp
// 1. Criar opções
var options = new RobocopyOptions
{
    SourcePath = @"C:\Source",
    DestinationPath = @"D:\Destination",
    UseMultiThread = true,
    ThreadCount = 8
};

// 2. Construir comando
var builder = new RobocopyCommandBuilder();
var command = builder.BuildCommand(options);

// 3. Configurar log
var logService = new LogService();
options.LogFilePath = logService.GetLogFilePath();

// 4. Executar
var executor = new RobocopyExecutor();
executor.OutputReceived += (s, output) => Console.WriteLine(output);
executor.ExecutionCompleted += (s, exitCode) => 
{
    var description = RobocopyExecutor.GetExitCodeDescription(exitCode);
    Console.WriteLine(description);
};

await executor.ExecuteAsync(command, CancellationToken.None);
```

### Exemplo: Validar Caminhos

```csharp
var (isValid, error) = PathValidator.ValidateSourcePath(sourcePath);
if (!isValid)
{
    ShowError(error);
    return;
}

var (isDestValid, warning) = PathValidator.ValidateDestinationPath(destPath);
if (warning != null)
{
    ShowWarning(warning);
}
```

### Exemplo: Persistir Configurações

```csharp
var settingsService = new SettingsService();

// Carregar
var settings = settingsService.LoadSettings();

// Modificar
settings.Theme = "Dark";
settings.DefaultThreadCount = 16;

// Salvar
settingsService.SaveSettings(settings);
```

## ?? Métricas de Qualidade

| Métrica | Valor | Status |
|---------|-------|--------|
| Acoplamento | Baixo | ? |
| Coesão | Alta | ? |
| Complexidade Ciclomática | < 10 | ? |
| Linhas por Classe | < 200 | ? |
| Cobertura de Testes | 0% ? TBD | ? |

---

**Arquitetura modular, escalável e manutenível! ??**
