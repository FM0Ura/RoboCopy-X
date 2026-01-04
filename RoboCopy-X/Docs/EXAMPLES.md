# ?? Exemplos Práticos de Release

## ?? Cenário 1: Primeira Release (v1.0.0)

### Usando o Script Automático

```powershell
# 1. Execute o script
.\Build-Release.ps1 -Version "1.0.0"

# 2. O script gera:
#    ? RoboCopy-X.exe (standalone)
#    ? RoboCopy-X-v1.0.0-win-x64.zip
#    ? Checksums (SHA256 e MD5)
#    ? Release notes
#    ? Pasta Release-v1.0.0/

# 3. Commit e tag
git add .
git commit -m "Release v1.0.0 - Primeira versão estável"
git tag -a v1.0.0 -m "Release v1.0.0 - Primeira versão estável"

# 4. Push
git push origin master
git push origin v1.0.0

# 5. No GitHub: https://github.com/FM0Ura/RoboCopy-X/releases/new
#    - Selecione tag: v1.0.0
#    - Anexe: RoboCopy-X-v1.0.0-win-x64.zip
#    - Anexe: RoboCopy-X-v1.0.0-win-x64.zip.sha256
#    - Cole conteúdo de: Release-v1.0.0/RELEASE_NOTES.md
#    - Publique!
```

---

## ?? Cenário 2: Atualização com Nova Feature (v1.1.0)

### Usando GitHub Actions

```powershell
# 1. Desenvolva sua feature
git checkout -b feature/perfis-salvos

# 2. Faça commits
git add .
git commit -m "Add: Sistema de perfis salvos"

# 3. Merge na master
git checkout master
git merge feature/perfis-salvos

# 4. Crie e envie a tag
git tag -a v1.1.0 -m "Release v1.1.0 - Adicionado perfis salvos"
git push origin master
git push origin v1.1.0

# 5. GitHub Actions cuida do resto automaticamente! ??
#    Acompanhe em: https://github.com/FM0Ura/RoboCopy-X/actions
```

---

## ?? Cenário 3: Correção de Bug (v1.0.1)

### Hotfix Rápido

```powershell
# 1. Criar branch de hotfix
git checkout -b hotfix/drag-drop-bug

# 2. Corrigir o bug
# ... fazer correções ...

# 3. Commit
git add .
git commit -m "Fix: Corrigido bug no drag & drop"

# 4. Merge e tag
git checkout master
git merge hotfix/drag-drop-bug
git tag -a v1.0.1 -m "Release v1.0.1 - Correção drag & drop"

# 5. Push (GitHub Actions cuida do build)
git push origin master
git push origin v1.0.1
```

---

## ?? Cenário 4: Beta Release (v2.0.0-beta)

### Testar Features Experimentais

```powershell
# 1. Build local
.\Build-Release.ps1 -Version "2.0.0-beta"

# 2. Tag como prerelease
git tag -a v2.0.0-beta -m "Beta: Redesign da interface"
git push origin v2.0.0-beta

# 3. No GitHub, marque como "pre-release"
#    ? This is a pre-release
```

---

## ?? Cenário 5: Verificar Build Antes de Publicar

### Teste Local Completo

```powershell
# 1. Build local
.\Build-Release.ps1 -Version "1.0.0"

# 2. Testar executável
$testFolder = ".\Test-v1.0.0"
Expand-Archive -Path ".\RoboCopy-X-v1.0.0-win-x64.zip" -DestinationPath $testFolder

# 3. Executar e testar
Start-Process "$testFolder\RoboCopy-X.exe"

# 4. Verificar checksum
$zipHash = (Get-FileHash ".\RoboCopy-X-v1.0.0-win-x64.zip" -Algorithm SHA256).Hash
$storedHash = (Get-Content ".\RoboCopy-X-v1.0.0-win-x64.zip.sha256").Split()[0]

if ($zipHash -eq $storedHash) {
    Write-Host "? Checksum válido!" -ForegroundColor Green
} else {
    Write-Host "? Checksum inválido!" -ForegroundColor Red
}

# 5. Se tudo OK, prosseguir com tag e push
```

---

## ?? Cenário 6: Corrigir Release com Problemas

### Deletar e Recriar

```powershell
# 1. Deletar tag local
git tag -d v1.0.0

# 2. Deletar tag remota
git push origin --delete v1.0.0

# 3. Deletar release no GitHub (via web)
#    https://github.com/FM0Ura/RoboCopy-X/releases

# 4. Corrigir problemas
# ... fazer correções ...

# 5. Rebuild
.\Build-Release.ps1 -Version "1.0.0"

# 6. Recriar tag
git tag -a v1.0.0 -m "Release v1.0.0 (corrigido)"
git push origin v1.0.0
```

