using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Blake3;

namespace RoboCopy_X.Services
{
    /// <summary>
    /// Serviço responsável pela validação de integridade de arquivos através de hashing
    /// </summary>
    public class HashValidationService
    {
        public event EventHandler<string>? ValidationProgress;

        /// <summary>
        /// Tipos de algoritmos de hash suportados
        /// </summary>
        public enum HashAlgorithmType
        {
            MD5,
            SHA1,
            SHA256,
            SHA384,
            SHA512,
            SHA3_256,
            SHA3_384,
            SHA3_512,
            BLAKE3
        }

        /// <summary>
        /// Calcula o hash de um arquivo usando o algoritmo especificado
        /// </summary>
        public async Task<string> ComputeFileHashAsync(string filePath, HashAlgorithmType algorithmType, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");
            }

            return await Task.Run(() =>
            {
                // BLAKE3 usa API diferente
                if (algorithmType == HashAlgorithmType.BLAKE3)
                {
                    using var stream = File.OpenRead(filePath);
                    var hasher = Hasher.New();
                    
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        hasher.Update(buffer.AsSpan(0, bytesRead));
                    }
                    
                    var hash = hasher.Finalize();
                    return hash.ToString();
                }
                else
                {
                    using var hashAlgorithm = CreateHashAlgorithm(algorithmType);
                    using var stream = File.OpenRead(filePath);
                    
                    var hash = hashAlgorithm.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Valida se dois arquivos têm o mesmo hash
        /// </summary>
        public async Task<bool> ValidateFilesAsync(string sourceFile, string destinationFile, HashAlgorithmType algorithmType, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException($"Arquivo de origem não encontrado: {sourceFile}");
            }

            if (!File.Exists(destinationFile))
            {
                throw new FileNotFoundException($"Arquivo de destino não encontrado: {destinationFile}");
            }

            ValidationProgress?.Invoke(this, $"Calculando hash do arquivo de origem: {Path.GetFileName(sourceFile)}");
            var sourceHash = await ComputeFileHashAsync(sourceFile, algorithmType, cancellationToken);
            ValidationProgress?.Invoke(this, $"  Hash origem : {sourceHash}");

            ValidationProgress?.Invoke(this, $"Calculando hash do arquivo de destino: {Path.GetFileName(destinationFile)}");
            var destinationHash = await ComputeFileHashAsync(destinationFile, algorithmType, cancellationToken);
            ValidationProgress?.Invoke(this, $"  Hash destino: {destinationHash}");

            var isValid = string.Equals(sourceHash, destinationHash, StringComparison.OrdinalIgnoreCase);
            
            if (isValid)
            {
                ValidationProgress?.Invoke(this, $"? Validação bem-sucedida: {Path.GetFileName(sourceFile)}");
            }
            else
            {
                ValidationProgress?.Invoke(this, $"? FALHA NA VALIDAÇÃO: {Path.GetFileName(sourceFile)}");
                ValidationProgress?.Invoke(this, $"  Os hashes são DIFERENTES!");
            }

            return isValid;
        }

        /// <summary>
        /// Valida todos os arquivos de um diretório recursivamente
        /// </summary>
        public async Task<HashValidationResult> ValidateDirectoryAsync(
            string sourceDirectory, 
            string destinationDirectory, 
            HashAlgorithmType algorithmType,
            bool recursive = true,
            CancellationToken cancellationToken = default)
        {
            var result = new HashValidationResult();

            if (!Directory.Exists(sourceDirectory))
            {
                throw new DirectoryNotFoundException($"Diretório de origem não encontrado: {sourceDirectory}");
            }

            if (!Directory.Exists(destinationDirectory))
            {
                throw new DirectoryNotFoundException($"Diretório de destino não encontrado: {destinationDirectory}");
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var sourceFiles = Directory.GetFiles(sourceDirectory, "*", searchOption);

            foreach (var sourceFile in sourceFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = Path.GetRelativePath(sourceDirectory, sourceFile);
                var destinationFile = Path.Combine(destinationDirectory, relativePath);

                result.TotalFiles++;

                try
                {
                    if (!File.Exists(destinationFile))
                    {
                        result.MissingFiles++;
                        result.FailedFiles.Add(relativePath);
                        ValidationProgress?.Invoke(this, $"? ARQUIVO AUSENTE NO DESTINO: {relativePath}");
                        continue;
                    }

                    var isValid = await ValidateFilesAsync(sourceFile, destinationFile, algorithmType, cancellationToken);
                    
                    if (isValid)
                    {
                        result.ValidFiles++;
                    }
                    else
                    {
                        result.InvalidFiles++;
                        result.FailedFiles.Add(relativePath);
                        ValidationProgress?.Invoke(this, $"? ARQUIVO CORROMPIDO (hash inválido): {relativePath}");
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorFiles++;
                    result.FailedFiles.Add(relativePath);
                    ValidationProgress?.Invoke(this, $"? ERRO AO VALIDAR: {relativePath}");
                    ValidationProgress?.Invoke(this, $"  Detalhes: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Cria uma instância do algoritmo de hash especificado
        /// </summary>
        private HashAlgorithm CreateHashAlgorithm(HashAlgorithmType algorithmType)
        {
            return algorithmType switch
            {
                HashAlgorithmType.MD5 => MD5.Create(),
                HashAlgorithmType.SHA1 => SHA1.Create(),
                HashAlgorithmType.SHA256 => SHA256.Create(),
                HashAlgorithmType.SHA384 => SHA384.Create(),
                HashAlgorithmType.SHA512 => SHA512.Create(),
                HashAlgorithmType.SHA3_256 => SHA3_256.Create(),
                HashAlgorithmType.SHA3_384 => SHA3_384.Create(),
                HashAlgorithmType.SHA3_512 => SHA3_512.Create(),
                HashAlgorithmType.BLAKE3 => throw new InvalidOperationException("BLAKE3 usa API específica"),
                _ => throw new ArgumentException($"Algoritmo de hash não suportado: {algorithmType}")
            };
        }

        /// <summary>
        /// Obtém o nome amigável do algoritmo de hash
        /// </summary>
        public static string GetAlgorithmDisplayName(HashAlgorithmType algorithmType)
        {
            return algorithmType switch
            {
                HashAlgorithmType.MD5 => "MD5 (Rápido, menos seguro)",
                HashAlgorithmType.SHA1 => "SHA-1 (Rápido)",
                HashAlgorithmType.SHA256 => "SHA-256 (Recomendado)",
                HashAlgorithmType.SHA384 => "SHA-384 (Seguro)",
                HashAlgorithmType.SHA512 => "SHA-512 (Mais seguro)",
                HashAlgorithmType.SHA3_256 => "SHA3-256 (Moderno, NIST 2015)",
                HashAlgorithmType.SHA3_384 => "SHA3-384 (Moderno, muito seguro)",
                HashAlgorithmType.SHA3_512 => "SHA3-512 (Moderno, máxima segurança)",
                HashAlgorithmType.BLAKE3 => "BLAKE3 (Ultra-rápido e seguro)",
                _ => algorithmType.ToString()
            };
        }
    }

    /// <summary>
    /// Resultado da validação de hash de um diretório
    /// </summary>
    public class HashValidationResult
    {
        public int TotalFiles { get; set; }
        public int ValidFiles { get; set; }
        public int InvalidFiles { get; set; }
        public int MissingFiles { get; set; }
        public int ErrorFiles { get; set; }
        public System.Collections.Generic.List<string> FailedFiles { get; set; } = new();

        public bool IsSuccess => InvalidFiles == 0 && MissingFiles == 0 && ErrorFiles == 0;
        
        public string GetSummary()
        {
            return $"Total: {TotalFiles} | Válidos: {ValidFiles} | Inválidos: {InvalidFiles} | Ausentes: {MissingFiles} | Erros: {ErrorFiles}";
        }
    }
}
