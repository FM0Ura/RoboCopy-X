using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace RoboCopy_X.Helpers
{
    public static class StyledDialogHelper
    {
        public enum DialogType
        {
            Error,
            Warning,
            Info,
            Success,
            Question
        }

        public static async Task<ContentDialogResult> ShowStyledDialogAsync(
            string title,
            string message,
            DialogType dialogType,
            XamlRoot xamlRoot,
            string? primaryButtonText = null,
            string? secondaryButtonText = null,
            string closeButtonText = "OK")
        {
            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = CreateStyledTitle(title, dialogType),
                Content = CreateStyledContent(message, dialogType),
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                CloseButtonText = closeButtonText,
                DefaultButton = string.IsNullOrEmpty(primaryButtonText) ? ContentDialogButton.Close : ContentDialogButton.Primary
            };

            // Aplicar estilo baseado no tipo
            ApplyDialogStyle(dialog, dialogType);

            return await dialog.ShowAsync();
        }

        private static FrameworkElement CreateStyledTitle(string title, DialogType dialogType)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12
            };

            // Adicionar ícone
            var icon = new FontIcon
            {
                FontSize = 24,
                Glyph = GetIconGlyph(dialogType),
                Foreground = GetIconBrush(dialogType)
            };

            stackPanel.Children.Add(icon);

            // Adicionar título
            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(titleText);

            return stackPanel;
        }

        private static FrameworkElement CreateStyledContent(string message, DialogType dialogType)
        {
            var scrollViewer = new ScrollViewer
            {
                MaxHeight = 400,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var border = new Border
            {
                Background = GetContentBackground(dialogType),
                BorderBrush = GetBorderBrush(dialogType),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 8, 0, 0)
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                LineHeight = 22
            };

            border.Child = textBlock;
            scrollViewer.Content = border;

            return scrollViewer;
        }

        private static void ApplyDialogStyle(ContentDialog dialog, DialogType dialogType)
        {
            // Configurar botões baseado no tipo
            switch (dialogType)
            {
                case DialogType.Error:
                    if (string.IsNullOrEmpty(dialog.PrimaryButtonText))
                    {
                        dialog.CloseButtonText = "Entendi";
                    }
                    break;

                case DialogType.Warning:
                    if (string.IsNullOrEmpty(dialog.PrimaryButtonText))
                    {
                        dialog.CloseButtonText = "OK";
                    }
                    break;

                case DialogType.Question:
                    if (string.IsNullOrEmpty(dialog.PrimaryButtonText))
                    {
                        dialog.PrimaryButtonText = "Sim";
                        dialog.CloseButtonText = "Não";
                    }
                    break;
            }
        }

        private static string GetIconGlyph(DialogType dialogType)
        {
            return dialogType switch
            {
                DialogType.Error => "\uE783",      // ErrorBadge
                DialogType.Warning => "\uE7BA",    // Warning
                DialogType.Info => "\uE946",       // Info
                DialogType.Success => "\uE73E",    // CheckMark
                DialogType.Question => "\uE9CE",   // Help
                _ => "\uE946"
            };
        }

        private static Brush GetIconBrush(DialogType dialogType)
        {
            return dialogType switch
            {
                DialogType.Error => new SolidColorBrush(Microsoft.UI.Colors.Red),
                DialogType.Warning => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                DialogType.Info => new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                DialogType.Success => new SolidColorBrush(Microsoft.UI.Colors.Green),
                DialogType.Question => new SolidColorBrush(Microsoft.UI.Colors.Purple),
                _ => new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
        }

        private static Brush GetContentBackground(DialogType dialogType)
        {
            var opacity = 0.05;
            return dialogType switch
            {
                DialogType.Error => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 255, 0, 0)),
                DialogType.Warning => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 255, 165, 0)),
                DialogType.Info => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 30, 144, 255)),
                DialogType.Success => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 0, 128, 0)),
                DialogType.Question => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 128, 0, 128)),
                _ => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 128, 128, 128))
            };
        }

        private static Brush GetBorderBrush(DialogType dialogType)
        {
            var opacity = 0.3;
            return dialogType switch
            {
                DialogType.Error => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 255, 0, 0)),
                DialogType.Warning => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 255, 165, 0)),
                DialogType.Info => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 30, 144, 255)),
                DialogType.Success => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 0, 128, 0)),
                DialogType.Question => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 128, 0, 128)),
                _ => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                    (byte)(255 * opacity), 128, 128, 128))
            };
        }

        // Métodos de conveniência
        public static Task<ContentDialogResult> ShowErrorAsync(
            string title,
            string message,
            XamlRoot xamlRoot,
            string closeButtonText = "Entendi")
        {
            return ShowStyledDialogAsync(title, message, DialogType.Error, xamlRoot, 
                closeButtonText: closeButtonText);
        }

        public static Task<ContentDialogResult> ShowWarningAsync(
            string title,
            string message,
            XamlRoot xamlRoot,
            string? primaryButtonText = null,
            string closeButtonText = "OK")
        {
            return ShowStyledDialogAsync(title, message, DialogType.Warning, xamlRoot,
                primaryButtonText: primaryButtonText,
                closeButtonText: closeButtonText);
        }

        public static Task<ContentDialogResult> ShowInfoAsync(
            string title,
            string message,
            XamlRoot xamlRoot,
            string closeButtonText = "OK")
        {
            return ShowStyledDialogAsync(title, message, DialogType.Info, xamlRoot,
                closeButtonText: closeButtonText);
        }

        public static Task<ContentDialogResult> ShowSuccessAsync(
            string title,
            string message,
            XamlRoot xamlRoot,
            string closeButtonText = "Ótimo!")
        {
            return ShowStyledDialogAsync(title, message, DialogType.Success, xamlRoot,
                closeButtonText: closeButtonText);
        }

        public static Task<ContentDialogResult> ShowQuestionAsync(
            string title,
            string message,
            XamlRoot xamlRoot,
            string primaryButtonText = "Sim",
            string closeButtonText = "Não")
        {
            return ShowStyledDialogAsync(title, message, DialogType.Question, xamlRoot,
                primaryButtonText: primaryButtonText,
                closeButtonText: closeButtonText);
        }
    }
}
