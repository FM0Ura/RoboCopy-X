using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RoboCopy_X.Services;

namespace RoboCopy_X.Helpers
{
    public static class PathValidator
    {
        public static bool IsValidPath(string? path)
        {
            return !string.IsNullOrWhiteSpace(path);
        }

        public static bool DirectoryExists(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return Directory.Exists(path);
        }

        public static (bool IsValid, string? ErrorMessage) ValidateSourcePath(string? path)
        {
            if (!IsValidPath(path))
            {
                return (false, "Por favor, especifique o caminho de origem.");
            }

            if (!DirectoryExists(path))
            {
                return (false, "O caminho de origem não existe!");
            }

            return (true, null);
        }

        public static (bool IsValid, string? WarningMessage) ValidateDestinationPath(string? path)
        {
            if (!IsValidPath(path))
            {
                return (false, "Por favor, especifique o caminho de destino.");
            }

            if (!DirectoryExists(path))
            {
                return (true, "Aviso: O caminho de destino não existe e será criado.");
            }

            return (true, null);
        }

        /// <summary>
        /// Valida se há espaço suficiente no destino para a operação
        /// </summary>
        public static (bool IsValid, string? ErrorMessage, long RequiredSpace, long AvailableSpace) 
            ValidateDiskSpace(string sourcePath, string destinationPath, bool includeSubdirectories = true)
        {
            try
            {
                var diskSpaceService = new Services.DiskSpaceService();
                var (hasSpace, message, destInfo, requiredSpace) = 
                    diskSpaceService.CheckAvailableSpace(sourcePath, destinationPath, includeSubdirectories);

                if (!hasSpace)
                {
                    return (false, message, requiredSpace, destInfo.AvailableFreeSpace);
                }

                return (true, message, requiredSpace, destInfo.AvailableFreeSpace);
            }
            catch (System.Exception ex)
            {
                return (false, $"Erro ao verificar espaço em disco: {ex.Message}", 0, 0);
            }
        }

        /// <summary>
        /// CRÍTICA #1: Valida se origem e destino são diferentes
        /// </summary>
        public static (bool IsValid, string? ErrorMessage) ValidateDifferentPaths(
            string sourcePath, 
            string destinationPath)
        {
            try
            {
                var normalizedSource = Path.GetFullPath(sourcePath).TrimEnd('\\', '/').ToLowerInvariant();
                var normalizedDest = Path.GetFullPath(destinationPath).TrimEnd('\\', '/').ToLowerInvariant();
                
                if (normalizedSource == normalizedDest)
                {
                    return (false, 
                        "ERRO: Origem e destino são iguais!\n\n" +
                        $"Caminho: {sourcePath}\n\n" +
                        "A origem e o destino devem ser diferentes.\n" +
                        "Escolha um destino diferente para continuar.");
                }
                
                return (true, null);
            }
            catch (System.Exception ex)
            {
                return (false, $"Erro ao comparar caminhos: {ex.Message}");
            }
        }

        /// <summary>
        /// CRÍTICA #2: Valida se destino não está dentro da origem (previne loop infinito)
        /// </summary>
        public static (bool IsValid, string? ErrorMessage) ValidateNestedPaths(
            string sourcePath, 
            string destinationPath)
        {
            try
            {
                var normalizedSource = Path.GetFullPath(sourcePath).TrimEnd('\\', '/').ToLowerInvariant() + "\\";
                var normalizedDest = Path.GetFullPath(destinationPath).TrimEnd('\\', '/').ToLowerInvariant() + "\\";
                
                if (normalizedDest.StartsWith(normalizedSource))
                {
                    return (false, 
                        "PERIGO: Destino está dentro da origem!\n\n" +
                        $"Origem: {sourcePath}\n" +
                        $"Destino: {destinationPath}\n\n" +
                        "Isso causará:\n" +
                        "• Recursão infinita durante a cópia\n" +
                        "• Enchimento completo do disco\n" +
                        "• Possível travamento do sistema\n\n" +
                        "Operação bloqueada por segurança.\n" +
                        "Escolha um destino fora da pasta de origem.");
                }
                
                return (true, null);
            }
            catch (System.Exception ex)
            {
                return (false, $"Erro ao verificar hierarquia de pastas: {ex.Message}");
            }
        }

