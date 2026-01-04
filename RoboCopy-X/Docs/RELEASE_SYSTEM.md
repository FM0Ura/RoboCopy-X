# ?? Sistema de Release - Resumo Executivo

## ? O Que Foi Criado

Um sistema completo para criar e publicar releases do RoboCopy-X no GitHub, com **3 métodos diferentes** para atender todas as necessidades.

---

## ?? Arquivos do Sistema

| Arquivo | Descrição | Uso |
|---------|-----------|-----|
| **Build-Release.ps1** | Script PowerShell automatizado | Executar localmente |
| **RELEASE_GUIDE.md** | Guia completo de release | Referência detalhada |
| **QUICK_RELEASE.md** | Início rápido | Consulta rápida |
| **EXAMPLES.md** | 10 cenários práticos | Exemplos reais |
| **.github/workflows/release.yml** | CI/CD GitHub Actions | Automação total |

---

## ?? Escolha Seu Método

### ?? Método 1: Build Local (Recomendado para Iniciantes)

**Quando usar**: Primeira release, testes locais, controle total

```powershell
# Execute este comando
.\Build-Release.ps1 -Version "1.0.0"
```

**O que faz**:
- ? Valida ambiente (.NET 8)
- ? Compila em modo Release
- ? Gera executável standalone (sem .NET necessário)
- ? Cria ZIP com documentação
- ? Calcula checksums SHA256 e MD5
- ? Gera release notes prontos para GitHub
- ? Fornece comandos Git para copiar/colar

**Resultado**:
```
?? Arquivos gerados:
   • RoboCopy-X-v1.0.0-win-x64.zip      (~25 MB)
   • RoboCopy-X-v1.0.0-win-x64.zip.sha256
   • RoboCopy-X-v1.0.0-win-x64.zip.md5
   • Release-v1.0.0/                    (pasta com conteúdo)
```

---

### ?? Método 2: GitHub Actions (Totalmente Automático)

**Quando usar**: Após primeira release, releases frequentes, CI/CD

```powershell
# 1. Criar tag
git tag -a v1.0.0 -m "Release v1.0.0"

# 2. Enviar para GitHub
git push origin v1.0.0

# 3. Pronto! GitHub Actions cuida de tudo
```

**O que faz automaticamente**:
- ? Detecta nova tag
- ? Faz checkout do código
- ? Configura .NET 8
- ? Compila o projeto
- ? Publica executável
- ? Cria ZIP e checksums
- ? Gera release notes
- ? Cria release no GitHub
- ? Anexa todos os arquivos

**Acompanhar progresso**:
- https://github.com/FM0Ura/RoboCopy-X/actions

---

### ?? Método 3: Manual (Para Aprendizado)

**Quando usar**: Entender o processo, debug, customização

Consulte: **RELEASE_GUIDE.md** ? Seção "Método Manual"

---

## ?? Comparação dos Métodos

| Característica | Build Local | GitHub Actions | Manual |
|----------------|-------------|----------------|--------|
| **Facilidade** | ???? | ????? | ?? |
| **Controle** | ???? | ??? | ????? |
| **Velocidade** | ??? | ???? | ?? |
| **Automação** | ???? | ????? | ? |
| **Aprendizado** | ??? | ?? | ????? |

---

## ?? Fluxo Recomendado

### Para Primeira Release (v1.0.0)

```powershell
# 1. Testar build local
.\Build-Release.ps1 -Version "1.0.0"

# 2. Testar executável gerado
.\Release-v1.0.0\RoboCopy-X.exe

# 3. Se OK, criar tag e enviar
git add .
git commit -m "Release v1.0.0"
git tag -a v1.0.0 -m "Release v1.0.0 - Primeira versão estável"
git push origin master
git push origin v1.0.0

# 4. Criar release no GitHub manualmente
# https://github.com/FM0Ura/RoboCopy-X/releases/new
```

