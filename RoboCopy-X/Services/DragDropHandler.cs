using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace RoboCopy_X.Services
{
    /// <summary>
    /// Serviço responsável por gerenciar operações de drag and drop para seleção de pastas.
    /// </summary>
    public class DragDropHandler
    {
        private readonly Action<string> _onPathSelected;
        private readonly Action<string, InfoBarSeverity> _onError;

        /// <summary>
        /// Inicializa uma nova instância do manipulador de drag and drop.
        /// </summary>
        /// <param name="onPathSelected">Callback chamado quando um caminho é selecionado</param>
        /// <param name="onError">Callback chamado quando ocorre um erro</param>
        public DragDropHandler(
            Action<string> onPathSelected,
            Action<string, InfoBarSeverity>? onError = null)
        {
            _onPathSelected = onPathSelected ?? throw new ArgumentNullException(nameof(onPathSelected));
            _onError = onError ?? ((msg, sev) => { }); // Default: não faz nada
        }

        /// <summary>
        /// Manipula o evento DragOver de um Border.
        /// </summary>
        /// <param name="sender">Border que recebeu o evento</param>
        /// <param name="e">Argumentos do evento</param>
        /// <param name="caption">Legenda a ser exibida</param>
        public void HandleDragOver(object sender, DragEventArgs e, string caption)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.Caption = caption;

            // Verifica se temos itens de armazenamento
            var hasStorageItems = e.DataView.Contains(StandardDataFormats.StorageItems);

            if (!hasStorageItems)
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }

            // Feedback visual
            if (sender is Border border && hasStorageItems)
            {
                border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
                border.BorderThickness = new Thickness(3);
            }
        }

        /// <summary>
        /// Manipula o evento Drop de um Border.
        /// </summary>
        /// <param name="sender">Border que recebeu o evento</param>
        /// <param name="e">Argumentos do evento</param>
        public async Task HandleDropAsync(object sender, DragEventArgs e)
        {
            // Reset feedback visual
            if (sender is Border border)
            {
                border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                border.BorderThickness = new Thickness(2);
            }

            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items != null && items.Count > 0)
                    {
                        var item = items[0];
                        
                        if (item is StorageFolder folder)
                        {
                            _onPathSelected(folder.Path);
                        }
                        else if (item is StorageFile file)
                        {
                            // Se for arquivo, usa o diretório pai
                            try
                            {
                                var parentFolder = await file.GetParentAsync();
                                if (parentFolder != null)
                                {
                                    _onPathSelected(parentFolder.Path);
                                }
                            }
                            catch
                            {
                                _onError("Não foi possível obter a pasta do arquivo.", InfoBarSeverity.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _onError($"Erro ao processar arquivo: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        /// <summary>
        /// Manipula o evento DragLeave de um Border.
        /// </summary>
        /// <param name="sender">Border que recebeu o evento</param>
        /// <param name="e">Argumentos do evento</param>
        public void HandleDragLeave(object sender, DragEventArgs e)
        {
            // Reset feedback visual quando drag sai
            if (sender is Border border)
            {
                border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                border.BorderThickness = new Thickness(2);
            }
        }
    }
}