        /// <summary>
        /// CRÍTICA #3: Verifica permissões de leitura na origem
        /// </summary>
        public static (bool HasPermission, string? ErrorMessage) CheckReadPermission(string path)
        {
            try
            {
                // Tenta listar arquivos - teste prático de permissão
                _ = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                
                // Tenta listar subpastas
                _ = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
                
                return (true, null);
            }
            catch (System.UnauthorizedAccessException)
            {
                return (false, 
                    "Sem permissão de leitura!\n\n" +
                    $"Pasta: {path}\n\n" +
                    "Você não tem permissão para ler esta pasta.\n\n" +
                    "Soluções:\n" +
                    "• Execute o programa como Administrador\n" +
                    "• Verifique as permissões da pasta nas propriedades\n" +
                    "• Escolha outra pasta que você tenha acesso");
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return (false, "A pasta não foi encontrada.");
            }
            catch (System.Exception ex)
            {
                return (false, $"Erro ao verificar permissões de leitura: {ex.Message}");
            }
        }

        /// <summary>
        /// CRÍTICA #4: Verifica permissões de escrita no destino
        /// </summary>
        public static (bool HasPermission, string? ErrorMessage) CheckWritePermission(string path)
        {
            try
            {
                // Se o destino não existe, verifica a pasta pai
                var testPath = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
                
                if (string.IsNullOrEmpty(testPath) || !Directory.Exists(testPath))
                {
                    // Se pasta pai não existe, tenta criar (teste de permissão)
                    var parentPath = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        return (false, "Caminho de destino inválido.");
                    }
                    testPath = parentPath;
                }
                
                // Criar arquivo temporário para testar escritura
                var testFile = Path.Combine(testPath, $".robocopy_write_test_{System.Guid.NewGuid()}.tmp");
                
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                
                return (true, null);
            }
            catch (System.UnauthorizedAccessException)
            {
                return (false, 
                    "Sem permissão de escrita!\n\n" +
                    $"Pasta: {path}\n\n" +
                    "Você não tem permissão para escrever nesta pasta.\n\n" +
                    "Soluções:\n" +
                    "• Execute o programa como Administrador\n" +
                    "• Verifique se o disco não está protegido contra gravação\n" +
                    "• Verifique as permissões da pasta\n" +
                    "• Escolha outra pasta que você tenha acesso");
            }
            catch (System.IO.IOException ex)
            {
                if (ex.Message.Contains("read-only", System.StringComparison.OrdinalIgnoreCase))
                {
                    return (false, 
                        "Disco protegido contra gravação!\n\n" +
                        "O disco de destino está em modo somente leitura.\n\n" +
                        "Verifique:\n" +
                        "• Se é um CD/DVD (não é possível gravar)\n" +
                        "• Se o pen drive tem switch de proteção\n" +
                        "• As propriedades do disco");
                }
                return (false, $"Erro ao verificar permissões de escrita: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                return (false, $"Erro ao verificar permissões de escrita: {ex.Message}");
            }
        }

