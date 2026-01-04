# Algoritmos de Hash Disponíveis

O RoboCopy-X agora suporta múltiplos algoritmos de hash para validação de integridade de arquivos, incluindo algoritmos modernos e seguros.

## Algoritmos Disponíveis

### SHA-2 (Família Tradicional)

#### MD5 
- **Status**: ?? Obsoleto - Não recomendado
- **Velocidade**: ??? Muito rápido
- **Segurança**: ? Fraca (vulnerável a colisões)
- **Tamanho do Hash**: 128 bits (32 caracteres hex)
- **Uso Recomendado**: Apenas para compatibilidade com sistemas legados

#### SHA-1
- **Status**: ?? Obsoleto - Não recomendado
- **Velocidade**: ??? Rápido
- **Segurança**: ? Fraca (vulnerável a colisões desde 2017)
- **Tamanho do Hash**: 160 bits (40 caracteres hex)
- **Uso Recomendado**: Apenas para compatibilidade com sistemas antigos

#### SHA-256
- **Status**: ? **Recomendado** (Padrão)
- **Velocidade**: ?? Boa
- **Segurança**: ????????? Muito Alta
- **Tamanho do Hash**: 256 bits (64 caracteres hex)
- **Uso Recomendado**: Uso geral - melhor equilíbrio entre velocidade e segurança
- **Nota**: Amplamente adotado como padrão da indústria

#### SHA-384
- **Status**: ? Excelente
- **Velocidade**: ?? Boa
- **Segurança**: ???????????? Muito Alta
- **Tamanho do Hash**: 384 bits (96 caracteres hex)
- **Uso Recomendado**: Quando segurança adicional é necessária sem grande impacto de performance

#### SHA-512
- **Status**: ? Excelente
- **Velocidade**: ? Moderada (mais lento)
- **Segurança**: ??????????????? Máxima
- **Tamanho do Hash**: 512 bits (128 caracteres hex)
- **Uso Recomendado**: Máxima segurança para dados críticos

### SHA-3 (Família Moderna - NIST 2015)

SHA-3 é a mais recente família de algoritmos de hash criptográficos padronizados pelo NIST (National Institute of Standards and Technology) em 2015. Utiliza uma construção completamente diferente (sponge construction) em comparação com SHA-2, oferecendo resistência adicional contra ataques futuros.

#### SHA3-256
- **Status**: ? **Moderno e Recomendado**
- **Velocidade**: ? Moderada
- **Segurança**: ???????????? Muito Alta + Resistente a ataques futuros
- **Tamanho do Hash**: 256 bits (64 caracteres hex)
- **Uso Recomendado**: Ambientes que exigem padrões modernos ou máxima resistência criptográfica
- **Vantagens**: 
  - Construção matemática diferente do SHA-2 (diversidade algorítmica)
  - Resistente a ataques de extensão de comprimento
  - Aprovado pelo NIST como padrão de próxima geração

#### SHA3-384
- **Status**: ? Moderno e Muito Seguro
- **Velocidade**: ? Moderada
- **Segurança**: ??????????????? Máxima + Resistente a ataques futuros
- **Tamanho do Hash**: 384 bits (96 caracteres hex)
- **Uso Recomendado**: Segurança avançada com padrões modernos
- **Vantagens**: Mesmas do SHA3-256, com segurança ainda maior

#### SHA3-512
- **Status**: ? Moderno - Máxima Segurança
- **Velocidade**: ? Lenta (mais processamento)
- **Segurança**: ?????????????????? Máxima Absoluta + Resistente a ataques futuros
- **Tamanho do Hash**: 512 bits (128 caracteres hex)
- **Uso Recomendado**: Dados extremamente sensíveis que exigem máxima proteção com padrões modernos
- **Vantagens**: Combinação de maior tamanho de hash com construção SHA-3

### BLAKE3 (Ultra-Moderno - 2020) ? NOVO

BLAKE3 é o algoritmo de hash criptográfico mais rápido disponível, lançado em 2020. Representa a terceira geração da família BLAKE, com design completamente novo focado em máxima performance e paralelização.

