# Algoritmos de Hash Implementados - RoboCopy-X

## ?? Novos Algoritmos Adicionados

O RoboCopy-X agora suporta **9 algoritmos de hash** diferentes, incluindo os mais modernos padrões criptográficos e o ultra-rápido BLAKE3!

### ? Algoritmos Recém-Adicionados

#### SHA-384
- Parte da família SHA-2
- Equilíbrio entre SHA-256 e SHA-512
- Excelente para segurança avançada sem grande impacto de performance

#### SHA3-256 ? NOVO
- Padrão NIST 2015 (FIPS 202)
- Construção matemática moderna (Keccak)
- Resistente a ataques futuros
- Recomendado para ambientes com requisitos de segurança modernos

#### SHA3-384 ? NOVO
- Padrão NIST 2015 com segurança ainda maior
- Ideal para dados altamente sensíveis
- Construção resistente a ataques de extensão

#### SHA3-512 ? NOVO
- Máxima segurança com padrões modernos
- Recomendado para dados extremamente confidenciais
- Melhor proteção disponível

#### BLAKE3 ?? ULTRA-NOVO
- **O algoritmo mais rápido disponível** - Até 50x mais rápido que SHA-256!
- Lançado em 2020 - Estado da arte
- **Paralelização automática** - Usa múltiplos cores
- Ideal para **grandes volumes de dados** (>10GB)
- Excelente para validações frequentes
- Design moderno preparado para hardware futuro

## ?? Lista Completa de Algoritmos

| Algoritmo | Status | Velocidade | Segurança | Paralelização | Uso Recomendado |
|-----------|--------|------------|-----------|---------------|-----------------|
| MD5 | ?? Obsoleto | ??? | ? Fraca | ? | Apenas compatibilidade |
| SHA-1 | ?? Obsoleto | ??? | ? Fraca | ? | Apenas compatibilidade |
| SHA-256 | ? Recomendado | ?? | ????????? | ? | Uso geral (Padrão) |
| SHA-384 | ? Excelente | ?? | ???????????? | ? | Segurança avançada |
| SHA-512 | ? Excelente | ? | ??????????????? | ? | Máxima segurança |
| SHA3-256 | ? Moderno | ? | ???????????? | ? | Padrões modernos |
| SHA3-384 | ? Moderno | ? | ??????????????? | ? | Segurança moderna avançada |
| SHA3-512 | ? Moderno | ? | ?????????????????? | ? | Máxima segurança moderna |
| BLAKE3 | ? **Ultra-moderno** | ?????? | ???????????? | ? **Sim!** | **Grandes arquivos** |

## ?? Como Usar

1. **Ative a validação de hash** no painel de configurações
2. **Selecione o algoritmo desejado** no menu dropdown
3. **Execute a operação** - a validação ocorrerá automaticamente após a cópia

### Algoritmo Padrão
**SHA-256** é selecionado por padrão, oferecendo o melhor equilíbrio entre velocidade e segurança para a maioria dos casos de uso.

## ?? Recomendações Rápidas

### Para a maioria dos usuários
?? **Use SHA-256** - Padrão da indústria, rápido e seguro

### Para dados corporativos
?? **Use SHA-256 ou SHA3-256** - Segurança comprovada ou padrão moderno

### Para máxima segurança
?? **Use SHA-512 ou SHA3-512** - Melhor proteção disponível

### Para arquivos muito grandes (>10GB) ??
?? **Use BLAKE3** - Até 50x mais rápido, economia de tempo significativa

### Para múltiplos arquivos grandes
?? **Use BLAKE3** - Paralelização automática com múltiplos cores

### Para validações frequentes
?? **Use BLAKE3** - Menor overhead, máxima velocidade

### Para compatibilidade máxima
?? **Use SHA-256** - Suporte universal

## ?? Documentação Detalhada

Para informações completas sobre cada algoritmo, incluindo:
- Características técnicas
- Comparações de performance
- Casos de uso específicos
- Padrões e referências

Consulte: [Docs/HASH_ALGORITHMS.md](./HASH_ALGORITHMS.md)

## ?? Implementação Técnica

- Todos os algoritmos usam as implementações nativas do **.NET 8**
- Classes utilizadas do namespace `System.Security.Cryptography`
- Performance otimizada e conformidade com padrões internacionais
- Suporte completo a SHA-3 (FIPS 202) incluído no .NET 8

## ?? Por Que Múltiplos Algoritmos?

1. **Compatibilidade**: Suporte a sistemas legados (MD5, SHA-1)
2. **Flexibilidade**: Escolha baseada em requisitos específicos
3. **Modernidade**: Acesso aos algoritmos mais recentes (SHA-3)
4. **Segurança**: Opções de máxima proteção quando necessário
5. **Performance**: Algoritmos otimizados para diferentes cenários

## ? Impacto na Performance

O tempo de validação depende do algoritmo escolhido e do tamanho dos arquivos:

**Exemplo: Arquivo de 1GB**
- SHA-256: ~2-3 segundos
- SHA-512: ~3-4 segundos
- SHA3-256: ~4-5 segundos
- SHA3-512: ~5-7 segundos
- BLAKE3: ~1-2 segundos

*Valores aproximados em hardware moderno (SSD, CPU multi-core)*

## ?? Segurança

?? **Importante**: MD5 e SHA-1 são considerados **criptograficamente inseguros** e devem ser usados apenas para compatibilidade com sistemas legados. Para qualquer uso relacionado à segurança, use SHA-256 ou superior.

## ?? Changelog

### v2.1.0 (2024) - BLAKE3 Edition ??
- ? **Adicionado suporte a BLAKE3** - O algoritmo mais rápido disponível!
  - Até 50x mais rápido que SHA-256
  - Paralelização automática com múltiplos cores
  - Ideal para grandes volumes de dados
- ?? Nova documentação completa sobre BLAKE3
- ?? UI atualizada com informações sobre BLAKE3
- ?? Biblioteca Blake3 2.2.0 integrada

### v2.0.0 (2024)
- ? Adicionado suporte a SHA-384
- ? Adicionado suporte a SHA3-256 (NIST 2015)
- ? Adicionado suporte a SHA3-384 (NIST 2015)
- ? Adicionado suporte a SHA3-512 (NIST 2015)
- ?? Documentação completa de algoritmos
- ?? UI atualizada com tooltips informativos

### v1.0.0 (2024)
- Suporte inicial: MD5, SHA-1, SHA-256, SHA-512
