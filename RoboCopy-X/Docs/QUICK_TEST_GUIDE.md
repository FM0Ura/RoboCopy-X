# ?? Guia Rápido de Testes - Validações Críticas

## ? Teste Rápido (5 minutos)

Execute estes 6 testes para verificar todas as validações:

---

### **? Teste 1: Caminhos Iguais** (30 segundos)

**Setup:**
```
Não precisa criar nada!
```

**No RoboCopy-X:**
1. **Origem:** `C:\Users\Public`
2. **Destino:** `C:\Users\Public`
3. Clicar **Executar**

**Resultado Esperado:**
```
? Erro: Origem e destino são iguais!

Caminho: C:\Users\Public

[Dialog de erro é exibido]
[Operação é BLOQUEADA]
```

---

### **? Teste 2: Destino Dentro da Origem** (30 segundos)

**Setup:**
```
Criar: C:\Teste
```

**No RoboCopy-X:**
1. **Origem:** `C:\Teste`
2. **Destino:** `C:\Teste\Backup`
3. Clicar **Executar**

**Resultado Esperado:**
```
?? PERIGO: Destino está dentro da origem!

Origem: C:\Teste
Destino: C:\Teste\Backup

Isso causará:
• Recursão infinita...

[Dialog de erro é exibido]
[Operação é BLOQUEADA]
```

---

### **? Teste 3: Sem Permissão de Leitura** (1 minuto)

**Setup:**
```powershell
# PowerShell como Administrador
$folder = "C:\TestProtegido"
New-Item -Path $folder -ItemType Directory -Force
# Remover permissões (simplificado):
# Ou usar: C:\Windows\System32\config (já protegido)
```

**No RoboCopy-X:**
1. **Origem:** `C:\Windows\System32\config`
2. **Destino:** `C:\Teste`
3. Clicar **Executar**

**Resultado Esperado:**
```
? Sem permissão de leitura!

Pasta: C:\Windows\System32\config

Soluções:
• Execute como Administrador...

[Dialog de erro é exibido]
[Operação é BLOQUEADA]
```

---

### **? Teste 4: Sem Permissão de Escrita** (30 segundos)

**Setup:**
```
Use um CD/DVD no drive
OU
Conecte pen drive protegido contra gravação
```

**No RoboCopy-X:**
1. **Origem:** `C:\Windows\Web`
2. **Destino:** `E:\` (letra do CD/DVD ou pen drive protegido)
3. Clicar **Executar**

**Resultado Esperado:**
```
? Sem permissão de escrita!

OU

? Disco protegido contra gravação!

[Dialog de erro é exibido]
[Operação é BLOQUEADA]
```

---

### **? Teste 5: Caminho de Sistema** (30 segundos)

**No RoboCopy-X:**
1. **Origem:** `C:\Windows`
2. **Destino:** `C:\Teste`
3. Clicar **Executar**

**Resultado Esperado:**
```
?? ATENÇÃO: Pasta do sistema Windows!

Caminho: C:\Windows
Sistema: C:\Windows

Copiar de/para pastas do sistema pode causar...

Deseja realmente continuar?

[Continuar Mesmo Assim]  [Cancelar]

[Dialog de AVISO é exibido]
[Solicita CONFIRMAÇÃO do usuário]
```

---

### **? Teste 6: Espaço Insuficiente** (2 minutos)

**Setup - Opção A (Simples):**
```
Use um pen drive pequeno com pouco espaço (< 500MB livres)
```

**Setup - Opção B (Criar arquivos grandes):**
```powershell
# PowerShell
$folder = "C:\TestGrande"
New-Item -Path $folder -ItemType Directory -Force
# Criar arquivo de 1GB
fsutil file createnew "$folder\arquivo1gb.bin" 1073741824
```

**No RoboCopy-X:**
1. **Origem:** `C:\TestGrande` (1GB)
2. **Destino:** Pen drive com < 500MB livres
3. ?? Marcar "Copiar subdiretórios"
4. Clicar **Executar**

**Resultado Esperado:**
```
[Aguarda cálculo de tamanho - pode demorar alguns segundos]

? Espaço insuficiente!

Necessário: 1.00 GB
Margem de segurança: 100.00 MB
Total necessário: 1.10 GB
Disponível: 450.00 MB
Faltam: 650.00 MB

Libere espaço no disco de destino...

[Dialog de erro é exibido]
[Operação é BLOQUEADA]
```

---

## ?? Checklist de Verificação

Após executar todos os testes, marque:

- [ ] ? Teste 1: Caminhos Iguais - BLOQUEADO
- [ ] ? Teste 2: Destino Dentro Origem - BLOQUEADO
- [ ] ? Teste 3: Sem Permissão Leitura - BLOQUEADO
- [ ] ? Teste 4: Sem Permissão Escrita - BLOQUEADO
- [ ] ? Teste 5: Caminho Sistema - AVISO + CONFIRMAÇÃO
- [ ] ? Teste 6: Espaço Insuficiente - BLOQUEADO

---

## ? Teste de Sucesso (Bonus)

Para verificar que operações válidas ainda funcionam:

**Setup:**
```powershell
$origem = "C:\TestOrigem"
$destino = "C:\TestDestino"
New-Item -Path $origem -ItemType Directory -Force
"teste" | Out-File "$origem\arquivo.txt"
```

**No RoboCopy-X:**
1. **Origem:** `C:\TestOrigem`
2. **Destino:** `C:\TestDestino`
3. ?? Marcar "Copiar subdiretórios"
4. Clicar **Executar**

**Resultado Esperado:**
```
? Espaço suficiente disponível.

Necessário: 5 bytes
Disponível: [muito espaço] GB
Margem de segurança: 100.00 MB

[InfoBar informativo é exibido]
[Operação PROSSEGUE normalmente]
[Robocopy é executado com sucesso]
```

---

## ?? Problemas Comuns

### **Validação não apareceu:**
- Certifique-se de que compilou após as mudanças
- Reinicie a aplicação
- Verifique se está usando a versão correta

### **Cálculo de espaço demora muito:**
- Normal para pastas grandes (> 100GB)
- Tempo esperado: ~1-5 segundos por 10GB
- Se demorar mais de 1 minuto, verifique se origem tem milhões de arquivos

### **Erro de compilação:**
- Execute: `dotnet clean` e depois `dotnet build`
- Verifique se todos os arquivos foram salvos

---

## ?? Tempo Estimado

| Teste | Tempo |
|-------|-------|
| Teste 1 | 30s |
| Teste 2 | 30s |
| Teste 3 | 1min |
| Teste 4 | 30s |
| Teste 5 | 30s |
| Teste 6 | 2min |
| **TOTAL** | **~5min** |

---

## ?? Critério de Sucesso

**Todos os testes devem:**
1. ? Exibir mensagem de erro clara
2. ? Bloquear operação (ou solicitar confirmação para sistema)
3. ? NÃO executar o Robocopy
4. ? Mostrar dialog com opção "OK" ou "Cancelar"

Se QUALQUER teste falhar, reporte o problema!

---

**Boa sorte nos testes! ???**
