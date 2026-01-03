using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace RoboCopy_X.Services
{
    /// <summary>
    /// Serviço responsável por operações de interface de usuário,
    /// incluindo gerenciamento de InfoBar e scrolling automático.
    /// </summary>
    public class UIService
    {
        private readonly InfoBar _infoBar;
        private readonly ScrollViewer _outputScrollViewer;
        private readonly TextBlock _outputTextBlock;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly bool _autoCloseSuccessMessages;

        /// <summary>
        /// Inicializa uma nova instância do serviço de UI.
        /// </summary>
        /// <param name="infoBar">InfoBar para exibir mensagens</param>
        /// <param name="outputScrollViewer">ScrollViewer para a saída de texto</param>
        /// <param name="outputTextBlock">TextBlock para a saída de texto</param>
        /// <param name="dispatcherQueue">Fila do dispatcher para operações assíncronas</param>
        /// <param name="autoCloseSuccessMessages">Se mensagens de sucesso devem fechar automaticamente</param>
        public UIService(
            InfoBar infoBar,
            ScrollViewer outputScrollViewer,
            TextBlock outputTextBlock,
            DispatcherQueue dispatcherQueue,
            bool autoCloseSuccessMessages = true)
        {
            _infoBar = infoBar ?? throw new ArgumentNullException(nameof(infoBar));
            _outputScrollViewer = outputScrollViewer ?? throw new ArgumentNullException(nameof(outputScrollViewer));
            _outputTextBlock = outputTextBlock ?? throw new ArgumentNullException(nameof(outputTextBlock));
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
            _autoCloseSuccessMessages = autoCloseSuccessMessages;
        }

        /// <summary>
        /// Exibe uma mensagem na InfoBar.
        /// </summary>
        /// <param name="message">Mensagem a ser exibida</param>
        /// <param name="severity">Severidade da mensagem</param>
        public void ShowInfoBar(string message, InfoBarSeverity severity)
        {
            _infoBar.Message = message;
            _infoBar.Severity = severity;
            _infoBar.IsOpen = true;

            // Auto-fecha mensagens de sucesso após 5 segundos se configurado
            if (severity == InfoBarSeverity.Success && _autoCloseSuccessMessages)
            {
                var timer = _dispatcherQueue.CreateTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += (s, e) =>
                {
                    _infoBar.IsOpen = false;
                    timer.Stop();
                };
                timer.Start();
            }
        }

        /// <summary>
        /// Adiciona texto à saída e faz scroll automático para o final.
        /// </summary>
        /// <param name="text">Texto a ser adicionado</param>
        public void AppendOutput(string text)
        {
            _outputTextBlock.Text += text;
            _outputScrollViewer.ChangeView(null, _outputScrollViewer.ScrollableHeight, null);
        }

        /// <summary>
        /// Limpa o texto de saída.
        /// </summary>
        public void ClearOutput()
        {
            _outputTextBlock.Text = string.Empty;
        }

        /// <summary>
        /// Define o texto de saída.
        /// </summary>
        /// <param name="text">Texto a ser definido</param>
        public void SetOutput(string text)
        {
            _outputTextBlock.Text = text;
        }
    }
}
