using System;
using System.IO;

namespace RoboCopy_X.Services
{
    public class DiskSpaceService
    {
        public class DiskSpaceInfo
        {
            public long AvailableFreeSpace { get; set; }
            public long TotalSize { get; set; }
            public long UsedSpace { get; set; }
            public double FreeSpacePercentage { get; set; }
            
            public string AvailableFreeSpaceFormatted => FormatBytes(AvailableFreeSpace);
            public string TotalSizeFormatted => FormatBytes(TotalSize);
            public string UsedSpaceFormatted => FormatBytes(UsedSpace);

            public static string FormatBytes(long bytes)
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = bytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }

        /// <summary>
        /// Obtém informações de espaço em disco para um caminho específico
        /// </summary>
        public DiskSpaceInfo GetDiskSpace(string path)
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(path) ?? throw new ArgumentException("Caminho inválido"));
                
                if (!drive.IsReady)
                {
                    throw new InvalidOperationException($"A unidade {drive.Name} não está pronta.");
                }

                return new DiskSpaceInfo
                {
                    AvailableFreeSpace = drive.AvailableFreeSpace,
                    TotalSize = drive.TotalSize,
                    UsedSpace = drive.TotalSize - drive.AvailableFreeSpace,
                    FreeSpacePercentage = (double)drive.AvailableFreeSpace / drive.TotalSize * 100
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao obter informações do disco: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Calcula o tamanho total de um diretório e seus subdiretórios
        /// </summary>
        public long CalculateDirectorySize(string path, bool includeSubdirectories = true)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    throw new DirectoryNotFoundException($"O diretório '{path}' não foi encontrado.");
                }

                long totalSize = 0;
                var dirInfo = new DirectoryInfo(path);

                // Adiciona tamanho dos arquivos no diretório atual
                foreach (var file in dirInfo.GetFiles())
                {
                    try
                    {
                        totalSize += file.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignora arquivos sem permissão
                        continue;
                    }
                    catch (FileNotFoundException)
                    {
                        // Ignora arquivos que não existem mais
                        continue;
                    }
                }

                // Se incluir subdiretórios, calcula recursivamente
                if (includeSubdirectories)
                {
                    foreach (var subDir in dirInfo.GetDirectories())
                    {
                        try
                        {
                            totalSize += CalculateDirectorySize(subDir.FullName, true);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Ignora subdiretórios sem permissão
                            continue;
                        }
                    }
                }

                return totalSize;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao calcular tamanho do diretório: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica se há espaço suficiente no destino para a operação
        /// </summary>
        public (bool HasSpace, string Message, DiskSpaceInfo DestinationInfo, long RequiredSpace) 
            CheckAvailableSpace(string sourcePath, string destinationPath, bool includeSubdirectories = true)
        {
            try
            {
                // Calcula tamanho necessário
                var requiredSpace = CalculateDirectorySize(sourcePath, includeSubdirectories);
                
                // Obtém espaço disponível no destino
                var destInfo = GetDiskSpace(destinationPath);

                // Adiciona margem de segurança de 10% ou 100MB (o que for maior)
                var safetyMargin = Math.Max((long)(requiredSpace * 0.1), 100 * 1024 * 1024);
                var totalRequired = requiredSpace + safetyMargin;

                bool hasSpace = destInfo.AvailableFreeSpace >= totalRequired;

                string message;
                if (hasSpace)
                {
                    message = $"? Espaço suficiente disponível.\n\n" +
                             $"Necessário: {DiskSpaceInfo.FormatBytes(requiredSpace)}\n" +
                             $"Disponível: {destInfo.AvailableFreeSpaceFormatted}\n" +
                             $"Margem de segurança: {DiskSpaceInfo.FormatBytes(safetyMargin)}\n" +
                             $"Após cópia: ~{DiskSpaceInfo.FormatBytes(destInfo.AvailableFreeSpace - totalRequired)} livres";
                }
                else
                {
                    var deficit = totalRequired - destInfo.AvailableFreeSpace;
                    message = $"? Espaço insuficiente!\n\n" +
                             $"Necessário: {DiskSpaceInfo.FormatBytes(requiredSpace)}\n" +
                             $"Margem de segurança: {DiskSpaceInfo.FormatBytes(safetyMargin)}\n" +
                             $"Total necessário: {DiskSpaceInfo.FormatBytes(totalRequired)}\n" +
                             $"Disponível: {destInfo.AvailableFreeSpaceFormatted}\n" +
                             $"Faltam: {DiskSpaceInfo.FormatBytes(deficit)}\n\n" +
                             $"Libere espaço no disco de destino antes de continuar.";
                }

                return (hasSpace, message, destInfo, requiredSpace);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao verificar espaço disponível: {ex.Message}", ex);
            }
        }
    }
}
