# ?? Guia de Release - RoboCopy-X

Este guia explica como criar e publicar uma release do RoboCopy-X no GitHub.

---

## ?? Método Rápido (Automático)

### 1. Execute o Script de Build

```powershell
# Na raiz do projeto, execute:
.\Build-Release.ps1 -Version "1.0.0"
```

O script irá:
- ? Validar ambiente (.NET 8)
- ? Limpar builds anteriores
- ? Restaurar dependências
- ? Compilar em modo Release
- ? Publicar executável standalone
- ? Criar estrutura de release
- ? Gerar notas da versão
- ? Compactar arquivos em ZIP
- ? Calcular checksums (SHA256 e MD5)
- ? Fornecer próximos passos

### 2. Commit e Tag

```powershell
# Adicionar e commitar mudanças
git add .
git commit -m "Release v1.0.0"

# Criar tag anotada
git tag -a v1.0.0 -m "Release v1.0.0 - Primeira versão estável"

# Enviar para GitHub
git push origin master
git push origin v1.0.0
```

### 3. Criar Release no GitHub

1. Acesse: https://github.com/FM0Ura/RoboCopy-X/releases/new
2. Selecione a tag: `v1.0.0`
3. Título: `RoboCopy-X v1.0.0`
4. Cole o conteúdo de: `Release-v1.0.0\RELEASE_NOTES.md`
5. Anexe os arquivos:
   - `RoboCopy-X-v1.0.0-win-x64.zip`
   - `RoboCopy-X-v1.0.0-win-x64.zip.sha256`
   - `RoboCopy-X-v1.0.0-win-x64.zip.md5` (opcional)
6. Clique em **"Publish release"**

---

## ??? Método Manual (Passo a Passo)

### Passo 1: Limpar Builds Anteriores

```powershell
# Limpar diretórios de build
Remove-Item RoboCopy-X\bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item RoboCopy-X\obj -Recurse -Force -ErrorAction SilentlyContinue
```

### Passo 2: Restaurar Dependências

```powershell
dotnet restore RoboCopy-X/RoboCopy-X.csproj
```

### Passo 3: Compilar

```powershell
dotnet build RoboCopy-X/RoboCopy-X.csproj -c Release
```

### Passo 4: Publicar Executável

```powershell
dotnet publish RoboCopy-X/RoboCopy-X.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false
```

### Passo 5: Localizar o Executável

O executável estará em:
```
RoboCopy-X\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\RoboCopy-X.exe
```

### Passo 6: Criar Estrutura de Release

```powershell
# Criar pasta
$version = "1.0.0"
$releaseFolder = "Release-v$version"
New-Item -ItemType Directory -Force -Path $releaseFolder

# Copiar arquivos
$publishPath = ".\RoboCopy-X\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"
Copy-Item "$publishPath\RoboCopy-X.exe" $releaseFolder
Copy-Item ".\README.md" $releaseFolder
Copy-Item ".\LICENSE" $releaseFolder
```

### Passo 7: Compactar

```powershell
Compress-Archive -Path "$releaseFolder\*" `
    -DestinationPath ".\RoboCopy-X-v$version-win-x64.zip" `
    -Force
```

### Passo 8: Calcular Checksum

```powershell
$zipFile = ".\RoboCopy-X-v$version-win-x64.zip"
$hash = (Get-FileHash $zipFile -Algorithm SHA256).Hash
"$hash *RoboCopy-X-v$version-win-x64.zip" | `
    Out-File -FilePath "$zipFile.sha256" -Encoding ASCII -NoNewline
```

---

## ?? Automatização com GitHub Actions

### Criar Workflow de Release

Crie o arquivo `.github/workflows/release.yml`:

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

permissions:
  contents: write

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore
      run: dotnet restore RoboCopy-X/RoboCopy-X.csproj
    
    - name: Build
      run: dotnet build RoboCopy-X/RoboCopy-X.csproj -c Release --no-restore
    
    - name: Publish
      run: |
        dotnet publish RoboCopy-X/RoboCopy-X.csproj `
          -c Release `
          -r win-x64 `
          --self-contained true `
          -p:PublishSingleFile=true `
          -p:IncludeNativeLibrariesForSelfExtract=true `
          -o ./publish
    
    - name: Create Release Package
      run: |
        $version = $env:GITHUB_REF -replace 'refs/tags/v', ''
        $releaseFolder = "release-package"
        
        New-Item -ItemType Directory -Force -Path $releaseFolder
        Copy-Item ".\publish\RoboCopy-X.exe" $releaseFolder
        Copy-Item ".\README.md" $releaseFolder
        Copy-Item ".\LICENSE" $releaseFolder
        
        Compress-Archive -Path "$releaseFolder\*" `
          -DestinationPath ".\RoboCopy-X-v$version-win-x64.zip" -Force
        
        $hash = (Get-FileHash ".\RoboCopy-X-v$version-win-x64.zip" -Algorithm SHA256).Hash
        "$hash *RoboCopy-X-v$version-win-x64.zip" | `
          Out-File -FilePath ".\RoboCopy-X-v$version-win-x64.zip.sha256" -Encoding ASCII
      shell: pwsh
    
    - name: Create Release
      uses: softprops/action-gh-release@v1
      with:
        files: |
          RoboCopy-X-v*.zip
          RoboCopy-X-v*.zip.sha256
        draft: false
        prerelease: false
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

### Usar o Workflow

```powershell
# Criar e enviar tag
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0

