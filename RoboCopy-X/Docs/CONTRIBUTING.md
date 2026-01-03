# Guia de Contribuição

Obrigado por considerar contribuir com o RoboCopy-X! Este documento fornece diretrizes para contribuir com o projeto.

## ?? Sumário

- [Código de Conduta](#código-de-conduta)
- [Como Contribuir](#como-contribuir)
- [Estilo de Código](#estilo-de-código)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Processo de Pull Request](#processo-de-pull-request)
- [Relatando Bugs](#relatando-bugs)
- [Sugerindo Melhorias](#sugerindo-melhorias)

## ?? Código de Conduta

Este projeto segue um código de conduta. Ao participar, você concorda em manter um ambiente respeitoso e acolhedor para todos.

## ?? Como Contribuir

### 1. Fork e Clone

```bash
# Fork via GitHub UI, depois clone seu fork
git clone https://github.com/SEU-USUARIO/RoboCopy-X.git
cd RoboCopy-X
```

### 2. Configurar Ambiente

```bash
# Instalar .NET 8 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0

# Restaurar dependências
dotnet restore

# Verificar build
dotnet build
```

### 3. Criar Branch

```bash
# Branch para features
git checkout -b feature/minha-feature

# Branch para correções
git checkout -b fix/meu-bug
```

### 4. Fazer Alterações

- Siga o [estilo de código](#estilo-de-código)
- Adicione testes se aplicável
- Atualize documentação conforme necessário

### 5. Commit

```bash
# Commits semânticos
git commit -m "Add: Nova funcionalidade X"
git commit -m "Fix: Correção do bug Y"
git commit -m "Docs: Atualização do README"
```

**Prefixos de Commit:**
- `Add:` - Nova funcionalidade
- `Fix:` - Correção de bug
- `Docs:` - Documentação
- `Style:` - Formatação, sem mudança de código
- `Refactor:` - Refatoração de código
- `Test:` - Adição/atualização de testes
- `Chore:` - Tarefas de manutenção

### 6. Push e PR

```bash
git push origin feature/minha-feature
# Abra PR via GitHub UI
```

## ?? Estilo de Código

### C# (.NET 8)

#### Nomenclatura

```csharp
// Classes, Métodos, Propriedades: PascalCase
public class MyService
{
    public string PropertyName { get; set; }
    
    public void MethodName() { }
}

// Campos privados: _camelCase com underscore
private string _privateField;
private int _counter;

// Parâmetros e variáveis locais: camelCase
public void ProcessData(string inputValue)
{
    var localVariable = inputValue;
}

// Constantes: PascalCase
private const int MaxRetryCount = 5;
```

#### Comentários XML

**Sempre** adicione comentários XML em:
- Classes públicas
- Métodos públicos e protegidos
- Propriedades públicas
- Enums e seus valores

```csharp
/// <summary>
/// Executa a operação de cópia usando Robocopy.
/// </summary>
/// <param name="source">Caminho de origem</param>
/// <param name="destination">Caminho de destino</param>
/// <param name="options">Opções de cópia</param>
/// <returns>Código de saída do Robocopy</returns>
/// <exception cref="ArgumentNullException">
/// Lançado quando source ou destination são nulos
/// </exception>
public async Task<int> ExecuteAsync(
    string source, 
    string destination, 
    RobocopyOptions options)
{
    // Implementação
}
```

#### Estrutura de Classe

```csharp
public class ExampleService
{
    // 1. Campos privados
    private readonly IDependency _dependency;
    private int _counter;
    
    // 2. Construtores
    public ExampleService(IDependency dependency)
    {
        _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
    }
    
    // 3. Propriedades públicas
    public string PropertyName { get; set; }
    
    // 4. Métodos públicos
    public void PublicMethod() { }
    
    // 5. Métodos privados
    private void PrivateMethod() { }
}
```

#### Boas Práticas

```csharp
// ? BOM: Null-checking com throw
public ExampleService(IDependency dependency)
{
    _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
}

// ? BOM: Guard clauses no início
public void Process(string input)
{
    if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentException("Input cannot be empty", nameof(input));
    
    // Lógica principal
}

// ? BOM: Using statements para IDisposable
using var file = File.OpenRead("path");
// Automaticamente disposed

// ? BOM: Pattern matching
if (obj is MyType myInstance)
{
    myInstance.DoSomething();
}

// ? BOM: Async/await
public async Task<string> GetDataAsync()
{
    var result = await _service.FetchAsync();
    return result;
}

// ? EVITAR: Async void (exceto event handlers)
// ? Use: async Task
public async Task ProcessAsync() { }
```

### XAML

```xaml
<!-- Indentação: 4 espaços -->
<!-- Propriedades complexas em linhas separadas -->
<Button x:Name="MyButton"
        Content="Click Me"
        Width="120"
        Height="40"
        HorizontalAlignment="Center"
        Click="MyButton_Click">
    <Button.Resources>
        <!-- Resources aqui -->
    </Button.Resources>
</Button>

<!-- Naming: PascalCase com sufixo do tipo -->
<TextBlock x:Name="StatusTextBlock" />
<ComboBox x:Name="OptionsComboBox" />
<CheckBox x:Name="EnableFeatureCheckBox" />
```

## ?? Estrutura do Projeto

```
RoboCopy-X/
??? Services/           # Lógica de negócio, serviços reutilizáveis
??? Helpers/            # Utilitários e helpers
??? Models/             # POCOs, DTOs, ViewModels
??? Docs/               # Documentação técnica
??? MainWindow.xaml     # UI principal
??? App.xaml            # Configuração da aplicação
```

### Quando Criar Cada Tipo

- **Service**: Lógica de negócio complexa, operações assíncronas, integração com APIs
- **Helper**: Métodos estáticos utilitários, validações, formatadores
- **Model**: Estruturas de dados, opções de configuração

## ?? Processo de Pull Request

### Checklist

Antes de submeter um PR, verifique:

- [ ] Código compila sem erros nem warnings
- [ ] Testes passam (se aplicável)
- [ ] Código segue o estilo do projeto
- [ ] Comentários XML adicionados
- [ ] Documentação atualizada
- [ ] Commit messages são claros
- [ ] Branch está atualizada com master

### Template de PR

```markdown
## Descrição
Breve descrição das mudanças

## Tipo de Mudança
- [ ] Bug fix
- [ ] Nova feature
- [ ] Refatoração
- [ ] Documentação

## Como Testar
1. Passo 1
2. Passo 2
3. Resultado esperado

## Screenshots (se aplicável)
[Adicionar screenshots]

## Checklist
- [ ] Código testado localmente
- [ ] Documentação atualizada
- [ ] Comentários adicionados
```

## ?? Relatando Bugs

### Informações Necessárias

```markdown
**Descrição do Bug**
Descrição clara e concisa do bug

**Passos para Reproduzir**
1. Vá para '...'
2. Clique em '...'
3. Role até '...'
4. Veja o erro

**Comportamento Esperado**
O que deveria acontecer

**Comportamento Atual**
O que está acontecendo

**Screenshots**
Se aplicável, adicione screenshots

**Ambiente**
- OS: Windows 11 22H2
- Versão: RoboCopy-X 1.0.0
- .NET: 8.0

**Informações Adicionais**
Qualquer contexto adicional
```

## ?? Sugerindo Melhorias

Use o template de feature request:

```markdown
**A feature resolve um problema? Qual?**
Descrição do problema

**Solução Desejada**
Como você gostaria que funcionasse

**Alternativas Consideradas**
Outras soluções que você considerou

**Contexto Adicional**
Screenshots, exemplos, etc.
```

## ?? Dúvidas?

- Abra uma [discussão](https://github.com/FM0Ura/RoboCopy-X/discussions)
- Comente em uma issue existente
- Entre em contato com os mantenedores

---

**Obrigado por contribuir! ??**