#### BLAKE3
- **Status**: ? **Ultra-Moderno** - Estado da Arte
- **Velocidade**: ?????? **Excepcional** (até 50x mais rápido que SHA-256)
- **Segurança**: ???????????? Muito Alta
- **Tamanho do Hash**: 256 bits (64 caracteres hex)
- **Paralelização**: ? **Automática** - Usa múltiplos cores
- **Uso Recomendado**: **Grandes volumes de dados** - Ideal para arquivos >10GB
- **Vantagens Especiais**:
  - **Performance incomparável**: Mais rápido que qualquer outro algoritmo seguro
  - **Escalabilidade**: Velocidade aumenta proporcionalmente com número de cores
  - **Otimizações SIMD**: Aproveita AVX2, AVX-512, SSE, NEON
  - **Tree hashing**: Estrutura de árvore permite processamento paralelo
  - **Moderno**: Design de 2020 preparado para hardware futuro
- **Quando Usar**:
  - ? Arquivos muito grandes (>10 GB)
  - ? Múltiplos arquivos para validar
  - ? Hardware moderno com múltiplos cores
  - ? Quando velocidade é crítica
  - ? Backups de servidores
  - ? Validação frequente de integridade
- **Considerações**:
  - ?? Não é padrão NIST/FIPS (ainda)
  - ?? Compatibilidade limitada com sistemas legados
  - ? Excelente para uso interno e ambientes modernos

**?? Documentação Completa**: Veja [BLAKE3_GUIDE.md](./BLAKE3_GUIDE.md) para detalhes técnicos e benchmarks

## Comparação de Performance

### Velocidade Relativa (arquivos grandes, multi-core)
```
BLAKE3    ???????????????????????????? Mais rápido (4000+ MB/s)
MD5       ???????????????????????????? (600 MB/s)
SHA-1     ???????????????????????????? (500 MB/s)
SHA-256   ???????????????????????????? (400 MB/s)
SHA-384   ???????????????????????????? (400 MB/s)
SHA-512   ???????????????????????????? (500 MB/s)
SHA3-256  ???????????????????????????? (300 MB/s)
SHA3-384  ???????????????????????????? (250 MB/s)
SHA3-512  ???????????????????????????? (200 MB/s)
```

*Nota: BLAKE3 escala linearmente com múltiplos cores - até 9600+ MB/s em CPUs de 8 cores!*

### Segurança (2024)
```
MD5       ???????????????????? Inseguro
SHA-1     ???????????????????? Vulnerável
SHA-256   ???????????????????? Seguro
SHA-384   ???????????????????? Seguro
SHA-512   ???????????????????? Muito Seguro
SHA3-256  ???????????????????? Muito Seguro (Moderno)
SHA3-384  ???????????????????? Muito Seguro (Moderno)
SHA3-512  ???????????????????? Máximo (Moderno)
BLAKE3    ???????????????????? Muito Seguro (Ultra-moderno)
```

### Paralelização (Uso de Múltiplos Cores)
```
BLAKE3    ? Sim - Escalabilidade linear com número de cores
MD5       ? Não
SHA-1     ? Não
SHA-256   ? Não
SHA-384   ? Não
SHA-512   ? Não
SHA3-256  ? Não
SHA3-384  ? Não
SHA3-512  ? Não
```

## Recomendações de Uso

### ?? Backups Pessoais Regulares
**Recomendado**: SHA-256 ou BLAKE3
- SHA-256: Oferece excelente equilíbrio entre velocidade e segurança
- BLAKE3: Ideal se você tem arquivos grandes (>1GB) e hardware moderno

### ?? Dados Corporativos
**Recomendado**: SHA-256 ou SHA3-256
- SHA-256: Padrão da indústria, máxima compatibilidade
- SHA3-256: Padrão moderno para organizações com políticas de segurança atualizadas
- BLAKE3: Excelente para backups internos e validação rápida

