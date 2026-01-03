using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace RoboCopy_X.Services
{
    /// <summary>
    /// Serviço responsável por rastrear e gerenciar o progresso de operações do Robocopy.
    /// </summary>
    public class ProgressTracker
    {
        private readonly ProgressBar? _progressBar;
        private readonly TextBlock? _progressPercentageText;
        private readonly TextBlock? _progressStatusText;
        private readonly TextBlock? _progressDetailsText;
        private readonly StackPanel? _progressSection;

        private DateTime _operationStartTime;
        private int _totalFiles;
        private int _processedFiles;

        /// <summary>
        /// Inicializa uma nova instância do rastreador de progresso.
        /// </summary>
        /// <param name="progressBar">Barra de progresso</param>
        /// <param name="progressPercentageText">Texto de porcentagem</param>
        /// <param name="progressStatusText">Texto de status</param>
        /// <param name="progressDetailsText">Texto de detalhes</param>
        /// <param name="progressSection">Seção completa de progresso</param>
        public ProgressTracker(
            ProgressBar? progressBar,
            TextBlock? progressPercentageText,
            TextBlock? progressStatusText,
            TextBlock? progressDetailsText,
            StackPanel? progressSection)
        {
            _progressBar = progressBar;
            _progressPercentageText = progressPercentageText;
            _progressStatusText = progressStatusText;
            _progressDetailsText = progressDetailsText;
            _progressSection = progressSection;
        }

        /// <summary>
        /// Total de arquivos a processar.
        /// </summary>
        public int TotalFiles
        {
            get => _totalFiles;
            set => _totalFiles = value;
        }

        /// <summary>
        /// Número de arquivos processados.
        /// </summary>
        public int ProcessedFiles
        {
            get => _processedFiles;
            set => _processedFiles = value;
        }

        /// <summary>
        /// Inicializa o rastreamento de uma nova operação.
        /// </summary>
        public void StartOperation()
        {
            _operationStartTime = DateTime.Now;
            _totalFiles = 0;
            _processedFiles = 0;

            ShowProgressSection();
            ResetProgress();
        }

        /// <summary>
        /// Finaliza o rastreamento da operação.
        /// </summary>
        public void StopOperation()
        {
            HideProgressSection();
            ResetProgress();
        }

        /// <summary>
        /// Exibe a seção de progresso.
        /// </summary>
        public void ShowProgressSection()
        {
            if (_progressSection != null)
            {
                _progressSection.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Oculta a seção de progresso.
        /// </summary>
        public void HideProgressSection()
        {
            if (_progressSection != null)
            {
                _progressSection.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Reseta todos os indicadores de progresso.
        /// </summary>
        public void ResetProgress()
        {
            if (_progressBar != null)
            {
                _progressBar.IsIndeterminate = false;
                _progressBar.Value = 0;
            }

            if (_progressPercentageText != null)
            {
                _progressPercentageText.Text = "0%";
            }

            if (_progressStatusText != null)
            {
                _progressStatusText.Text = "Preparando transferência...";
            }

            if (_progressDetailsText != null)
            {
                _progressDetailsText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Atualiza a porcentagem de progresso.
        /// </summary>
        /// <param name="percentage">Porcentagem de 0 a 100</param>
        public void UpdateProgress(double percentage)
        {
            // Atualiza barra de progresso
            if (_progressBar != null)
            {
                _progressBar.Value = Math.Min(100, Math.Max(0, percentage));
            }

            // Atualiza texto de porcentagem
            if (_progressPercentageText != null)
            {
                _progressPercentageText.Text = $"{percentage:F1}%";
            }

            // Calcula tempo restante
            if (percentage > 0 && percentage < 100)
            {
                var elapsed = DateTime.Now - _operationStartTime;
                var estimatedTotal = elapsed.TotalSeconds / (percentage / 100.0);
                var remaining = TimeSpan.FromSeconds(estimatedTotal - elapsed.TotalSeconds);

                if (remaining.TotalSeconds > 0 && _progressStatusText != null)
                {
                    string timeText = FormatTimeRemaining(remaining);

                    // Só atualiza se não estiver mostrando nome de arquivo
                    if (!_progressStatusText.Text.StartsWith("??") &&
                        !_progressStatusText.Text.Contains("Copiando:"))
                    {
                        _progressStatusText.Text = $"?? Transferindo... {timeText}";
                    }
                }
            }
            else if (percentage >= 100 && _progressStatusText != null)
            {
                _progressStatusText.Text = "? Operação concluída!";
            }
        }

        /// <summary>
        /// Atualiza o status da operação.
        /// </summary>
        /// <param name="status">Mensagem de status</param>
        public void UpdateStatus(string status)
        {
            if (_progressStatusText != null)
            {
                _progressStatusText.Text = status;
            }
        }

        /// <summary>
        /// Incrementa o contador de arquivos processados e atualiza o status.
        /// </summary>
        /// <param name="fileName">Nome do arquivo sendo processado</param>
        public void IncrementProcessedFiles(string fileName)
        {
            _processedFiles++;

            // Trunca nome de arquivo se muito longo
            if (fileName.Length > 50)
            {
                fileName = "..." + fileName.Substring(fileName.Length - 47);
            }

            UpdateStatus($"?? Copiando: {fileName}");

            // Atualiza detalhes se total conhecido
            if (_totalFiles > 0 && _progressDetailsText != null)
            {
                _progressDetailsText.Text = $"Arquivo {_processedFiles} de {_totalFiles}";
                _progressDetailsText.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Formata o tempo restante em formato legível.
        /// </summary>
        /// <param name="remaining">Tempo restante</param>
        /// <returns>String formatada</returns>
        private static string FormatTimeRemaining(TimeSpan remaining)
        {
            if (remaining.TotalHours >= 1)
            {
                return $"{remaining.Hours}h {remaining.Minutes}m restantes";
            }
            else if (remaining.TotalMinutes >= 1)
            {
                return $"{remaining.Minutes}m {remaining.Seconds}s restantes";
            }
            else
            {
                return $"{remaining.Seconds}s restantes";
            }
        }
    }
}