        /// <summary>
        /// CRÍTICA #5: Verifica se não são caminhos de sistema do Windows
        /// </summary>
        public static (bool IsSafe, string? WarningMessage) CheckSystemPath(string path)
        {
            try
            {
                var systemPaths = new[]
                {
                    @"C:\Windows",
                    @"C:\Program Files",
                    @"C:\Program Files (x86)",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows),
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.System),
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles),
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86)
                };
                
                var normalizedPath = Path.GetFullPath(path).ToLowerInvariant();
                
                foreach (var systemPath in systemPaths.Where(p => !string.IsNullOrEmpty(p)))
                {
                    var normalizedSystem = Path.GetFullPath(systemPath).ToLowerInvariant();
                    
                    if (normalizedPath.StartsWith(normalizedSystem))
                    {
                        return (false, 
                            "ATENÇÃO: Pasta do sistema Windows!\n\n" +
                            $"Caminho: {path}\n" +
                            $"Sistema: {systemPath}\n\n" +
                            "Copiar de/para pastas do sistema pode causar:\n" +
                            "• Instabilidade no Windows\n" +
                            "• Necessidade de privilégios de administrador\n" +
                            "• Arquivos bloqueados em uso pelo sistema\n" +
                            "• Corrupção do sistema operacional\n\n" +
                            "Esta operação não é recomendada!\n\n" +
                            "Deseja realmente continuar?");
                    }
                }
                
                return (true, null);
            }
            catch (System.Exception ex)
            {
                return (true, $"Aviso: Não foi possível verificar caminho do sistema: {ex.Message}");
            }
        }

        /// <summary>
        /// CRÍTICA #6: Verifica se há arquivos conflitantes no destino
        /// </summary>
        public static (bool HasConflicts, string ConflictInfo) CheckExistingFiles(
            string sourcePath,
            string destinationPath)
        {
            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    return (false, string.Empty);
                }

                // Obter todos os arquivos recursivamente
                var sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                var destFiles = Directory.GetFiles(destinationPath, "*", SearchOption.AllDirectories);

                // Criar mapa de caminhos relativos para comparação
                var sourceRelativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var destRelativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Normalizar caminhos da origem
                foreach (var file in sourceFiles)
                {
                    var relativePath = Path.GetRelativePath(sourcePath, file);
                    sourceRelativePaths.Add(relativePath);
                }

                // Normalizar caminhos do destino
                foreach (var file in destFiles)
                {
                    var relativePath = Path.GetRelativePath(destinationPath, file);
                    destRelativePaths.Add(relativePath);
                }

                // Encontrar arquivos conflitantes (mesmos caminhos relativos)
                var conflictingFiles = sourceRelativePaths.Intersect(destRelativePaths).ToList();

                if (conflictingFiles.Count == 0)
                {
                    return (false, string.Empty);
                }

                // Contar pastas também
                int totalSourceFolders = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories).Length;
                int totalDestFolders = Directory.GetDirectories(destinationPath, "*", SearchOption.AllDirectories).Length;

                var conflictInfo = new StringBuilder();
                conflictInfo.AppendLine($"Arquivos conflitantes encontrados: {conflictingFiles.Count}");
                conflictInfo.AppendLine($"Total de arquivos na origem: {sourceFiles.Length}");
                conflictInfo.AppendLine($"Total de arquivos no destino: {destFiles.Length}");
                conflictInfo.AppendLine($"Total de pastas na origem: {totalSourceFolders}");
                conflictInfo.AppendLine($"Total de pastas no destino: {totalDestFolders}");
                conflictInfo.AppendLine();
                conflictInfo.AppendLine("Exemplos de arquivos conflitantes:");
                
                // Mostrar até 5 exemplos de diferentes tipos/pastas
                var examples = conflictingFiles.Take(5).ToList();
                foreach (var file in examples)
                {
                    // Mostrar caminho relativo para melhor contexto
                    conflictInfo.AppendLine($"  • {file}");
                }

                if (conflictingFiles.Count > 5)
                {
                    conflictInfo.AppendLine($"  ... e mais {conflictingFiles.Count - 5} arquivos");
                }

                return (true, conflictInfo.ToString());
            }
            catch (UnauthorizedAccessException)
            {
                return (false, "Erro: Sem permissão para acessar alguns arquivos ou pastas.");
            }
            catch (System.Exception ex)
            {
                return (false, $"Erro ao verificar arquivos: {ex.Message}");
            }
        }
    }
}
