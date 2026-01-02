using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoboCopy_X.Services
{
    public class RobocopyExecutor
    {
        public event EventHandler<string>? OutputReceived;
        public event EventHandler<string>? ErrorReceived;
        public event EventHandler<int>? ExecutionCompleted;
        
        private Process? _currentProcess;

        public async Task<int> ExecuteAsync(string arguments, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "robocopy.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                _currentProcess = new Process
                {
                    StartInfo = processStartInfo,
                    EnableRaisingEvents = true
                };

                // Handle output
                _currentProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        OutputReceived?.Invoke(this, e.Data);
                    }
                };

                _currentProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        ErrorReceived?.Invoke(this, e.Data);
                    }
                };

                try
                {
                    _currentProcess.Start();
                    _currentProcess.BeginOutputReadLine();
                    _currentProcess.BeginErrorReadLine();

                    // Wait for process to exit or cancellation
                    while (!_currentProcess.WaitForExit(100))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    var exitCode = _currentProcess.ExitCode;
                    ExecutionCompleted?.Invoke(this, exitCode);
                    
                    return exitCode;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }, cancellationToken);
        }

        public void Cancel()
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                _currentProcess.Kill(true);
            }
        }

        public static string GetExitCodeDescription(int exitCode)
        {
            return exitCode switch
            {
                0 => "Nenhum arquivo foi copiado. Nenhuma falha encontrada. Nenhum arquivo foi incompatível.",
                1 => "Todos os arquivos foram copiados com sucesso.",
                2 => "Existem alguns arquivos adicionais no diretório de destino que não estão presentes no diretório de origem.",
                3 => "Alguns arquivos foram copiados. Arquivos adicionais estavam presentes.",
                4 => "Alguns arquivos incompatíveis foram detectados. Nenhum arquivo foi copiado.",
                5 => "Alguns arquivos foram copiados. Alguns arquivos eram incompatíveis.",
                6 => "Existem arquivos adicionais e arquivos incompatíveis. Nenhum arquivo foi copiado.",
                7 => "Arquivos foram copiados, havia arquivos adicionais e havia arquivos incompatíveis.",
                8 => "Várias falhas ocorreram durante a cópia.",
                >= 16 => "Erro fatal: Robocopy não executou a cópia.",
                _ => $"Código de saída desconhecido: {exitCode}"
            };
        }
    }
}