### Para Releases Seguintes (v1.1.0+)

```powershell
# Apenas criar e enviar tag
git tag -a v1.1.0 -m "Release v1.1.0 - Nova feature"
git push origin v1.1.0

# GitHub Actions cuida de tudo! ?
```

---

## ?? Guia de Leitura

### Nível: Iniciante
1. **QUICK_RELEASE.md** (1 min)
   - Comandos essenciais
   - Checklist rápido

### Nível: Intermediário
2. **RELEASE_GUIDE.md** (10 min)
   - Guia passo a passo completo
   - Todos os métodos explicados
   - Troubleshooting

### Nível: Avançado
3. **EXAMPLES.md** (20 min)
   - 10 cenários reais
   - Automações customizadas
   - Dicas avançadas

---

## ?? Conceitos Importantes

### Versionamento Semântico
```
v1.0.0 = MAJOR.MINOR.PATCH
    ?      ?     ?? Correção de bug (+1)
    ?      ???????? Nova funcionalidade (+1, reset patch)
    ??????????????? Breaking change (+1, reset minor e patch)
```

### Tags Git
```powershell
# Tag anotada (recomendado)
git tag -a v1.0.0 -m "Mensagem"

# Tag simples (não recomendado para releases)
git tag v1.0.0
```

### Checksum
```
SHA256: Para verificar integridade do arquivo
MD5:    Adicional (menos seguro que SHA256)
```

---

## ? Checklist Rápida

### Antes de Criar Release
- [ ] Código compila sem erros
- [ ] Funcionalidades testadas
- [ ] README.md atualizado
- [ ] Versão semântica correta
- [ ] Commits limpos na master

### Durante Release
- [ ] Script executado com sucesso
- [ ] Executável testado localmente
- [ ] ZIP criado com todos os arquivos
- [ ] Checksums calculados
- [ ] Tag Git criada

### Após Publicar
- [ ] Release publicada no GitHub
- [ ] Download do ZIP funciona
- [ ] Executável funciona standalone
- [ ] Links da documentação corretos
- [ ] Anunciar (opcional)

---

## ?? Ajuda Rápida

### Comandos Git Úteis
```powershell
# Ver tags
git tag -l

# Deletar tag local
git tag -d v1.0.0

# Deletar tag remota
git push origin --delete v1.0.0

# Ver diferenças desde última tag
git log v1.0.0..HEAD --oneline
```

### Verificar Executável
```powershell
# Verificar tamanho
(Get-Item .\Release-v1.0.0\RoboCopy-X.exe).Length / 1MB

# Verificar checksum
Get-FileHash .\RoboCopy-X-v1.0.0-win-x64.zip -Algorithm SHA256
```

---

## ?? Links Úteis

- ?? [Semantic Versioning](https://semver.org/)
- ?? [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github)
- ?? [GitHub Actions](https://docs.github.com/en/actions)
- ?? [.NET Publish](https://learn.microsoft.com/en-us/dotnet/core/deploying/)

---

## ?? Dicas Finais

### Para Economizar Tempo
- Use **GitHub Actions** após dominar o processo
- Mantenha **CHANGELOG.md** atualizado
- Automatize testes antes de release

### Para Segurança
- Sempre calcule **checksums**
- Teste o executável antes de publicar
- Use **tags anotadas** (não simples)

### Para Profissionalismo
- Siga **versionamento semântico**
- Escreva **release notes** descritivos
- Mantenha documentação atualizada

---

## ?? Suporte

Problemas ou dúvidas? Consulte:

1. **RELEASE_GUIDE.md** ? Seção "Solução de Problemas"
2. **EXAMPLES.md** ? Cenário similar ao seu caso
3. **GitHub Issues** ? Reporte bugs/sugestões

---

<div align="center">

**Sistema de Release criado com ?? para RoboCopy-X**

Versão 1.0 | 2025-01-20

</div>