### ?? Dados Altamente Confidenciais
**Recomendado**: SHA-512, SHA3-512 ou BLAKE3
- SHA-512/SHA3-512: Máxima segurança disponível
- BLAKE3: Combina segurança robusta com velocidade excepcional

### ?? Arquivos Muito Grandes (>100GB)
**Recomendado**: **BLAKE3** ??
- Economia de tempo significativa (até 50x mais rápido)
- Aproveitamento de múltiplos cores
- **Exemplo**: 100GB em ~10 segundos vs 5 minutos com SHA-256

### ? Performance Crítica / Validações Frequentes
**Recomendado**: **BLAKE3** ??
- Menor overhead de CPU
- Tempo de resposta mínimo
- Ideal para automações e scripts

### ?? Múltiplos Arquivos Simultaneamente
**Recomendado**: **BLAKE3** ??
- Paralelização automática
- Escalabilidade com número de cores
- Throughput máximo

### ?? Compatibilidade / Interoperabilidade
**Recomendado**: SHA-256
- Suporte universal
- Padrão da indústria
- Máxima compatibilidade com ferramentas externas

### ?? Conformidade Regulatória
**Recomendado**: SHA-256 ou SHA3-256/384/512
- SHA-256: FIPS 140-2
- SHA-3: FIPS 202
- Verificar requisitos específicos da regulamentação

## Por Que SHA-3?

SHA-3 foi desenvolvido como resposta a preocupações teóricas sobre a família SHA-2. Embora SHA-2 continue seguro, SHA-3 oferece:

1. **Diversidade Algorítmica**: Baseado em princípios matemáticos diferentes (função esponja Keccak)
2. **Resistência Futura**: Design moderno resistente a ataques conhecidos e teóricos
3. **Padrão NIST**: Aprovado como padrão federal dos EUA (FIPS 202)
4. **Resistência a Ataques de Extensão**: Protegido contra ataques de extensão de comprimento por design
5. **Validação Independente**: Passou por extensa análise criptográfica internacional

## Quando Usar Cada Algoritmo?

### Use MD5 ou SHA-1 apenas se:
- ? Você precisa de compatibilidade com sistemas muito antigos
- ?? **Nunca use para segurança crítica**

### Use SHA-256 se:
- ? Você quer o padrão da indústria
- ? Precisa de boa velocidade com excelente segurança
- ? Compatibilidade máxima é importante

### Use SHA-384/512 se:
- ? Segurança é mais importante que velocidade
- ? Trabalha com dados regulamentados
- ? Políticas de segurança exigem hashes maiores

### Use SHA3-256/384/512 se:
- ? Quer usar o padrão mais moderno
- ? Precisa de diversidade algorítmica (além de SHA-2)
- ? Trabalha em ambiente com requisitos de segurança rigorosos
- ? Planeja manter os dados por muitos anos

### Use BLAKE3 se:
- ? Precisa do algoritmo mais rápido disponível
- ? Está lidando com grandes volumes de dados ou arquivos muito grandes
- ? Tem hardware moderno que pode aproveitar paralelização
- ? Velocidade é crítica e a compatibilidade com padrões NIST/FIPS não é um problema

## Referências

- [NIST FIPS 180-4](https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.180-4.pdf) - SHA-2
- [NIST FIPS 202](https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf) - SHA-3
- [NIST Hash Functions](https://csrc.nist.gov/projects/hash-functions) - Projeto de Funções Hash

## Notas de Implementação

Esta implementação utiliza as classes nativas do .NET 8:
- `System.Security.Cryptography.MD5`
- `System.Security.Cryptography.SHA1`
- `System.Security.Cryptography.SHA256`
- `System.Security.Cryptography.SHA384`
- `System.Security.Cryptography.SHA512`
- `System.Security.Cryptography.SHA3_256`
- `System.Security.Cryptography.SHA3_384`
- `System.Security.Cryptography.SHA3_512`

Todos os algoritmos são implementados pelo próprio .NET, garantindo performance otimizada e conformidade com os padrões.