---

## ?? Cenário 7: Múltiplas Plataformas (Futuro)

### Build para x86 e ARM

```powershell
# Build para diferentes arquiteturas
$platforms = @("win-x64", "win-x86", "win-arm64")

foreach ($platform in $platforms) {
    Write-Host "Building for $platform..."
    
    dotnet publish RoboCopy-X/RoboCopy-X.csproj `
        -c Release `
        -r $platform `
        --self-contained true `
        -p:PublishSingleFile=true `
        -o "./publish/$platform"
    
    Compress-Archive -Path "./publish/$platform/*" `
        -DestinationPath "RoboCopy-X-v1.0.0-$platform.zip"
}
```

---

## ?? Cenário 8: Release com Changelog Detalhado

### Gerar Changelog Automaticamente

```powershell
# 1. Instalar ferramenta (uma vez)
dotnet tool install -g GitVersion.Tool

# 2. Gerar changelog desde última tag
$lastTag = git describe --tags --abbrev=0
$commits = git log $lastTag..HEAD --pretty=format:"%s" --no-merges

# 3. Categorizar commits
$features = $commits | Where-Object { $_ -match "^Add:" }
$fixes = $commits | Where-Object { $_ -match "^Fix:" }
$improvements = $commits | Where-Object { $_ -match "^Improve:" }

# 4. Criar changelog
$changelog = @"
# Changelog v1.1.0

## ? Novidades
$($features -join "`n")

## ?? Correções
$($fixes -join "`n")

## ?? Melhorias
$($improvements -join "`n")
"@

$changelog | Out-File "CHANGELOG.md" -Encoding UTF8
```

---

## ?? Cenário 9: Verificar Release no CI/CD

### Adicionar Testes ao Workflow

Edite `.github/workflows/release.yml`:

```yaml
jobs:
  test:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Run Tests
      run: dotnet test RoboCopy-X.Tests/RoboCopy-X.Tests.csproj
    
  build-and-release:
    needs: test  # Só faz release se testes passarem
    runs-on: windows-latest
    # ... resto do workflow ...
```

---

## ?? Cenário 10: Anunciar Release nas Redes

### Script de Compartilhamento

```powershell
$version = "1.0.0"
$releaseUrl = "https://github.com/FM0Ura/RoboCopy-X/releases/tag/v$version"

$message = @"
?? RoboCopy-X v$version lançado!

? Novidades:
• Interface moderna com Fluent Design
• Drag & Drop intuitivo
• Validações de segurança
• Multi-threading configurável

?? Download: $releaseUrl

#RoboCopyX #WinUI3 #OpenSource #Windows
"@

# Copiar para clipboard
$message | Set-Clipboard
Write-Host "? Mensagem copiada! Cole nas redes sociais:" -ForegroundColor Green
Write-Host $message
```

---

## ?? Dicas Importantes

### ? Antes de Cada Release

```powershell
# Checklist automatizado
$checks = @{
    "Código compila" = { dotnet build -c Release }
    "Testes passam" = { dotnet test }
    "README atualizado" = { git diff HEAD README.md }
    "Versão correta" = { $true } # Manual
}

foreach ($check in $checks.GetEnumerator()) {
    Write-Host "Verificando: $($check.Key)..." -NoNewline
    try {
        & $check.Value | Out-Null
        Write-Host " ?" -ForegroundColor Green
    } catch {
        Write-Host " ?" -ForegroundColor Red
        exit 1
    }
}
```

### ?? Estatísticas da Release

```powershell
# Ver tamanho dos arquivos
Get-ChildItem -Filter "RoboCopy-X-v*.zip" | 
    Select-Object Name, @{N="Size (MB)";E={[math]::Round($_.Length / 1MB, 2)}}

# Ver número de downloads (GitHub CLI)
gh release view v1.0.0 --json assets -q '.assets[].downloadCount'
```

### ?? Assinar Releases (Avançado)

```powershell
# Gerar assinatura GPG
gpg --armor --detach-sign RoboCopy-X-v1.0.0-win-x64.zip

# Anexar assinatura na release
# RoboCopy-X-v1.0.0-win-x64.zip.asc
```

---

## ?? Recursos Adicionais

- [Semantic Versioning](https://semver.org/)
- [GitHub Releases Guide](https://docs.github.com/en/repositories/releasing-projects-on-github)
- [.NET Publishing](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [GitHub Actions](https://docs.github.com/en/actions)

---

**Última atualização**: 2025-01-20