# O GitHub Actions automaticamente:
# 1. Compila o projeto
# 2. Cria o executável
# 3. Gera o ZIP
# 4. Calcula checksums
# 5. Cria a Release
# 6. Anexa os arquivos
```

---

## ?? Checklist de Release

### Antes de Criar a Release

- [ ] Código compila sem erros ou warnings
- [ ] Todas as funcionalidades testadas
- [ ] Tests passam (se houver)
- [ ] README.md atualizado
- [ ] CHANGELOG.md atualizado (se houver)
- [ ] Versão atualizada no projeto
- [ ] Commit de todas as mudanças
- [ ] Branch principal limpa

### Durante a Release

- [ ] Tag criada com versionamento semântico (v1.0.0)
- [ ] Executável gerado e testado
- [ ] ZIP criado com todos os arquivos necessários
- [ ] Checksums calculados
- [ ] Release notes preparados
- [ ] Screenshots/GIFs atualizados (se aplicável)

### Após Publicar

- [ ] Release publicada no GitHub
- [ ] Executável funciona standalone
- [ ] Download do ZIP funciona
- [ ] Checksum verificado
- [ ] Links da documentação funcionam
- [ ] Anunciar nas redes sociais (opcional)
- [ ] Atualizar changelog para próxima versão

---

## ?? Versionamento Semântico

Use o formato: `MAJOR.MINOR.PATCH`

- **MAJOR** (1.0.0): Mudanças incompatíveis na API
- **MINOR** (0.1.0): Novas funcionalidades compatíveis
- **PATCH** (0.0.1): Correções de bugs compatíveis

### Exemplos

```
v1.0.0 - Primeira release estável
v1.1.0 - Adicionado suporte a perfis salvos
v1.1.1 - Corrigido bug no drag & drop
v2.0.0 - Interface redesenhada (breaking change)
```

### Tags Especiais

```
v1.0.0-alpha    - Versão alpha
v1.0.0-beta     - Versão beta
v1.0.0-rc.1     - Release Candidate 1
```

---

## ?? Template de Release Notes

```markdown
# ?? RoboCopy-X vX.Y.Z

## ? Novidades
- Nova funcionalidade A
- Nova funcionalidade B

## ?? Melhorias
- Melhoria X
- Melhoria Y

## ?? Correções
- Corrigido bug A
- Corrigido bug B

## ?? Download
- Windows 64-bit: RoboCopy-X-vX.Y.Z-win-x64.zip

## ?? Requisitos
- Windows 10 1809+ ou Windows 11
- Arquitetura x64

## ?? Verificação
SHA256: [hash aqui]
```

---

## ?? Verificação do Executável

### Testar o Executável

```powershell
# 1. Extrair ZIP
Expand-Archive -Path ".\RoboCopy-X-v1.0.0-win-x64.zip" -DestinationPath ".\Test"

# 2. Verificar checksum
$zipHash = (Get-FileHash ".\RoboCopy-X-v1.0.0-win-x64.zip" -Algorithm SHA256).Hash
$storedHash = Get-Content ".\RoboCopy-X-v1.0.0-win-x64.zip.sha256"

if ($zipHash -eq $storedHash.Split()[0]) {
    Write-Host "? Checksum válido!" -ForegroundColor Green
} else {
    Write-Host "? Checksum inválido!" -ForegroundColor Red
}

# 3. Executar aplicação
.\Test\RoboCopy-X.exe
```

### Verificar Dependências

```powershell
# Verificar se é realmente standalone
# (não deve depender de .NET instalado)
dumpbin /dependents ".\Test\RoboCopy-X.exe"
```

---

## ?? Solução de Problemas

### Erro: "Executável não funciona"

**Problema**: Usuário reporta que executável não abre

**Soluções**:
1. Verificar se é Windows 10 1809+
2. Verificar arquitetura (x64 vs x86)
3. Executar como administrador
4. Verificar Windows Defender / Antivírus
5. Verificar se há dependências faltando

### Erro: "Arquivo muito grande"

**Problema**: ZIP está muito pesado

**Soluções**:
```powershell
# Habilitar trimming (remove código não usado)
dotnet publish -p:PublishTrimmed=true

# Usar ReadyToRun (pode aumentar tamanho mas melhora startup)
dotnet publish -p:PublishReadyToRun=true
```

### Erro: "Build falha no GitHub Actions"

**Problema**: Workflow não consegue compilar

**Soluções**:
1. Verificar versão do .NET no workflow
2. Verificar paths dos arquivos
3. Verificar permissões do GITHUB_TOKEN
4. Testar localmente primeiro

---

## ?? Suporte

- ?? Issues: https://github.com/FM0Ura/RoboCopy-X/issues
- ?? Discussões: https://github.com/FM0Ura/RoboCopy-X/discussions
- ?? Email: [seu-email]

---

**Última atualização**: 2025-01-20
**Versão do guia**: 1.0
