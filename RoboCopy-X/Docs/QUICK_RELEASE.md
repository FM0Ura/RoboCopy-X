# ?? Quick Start - Criar Release

## ?? Solução de Problemas Comuns

### Erro NETSDK1198 / NETSDK1094

Se você receber erros sobre perfis de publicação:

```powershell
# Teste primeiro se a publicação funciona:
.\Test-Publish.ps1

# Se passar, execute o build:
.\Build-Release.ps1 -Version "1.0.0"
```

---

## Método 1: Script Automático (Recomendado)

### Passo Único
```powershell
.\Build-Release.ps1 -Version "1.0.0"
```

Depois, siga as instruções na tela para criar tag e publicar no GitHub.

---

## Método 2: GitHub Actions (Totalmente Automático)

### Criar e Enviar Tag
```powershell
# 1. Commit suas mudanças
git add .
git commit -m "Preparar release v1.0.0"

# 2. Criar tag
git tag -a v1.0.0 -m "Release v1.0.0"

# 3. Enviar para GitHub
git push origin master
git push origin v1.0.0
```

? **Pronto!** O GitHub Actions automaticamente:
- Compila o projeto
- Cria o executável
- Gera o ZIP
- Calcula checksums
- Cria a release
- Anexa todos os arquivos

Veja o progresso em:
https://github.com/FM0Ura/RoboCopy-X/actions

---

## Método 3: Manual

### 1. Publicar
```powershell
dotnet publish RoboCopy-X/RoboCopy-X.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    -p:EnableCompressionInSingleFile=true `
    -o ./publish
```

### 2. Localizar Executável
```
./publish/RoboCopy-X.exe
```

### 3. Criar ZIP
- Copie o `RoboCopy-X.exe` para uma pasta
- Adicione `README.md` e `LICENSE`
- Compacte tudo

### 4. Publicar no GitHub
- Acesse: https://github.com/FM0Ura/RoboCopy-X/releases/new
- Anexe o ZIP
- Publique!

---

## ?? Testar Antes de Publicar

```powershell
# Testar se a publicação funciona
.\Test-Publish.ps1

# Ver mais detalhes
.\Test-Publish.ps1 -Verbose
```

---

## ?? Documentação Completa

Veja [RELEASE_GUIDE.md](RELEASE_GUIDE.md) para instruções detalhadas.

---

## ? Comandos Úteis

```powershell
# Ver tags existentes
git tag -l

# Deletar tag local
git tag -d v1.0.0

# Deletar tag remota
git push origin --delete v1.0.0

# Ver status do workflow
gh run list

# Ver logs do último workflow
gh run view
```

---

## ?? Versionamento

Use versionamento semântico: `MAJOR.MINOR.PATCH`

- **v1.0.0** - Primeira release
- **v1.1.0** - Nova funcionalidade
- **v1.1.1** - Correção de bug
- **v2.0.0** - Breaking change

---

## ? Checklist

Antes de criar release:

- [ ] Código compila sem erros
- [ ] Funcionalidades testadas
- [ ] README.md atualizado
- [ ] Versão correta
- [ ] Commit de tudo
- [ ] Teste de publicação passou (.\Test-Publish.ps1)
