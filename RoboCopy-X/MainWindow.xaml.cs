using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RoboCopy_X
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private Process? _currentProcess;
        private CancellationTokenSource? _cancellationTokenSource;
        private int _maxThreadCount;
        
        // File conflict choice
        private FileConflictChoice? _fileConflictChoice = null;
        
        // Admin mode state
        private bool _isAdminMode = false;
        private bool _wasRunningAsAdminOnStart = false;
        
        // Progress tracking
        private DateTime _operationStartTime;
        private int _totalFiles = 0;
        private int _processedFiles = 0;
        
        // Settings
        private ComboBox? _themeComboBox;
        private CheckBox? _micaBackdropCheckBox;
        private CheckBox? _autoCloseInfoBarCheckBox;
        private CheckBox? _confirmExecutionCheckBox;
        private CheckBox? _saveLastPathsCheckBox;
        private ComboBox? _defaultThreadsComboBox;
        private CheckBox? _defaultCopySubdirsCheckBox;
        private CheckBox? _defaultMultiThreadCheckBox;
        private TextBlock? _systemInfoTextBlock;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);
            
            // Set minimum window size
            var appWindow = AppWindow;
            if (appWindow != null)
            {
                var presenter = appWindow.Presenter as OverlappedPresenter;
                if (presenter != null)
                {
                    presenter.IsResizable = true;
                    presenter.IsMaximizable = true;
                    presenter.IsMinimizable = true;
                }
            }
            
            // Check if already running as admin
            InitializeAdminMode();
            
            // Ensure logs directory exists
            EnsureLogsDirectoryExists();
            
            // Detect system thread count
            InitializeThreadOptions();
            
            // Initialize retry options
            InitializeRetryOptions();
            
            // Initialize wait time options
            InitializeWaitTimeOptions();
            
            // Initialize with default enabled state
            UpdateThreadCountState();
        }

        private void EnsureLogsDirectoryExists()
        {
            try
            {
                var logsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logsPath))
                {
                    Directory.CreateDirectory(logsPath);
                }
            }
            catch (Exception ex)
            {
                // If we can't create the directory, log will be disabled
                System.Diagnostics.Debug.WriteLine($"Failed to create logs directory: {ex.Message}");
            }
        }

        private string GetLogFilePath()
        {
            var logsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return System.IO.Path.Combine(logsPath, $"robocopy_log_{timestamp}.txt");
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = CreateSettingsDialog();
            settingsDialog.XamlRoot = this.Content.XamlRoot;
            await settingsDialog.ShowAsync();
        }

        private ContentDialog CreateSettingsDialog()
        {
            var scrollViewer = new ScrollViewer { MaxHeight = 600 };
            var mainStack = new StackPanel { Spacing = 24, Padding = new Thickness(4) };

            // Appearance Section
            var appearanceStack = new StackPanel { Spacing = 12 };
            appearanceStack.Children.Add(new TextBlock 
            { 
                Text = "Aparência", 
                FontSize = 18, 
                FontWeight = FontWeights.SemiBold 
            });

            var themeStack = new StackPanel { Spacing = 8 };
            themeStack.Children.Add(new TextBlock { Text = "Tema da aplicação", FontWeight = FontWeights.SemiBold });
            
            _themeComboBox = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
            _themeComboBox.Items.Add(new ComboBoxItem { Content = "Escuro", Tag = "Dark" });
            _themeComboBox.Items.Add(new ComboBoxItem { Content = "Claro", Tag = "Light" });
            _themeComboBox.Items.Add(new ComboBoxItem { Content = "Sistema (padrão)", Tag = "Default", IsSelected = true });
            _themeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;
            themeStack.Children.Add(_themeComboBox);
            themeStack.Children.Add(new TextBlock 
            { 
                Text = "Define o tema de cores da interface", 
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            appearanceStack.Children.Add(themeStack);

            var micaStack = new StackPanel { Spacing = 8 };
            micaStack.Children.Add(new TextBlock { Text = "Transparência da janela", FontWeight = FontWeights.SemiBold });
            _micaBackdropCheckBox = new CheckBox 
            { 
                Content = "Ativar efeito Mica (Windows 11)", 
                IsChecked = true 
            };
            _micaBackdropCheckBox.Checked += MicaBackdropCheckBox_Changed;
            _micaBackdropCheckBox.Unchecked += MicaBackdropCheckBox_Changed;
            micaStack.Children.Add(_micaBackdropCheckBox);
            micaStack.Children.Add(new TextBlock 
            { 
                Text = "Desative se houver problemas de desempenho", 
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            appearanceStack.Children.Add(micaStack);
            
            mainStack.Children.Add(appearanceStack);
            mainStack.Children.Add(new Rectangle 
            { 
                Height = 1, 
                Fill = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"] 
            });

            // Behavior Section
            var behaviorStack = new StackPanel { Spacing = 12 };
            behaviorStack.Children.Add(new TextBlock 
            { 
                Text = "Comportamento", 
                FontSize = 18, 
                FontWeight = FontWeights.SemiBold 
            });

            var autoCloseStack = new StackPanel { Spacing = 8 };
            _autoCloseInfoBarCheckBox = new CheckBox 
            { 
                Content = "Fechar notificações de sucesso automaticamente", 
                IsChecked = true 
            };
            autoCloseStack.Children.Add(_autoCloseInfoBarCheckBox);
            autoCloseStack.Children.Add(new TextBlock 
            { 
                Text = "Notificações de sucesso fecham após 5 segundos", 
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            behaviorStack.Children.Add(autoCloseStack);

            var confirmStack = new StackPanel { Spacing = 8 };
            _confirmExecutionCheckBox = new CheckBox 
            { 
                Content = "Confirmar antes de executar operações", 
                IsChecked = false 
            };
            confirmStack.Children.Add(_confirmExecutionCheckBox);
            confirmStack.Children.Add(new TextBlock 
            { 
                Text = "Mostra diálogo de confirmação antes de executar o Robocopy", 
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            behaviorStack.Children.Add(confirmStack);

            var savePathsStack = new StackPanel { Spacing = 8 };
            _saveLastPathsCheckBox = new CheckBox 
            { 
                Content = "Lembrar últimos caminhos usados", 
                IsChecked = true 
            };
            savePathsStack.Children.Add(_saveLastPathsCheckBox);
            savePathsStack.Children.Add(new TextBlock 
            { 
                Text = "Salva os últimos caminhos de origem e destino", 
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            behaviorStack.Children.Add(savePathsStack);

            mainStack.Children.Add(behaviorStack);
            mainStack.Children.Add(new Rectangle 
            { 
                Height = 1, 
                Fill = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"] 
            });

            // Default Values Section
            var defaultsStack = new StackPanel { Spacing = 12 };
            defaultsStack.Children.Add(new TextBlock 
            { 
                Text = "Valores Padrão", 
                FontSize = 18, 
                FontWeight = FontWeights.SemiBold 
            });

            var threadsStack = new StackPanel { Spacing = 8 };
            threadsStack.Children.Add(new TextBlock { Text = "Threads padrão", FontWeight = FontWeights.SemiBold });
            _defaultThreadsComboBox = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
            _defaultThreadsComboBox.Items.Add(new ComboBoxItem { Content = "Usar recomendação do sistema", Tag = "Auto", IsSelected = true });
            _defaultThreadsComboBox.Items.Add(new ComboBoxItem { Content = "1 thread", Tag = "1" });
            _defaultThreadsComboBox.Items.Add(new ComboBoxItem { Content = "4 threads", Tag = "4" });
            _defaultThreadsComboBox.Items.Add(new ComboBoxItem { Content = "8 threads", Tag = "8" });
            _defaultThreadsComboBox.Items.Add(new ComboBoxItem { Content = "16 threads", Tag = "16" });
            threadsStack.Children.Add(_defaultThreadsComboBox);
            defaultsStack.Children.Add(threadsStack);

            _defaultCopySubdirsCheckBox = new CheckBox 
            { 
                Content = "Copiar subdiretórios por padrão (/E)", 
                IsChecked = false 
            };
            defaultsStack.Children.Add(new StackPanel { Spacing = 8, Children = { _defaultCopySubdirsCheckBox } });

            _defaultMultiThreadCheckBox = new CheckBox 
            { 
                Content = "Ativar multi-thread por padrão (/MT)", 
                IsChecked = true 
            };
            defaultsStack.Children.Add(new StackPanel { Spacing = 8, Children = { _defaultMultiThreadCheckBox } });

            mainStack.Children.Add(defaultsStack);
            mainStack.Children.Add(new Rectangle 
            { 
                Height = 1, 
                Fill = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"] 
            });

            // About Section
            var aboutStack = new StackPanel { Spacing = 12 };
            aboutStack.Children.Add(new TextBlock 
            { 
                Text = "Sobre", 
                FontSize = 18, 
                FontWeight = FontWeights.SemiBold 
            });

            var versionStack = new StackPanel { Spacing = 4 };
            versionStack.Children.Add(new TextBlock { Text = "RoboCopy-X", FontWeight = FontWeights.SemiBold });
            versionStack.Children.Add(new TextBlock { Text = "Versão 1.0.0", FontSize = 12 });
            versionStack.Children.Add(new TextBlock 
            { 
                Text = "Interface gráfica moderna para Robocopy", 
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            aboutStack.Children.Add(versionStack);

            var sysInfoStack = new StackPanel { Spacing = 8, Margin = new Thickness(0, 8, 0, 0) };
            sysInfoStack.Children.Add(new TextBlock { Text = "Informações do Sistema", FontWeight = FontWeights.SemiBold });
            
            var systemInfo = new StringBuilder();
            systemInfo.AppendLine($"Processadores: {Environment.ProcessorCount} núcleos lógicos");
            systemInfo.AppendLine($"Sistema: {Environment.OSVersion}");
            systemInfo.AppendLine($".NET: {Environment.Version}");
            systemInfo.AppendLine($"Arquitetura: {RuntimeInformation.ProcessArchitecture}");
            
            _systemInfoTextBlock = new TextBlock 
            { 
                Text = systemInfo.ToString(),
                FontSize = 12,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            };
            sysInfoStack.Children.Add(_systemInfoTextBlock);
            aboutStack.Children.Add(sysInfoStack);

            var githubLink = new HyperlinkButton 
            { 
                Content = "Ver documentação no GitHub",
                NavigateUri = new Uri("https://github.com/FM0Ura/RoboCopy-X"),
                Margin = new Thickness(0, 8, 0, 0)
            };
            aboutStack.Children.Add(githubLink);

            mainStack.Children.Add(aboutStack);
            mainStack.Children.Add(new Rectangle 
            { 
                Height = 1, 
                Fill = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"] 
            });

            // Reset Section
            var resetStack = new StackPanel { Spacing = 12 };
            resetStack.Children.Add(new TextBlock 
            { 
                Text = "Redefinir", 
                FontSize = 18, 
                FontWeight = FontWeights.SemiBold 
            });

            var resetButton = new Button 
            { 
                Content = "Restaurar configurações padrão",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            resetButton.Click += ResetSettingsButton_Click;
            resetStack.Children.Add(resetButton);
            resetStack.Children.Add(new TextBlock 
            { 
                Text = "Esta ação não pode ser desfeita", 
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            mainStack.Children.Add(resetStack);

            scrollViewer.Content = mainStack;

            return new ContentDialog
            {
                Title = "Configurações",
                Content = scrollViewer,
                CloseButtonText = "Fechar",
                DefaultButton = ContentDialogButton.Close
            };
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item && item.Tag is string theme)
            {
                var rootElement = this.Content as FrameworkElement;
                if (rootElement != null)
                {
                    rootElement.RequestedTheme = theme switch
                    {
                        "Dark" => ElementTheme.Dark,
                        "Light" => ElementTheme.Light,
                        _ => ElementTheme.Default
                    };
                }
            }
        }

        private void MicaBackdropCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.IsChecked == true)
            {
                this.SystemBackdrop = new MicaBackdrop();
            }
            else
            {
                this.SystemBackdrop = null;
            }
        }

        private async void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirmar redefinição",
                Content = "Tem certeza que deseja restaurar todas as configurações padrão?\n\nEsta ação não pode ser desfeita.",
                PrimaryButtonText = "Sim, restaurar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ResetToDefaultSettings();
                ShowInfoBar("Configurações restauradas para os valores padrão.", InfoBarSeverity.Success);
            }
        }

        private void InitializeSettingsDialog()
        {
            // Method is no longer needed as dialog is created dynamically
        }

        private void ResetToDefaultSettings()
        {
            // Reset theme
            if (_themeComboBox != null)
                _themeComboBox.SelectedIndex = 2; // Sistema
            
            // Reset appearance
            if (_micaBackdropCheckBox != null)
                _micaBackdropCheckBox.IsChecked = true;
            
            // Reset behavior
            if (_autoCloseInfoBarCheckBox != null)
                _autoCloseInfoBarCheckBox.IsChecked = true;
            if (_confirmExecutionCheckBox != null)
                _confirmExecutionCheckBox.IsChecked = false;
            if (_saveLastPathsCheckBox != null)
                _saveLastPathsCheckBox.IsChecked = true;
            
            // Reset default values
            if (_defaultThreadsComboBox != null)
                _defaultThreadsComboBox.SelectedIndex = 0; // Auto
            if (_defaultCopySubdirsCheckBox != null)
                _defaultCopySubdirsCheckBox.IsChecked = false;
            if (_defaultMultiThreadCheckBox != null)
                _defaultMultiThreadCheckBox.IsChecked = true;
            
            // Apply defaults to main form
            MultiThreadCheckBox.IsChecked = true;
            CopySubdirectoriesCheckBox.IsChecked = false;
        }

        // Drag and Drop for Source
        private void SourceDropBorder_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.Caption = "Soltar para definir origem";
            
            // Check if we have storage items
            var hasStorageItems = e.DataView.Contains(StandardDataFormats.StorageItems);
            
            if (!hasStorageItems)
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }

            // Visual feedback
            if (sender is Border border && hasStorageItems)
            {
                border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
                border.BorderThickness = new Thickness(3);
            }
        }

        private async void SourceDropBorder_Drop(object sender, DragEventArgs e)
        {
            // Reset visual feedback
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
                            SetSourcePath(folder.Path);
                        }
                        else if (item is StorageFile file)
                        {
                            // If it's a file, use its directory
                            try
                            {
                                var parentFolder = await file.GetParentAsync();
                                if (parentFolder != null)
                                {
                                    SetSourcePath(parentFolder.Path);
                                }
                            }
                            catch
                            {
                                ShowInfoBar("Não foi possível obter a pasta do arquivo.", InfoBarSeverity.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Erro ao processar arquivo: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private void SourceDropBorder_DragLeave(object sender, DragEventArgs e)
        {
            // Reset visual feedback when drag leaves
            if (sender is Border border)
            {
                border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                border.BorderThickness = new Thickness(2);
            }
        }

        private async void SourceDropBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var folder = await PickFolderAsync();
            if (folder != null)
            {
                SetSourcePath(folder.Path);
            }
        }

        // Drag and Drop for Destination
        private void DestinationDropBorder_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.Caption = "Soltar para definir destino";
            
            // Check if we have storage items
            var hasStorageItems = e.DataView.Contains(StandardDataFormats.StorageItems);
            
            if (!hasStorageItems)
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }

            // Visual feedback
            if (sender is Border border && hasStorageItems)
            {
                border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
                border.BorderThickness = new Thickness(3);
            }
        }

        private async void DestinationDropBorder_Drop(object sender, DragEventArgs e)
        {
            // Reset visual feedback
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
                            SetDestinationPath(folder.Path);
                        }
                        else if (item is StorageFile file)
                        {
                            // If it's a file, use its directory
                            try
                            {
                                var parentFolder = await file.GetParentAsync();
                                if (parentFolder != null)
                                {
                                    SetDestinationPath(parentFolder.Path);
                                }
                            }
                            catch
                            {
                                ShowInfoBar("Não foi possível obter a pasta do arquivo.", InfoBarSeverity.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Erro ao processar arquivo: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private void DestinationDropBorder_DragLeave(object sender, DragEventArgs e)
        {
            // Reset visual feedback when drag leaves
            if (sender is Border border)
            {
                border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                border.BorderThickness = new Thickness(2);
            }
        }

        private async void DestinationDropBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var folder = await PickFolderAsync();
            if (folder != null)
            {
                SetDestinationPath(folder.Path);
            }
        }

        // Helper methods to set paths
        private void SetSourcePath(string path)
        {
            SourcePathTextBox.Text = path;
            
            // Update visual elements directly
            if (SourcePathText != null)
            {
                SourcePathText.Text = path;
                SourcePathText.Visibility = Visibility.Visible;
            }
            
            if (SourcePlaceholder != null)
            {
                SourcePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void SetDestinationPath(string path)
        {
            DestinationPathTextBox.Text = path;
            
            // Update visual Elements directly
            if (DestinationPathText != null)
            {
                DestinationPathText.Text = path;
                DestinationPathText.Visibility = Visibility.Visible;
            }
            
            if (DestinationPlaceholder != null)
            {
                DestinationPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<StorageFolder?> PickFolderAsync()
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            return await folderPicker.PickSingleFolderAsync();
        }

        private void MirrorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Show warning when mirror is enabled
            ShowInfoBar("Atenção: O modo espelho (/MIR) irá deletar arquivos no destino que não existem na origem!", 
                       InfoBarSeverity.Warning);
        }

        private void MultiThreadCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateThreadCountState();
        }

        private void MultiThreadCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateThreadCountState();
        }

        private void ThreadCountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateThreadCountValue();
        }

        private void RetryCountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRetryCountValue();
        }

        private void WaitTimeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateWaitTimeValue();
        }

        private void UpdateThreadCountState()
        {
            if (ThreadCountComboBox != null)
            {
                ThreadCountComboBox.IsEnabled = MultiThreadCheckBox.IsChecked == true;
            }
            
            if (ThreadCountNumberBox != null)
            {
                ThreadCountNumberBox.IsEnabled = MultiThreadCheckBox.IsChecked == true;
            }
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reset file conflict choice for new operation
                _fileConflictChoice = null;
                
                // Validate inputs (agora assíncrono)
                if (!await ValidateInputsAsync())
                {
                    return;
                }

                // Build command
                var command = BuildRobocopyCommand();
                CommandPreviewTextBox.Text = command;

                // Show confirmation dialog if enabled
                if (_confirmExecutionCheckBox?.IsChecked == true)
                {
                    var result = await ShowQuestionDialogAsync(
                        "Confirmar Execução",
                        $"Deseja executar o seguinte comando?\n\n{command}",
                        "Executar",
                        "Cancelar");

                    if (result != ContentDialogResult.Primary)
                    {
                        // Reset file conflict choice if user cancels
                        _fileConflictChoice = null;
                        return;
                    }
                }

                // Update UI
                ExecuteButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                
                // Initialize progress tracking
                _operationStartTime = DateTime.Now;
                _totalFiles = 0;
                _processedFiles = 0;
                
                // Show progress section
                if (ProgressSection != null)
                {
                    ProgressSection.Visibility = Visibility.Visible;
                }
                
                // Initialize progress
                if (ProgressBar != null)
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = 0;
                }
                
                if (ProgressPercentageText != null)
                {
                    ProgressPercentageText.Text = "0%";
                }
                
                if (ProgressStatusText != null)
                {
                    ProgressStatusText.Text = "Preparando transferência...";
                }
                
                OutputTextBlock.Text = "Iniciando Robocopy...\n\n";

                // Execute
                _cancellationTokenSource = new CancellationTokenSource();
                await ExecuteRobocopyAsync(command, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                AppendOutput("\n\n=== OPERAÇÃO CANCELADA ===\n");
                ShowInfoBar("Operação cancelada pelo usuário.", InfoBarSeverity.Warning);
            }
            catch (Exception ex)
            {
                AppendOutput($"\n\n=== ERRO ===\n{ex.Message}\n");
                await ShowErrorDialogAsync("Erro ao executar Robocopy", ex.Message);
            }
            finally
            {
                // Reset UI
                ExecuteButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
                
                // Hide progress section
                if (ProgressSection != null)
                {
                    ProgressSection.Visibility = Visibility.Collapsed;
                }
                
                // Reset progress
                if (ProgressBar != null)
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = 0;
                }
                
                _currentProcess = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                
                // Reset file conflict choice after operation completes
                _fileConflictChoice = null;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                
                if (_currentProcess != null && !_currentProcess.HasExited)
                {
                    _currentProcess.Kill(true);
                    AppendOutput("\n\nCancelando operação...\n");
                }
            }
            catch (Exception ex)
            {
                _ = ShowErrorDialogAsync("Erro ao cancelar", ex.Message);
            }
        }

        private void ClearOutputButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBlock.Text = "Pronto para executar...";
        }

        private async Task<bool> ValidateInputsAsync()
        {
            var source = SourcePathTextBox.Text;
            var destination = DestinationPathTextBox.Text;
            
            // 1. Validações básicas de existência
            if (string.IsNullOrWhiteSpace(source))
            {
                ShowInfoBar("Por favor, especifique o caminho de origem.", InfoBarSeverity.Error);
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(destination))
            {
                ShowInfoBar("Por favor, especifique o caminho de destino.", InfoBarSeverity.Error);
                return false;
            }
            
            // 2. Verificar se origem existe
            if (!Directory.Exists(source))
            {
                ShowInfoBar("O caminho de origem não existe!", InfoBarSeverity.Error);
                return false;
            }
            
            // 3. CRÍTICA #1: Origem e destino diferentes
            var (isDifferent, diffError) = RoboCopy_X.Helpers.PathValidator.ValidateDifferentPaths(source, destination);
            if (!isDifferent)
            {
                await ShowErrorDialogAsync("Caminhos Iguais", diffError ?? "Origem e destino não podem ser iguais.");
                return false;
            }
            
            // 4. CRÍTICA #2: Destino não está dentro da origem
            var (isNotNested, nestError) = RoboCopy_X.Helpers.PathValidator.ValidateNestedPaths(source, destination);
            if (!isNotNested)
            {
                await ShowErrorDialogAsync("Hierarquia Inválida", nestError ?? "Destino não pode estar dentro da origem.");
                return false;
            }
            
            // 5. CRÍTICA #3: Verificar permissão de leitura na origem
            var (canRead, readError) = RoboCopy_X.Helpers.PathValidator.CheckReadPermission(source);
            if (!canRead)
            {
                await ShowErrorDialogAsync("Sem Permissão de Leitura", readError ?? "Não foi possível acessar a pasta de origem.");
                return false;
            }
            
            // 6. CRÍTICA #4: Verificar permissão de escrita no destino
            var (canWrite, writeError) = RoboCopy_X.Helpers.PathValidator.CheckWritePermission(destination);
            if (!canWrite)
            {
                await ShowErrorDialogAsync("Sem Permissão de Escrita", writeError ?? "Não foi possível acessar a pasta de destino.");
                return false;
            }
            
            // 7. CRÍTICA #5: Verificar se não são caminhos de sistema (ORIGEM)
            var (isSafeSource, sourceWarning) = RoboCopy_X.Helpers.PathValidator.CheckSystemPath(source);
            if (!isSafeSource && sourceWarning != null)
            {
                var result = await ShowWarningDialogAsync(
                    "Aviso de Segurança - Origem",
                    sourceWarning,
                    "Continuar Mesmo Assim",
                    "Cancelar");
                
                if (result != ContentDialogResult.Primary)
                    return false;
            }
            
            // 8. CRÍTICA #5: Verificar se não são caminhos de sistema (DESTINO)
            var (isSafeDest, destWarning) = RoboCopy_X.Helpers.PathValidator.CheckSystemPath(destination);
            if (!isSafeDest && destWarning != null)
            {
                var result = await ShowWarningDialogAsync(
                    "Aviso de Segurança - Destino",
                    destWarning,
                    "Continuar Mesmo Assim",
                    "Cancelar");
                
                if (result != ContentDialogResult.Primary)
                    return false;
            }
            
            // 9. CRÍTICA #6: Verificar se arquivos já existem no destino
            var sourceFolderName = System.IO.Path.GetFileName(source.TrimEnd('\\', '/'));
            var finalDestination = System.IO.Path.Combine(destination, sourceFolderName);
            
            if (Directory.Exists(finalDestination))
            {
                var (hasConflicts, conflictInfo) = RoboCopy_X.Helpers.PathValidator.CheckExistingFiles(source, finalDestination);
                
                if (hasConflicts)
                {
                    var choice = await ShowFileConflictDialogAsync(conflictInfo);
                    
                    if (choice == FileConflictChoice.Cancel)
                    {
                        return false;
                    }
                    
                    // Aplicar escolha do usuário
                    ApplyFileConflictChoice(choice);
                }
            }
            
            // 10. Avisar se destino não existe
            if (!Directory.Exists(destination))
            {
                ShowInfoBar("Aviso: O caminho de destino não existe e será criado.", InfoBarSeverity.Warning);
            }
            
            // 11. Verificar espaço em disco
            try
            {
                var includeSubdirs = CopySubdirectoriesCheckBox.IsChecked == true;
                var (isValid, errorMessage, requiredSpace, availableSpace) = 
                    RoboCopy_X.Helpers.PathValidator.ValidateDiskSpace(source, destination, includeSubdirs);
                
                if (!isValid)
                {
                    await ShowErrorDialogAsync("Espaço Insuficiente", errorMessage ?? "Não há espaço suficiente no disco de destino.");
                    return false;
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowInfoBar(errorMessage, InfoBarSeverity.Informational);
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Aviso: Não foi possível verificar espaço em disco. {ex.Message}", InfoBarSeverity.Warning);
            }
            
            return true;
        }

        /// <summary>
        /// Escolha do usuário sobre como lidar com conflitos de arquivos.
        /// </summary>
        private enum FileConflictChoice
        {
            /// <summary>
            /// Sobrescrever todos os arquivos existentes no destino.
            /// Usa o comportamento padrão do Robocopy.
            /// </summary>
            Overwrite,
            
            /// <summary>
            /// Ignorar/pular TODOS os arquivos que já existem no destino.
            /// Usa as flags /XC /XN /XO do Robocopy para pular arquivos:
            /// - /XC: Exclui arquivos alterados
            /// - /XN: Exclui arquivos mais novos
            /// - /XO: Exclui arquivos mais antigos
            /// </summary>
            Skip,
            
            /// <summary>
            /// Cancelar a operação.
            /// </summary>
            Cancel
        }

        private async Task<FileConflictChoice> ShowFileConflictDialogAsync(string conflictInfo)
        {
            var contentPanel = new StackPanel
            {
                Spacing = 16,
                Margin = new Thickness(0, 8, 0, 8)
            };

            // Warning icon and message
            var warningStack = new StackPanel
            {
                Spacing = 12,
                Orientation = Orientation.Horizontal
            };

            var warningIcon = new FontIcon
            {
                Glyph = "\uE7BA", // Warning icon
                FontSize = 32,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe Fluent Icons"),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Orange),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 4, 0, 0)
            };

            var messageStack = new StackPanel
            {
                Spacing = 8
            };

            var mainMessage = new TextBlock
            {
                Text = "Arquivos já existem no destino!",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            };

            var detailMessage = new TextBlock
            {
                Text = "A pasta de destino já contém arquivos da origem.\nComo deseja proceder?",
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            };

            messageStack.Children.Add(mainMessage);
            messageStack.Children.Add(detailMessage);

            warningStack.Children.Add(warningIcon);
            warningStack.Children.Add(messageStack);
            contentPanel.Children.Add(warningStack);

            // Conflict info section
            var infoBorder = new Border
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["LayerFillColorDefaultBrush"],
                BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 8, 0, 0)
            };

            var infoText = new TextBlock
            {
                Text = conflictInfo,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
            };

            infoBorder.Child = infoText;
            contentPanel.Children.Add(infoBorder);

            // Options explanation
            var optionsStack = new StackPanel
            {
                Spacing = 8,
                Margin = new Thickness(0, 16, 0, 0)
            };

            var optionsTitle = new TextBlock
            {
                Text = "Opções disponíveis:",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };

            var option1 = new TextBlock
            {
                Text = "• Sobrescrever: Substitui todos os arquivos existentes",
                FontSize = 13,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            };

            var option2 = new TextBlock
            {
                Text = "• Ignorar: Pula TODOS os arquivos que já existem no destino",
                FontSize = 13,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            };

            optionsStack.Children.Add(optionsTitle);
            optionsStack.Children.Add(option1);
            optionsStack.Children.Add(option2);
            contentPanel.Children.Add(optionsStack);

            var dialog = new ContentDialog
            {
                Title = "Conflito de Arquivos",
                Content = contentPanel,
                PrimaryButtonText = "Sobrescrever",
                SecondaryButtonText = "Ignorar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot
            };

            // Criar estilo customizado para o botão primário com fundo vermelho
            var primaryButtonStyle = new Style(typeof(Button));
            primaryButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Microsoft.UI.Colors.Red)));
            primaryButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(Microsoft.UI.Colors.White)));

            // Aplicar estilo ao ContentDialog através de recursos
            dialog.Resources["ContentDialogPrimaryButtonStyle"] = primaryButtonStyle;

            // Também tentar aplicar via evento Loaded com múltiplas tentativas
            dialog.Loaded += async (s, e) =>
            {
                // Aguardar um pouco para garantir que a árvore visual está carregada
                await Task.Delay(50);
                
                try
                {
                    // Tentar encontrar e estilizar o botão primário
                    var primaryButton = FindVisualChild<Button>(dialog, "PrimaryButton");
                    if (primaryButton != null)
                    {
                        primaryButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Red);
                        primaryButton.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                        primaryButton.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                    }
                    else
                    {
                        // Tentar busca alternativa sem filtro de nome
                        var buttons = FindVisualChildren<Button>(dialog);
                        if (buttons.Count >= 1)
                        {
                            // O primeiro botão geralmente é o Primary
                            var btn = buttons[0];
                            btn.Background = new SolidColorBrush(Microsoft.UI.Colors.Red);
                            btn.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                            btn.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                        }
                    }
                }
                catch
                {
                    // Silenciosamente falhar se não conseguir aplicar o estilo
                }
            };

            var result = await dialog.ShowAsync();

            return result switch
            {
                ContentDialogResult.Primary => FileConflictChoice.Overwrite,
                ContentDialogResult.Secondary => FileConflictChoice.Skip,
                _ => FileConflictChoice.Cancel
            };
        }

        // Helper method melhorado para encontrar elementos na árvore visual
        private T? FindVisualChild<T>(DependencyObject parent, string? childName = null) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    if (string.IsNullOrEmpty(childName))
                        return typedChild;

                    if (child is FrameworkElement fe && fe.Name == childName)
                        return typedChild;
                }

                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                    return result;
            }

            return null;
        }

        // Helper method para encontrar todos os elementos de um tipo
        private List<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var children = new List<T>();
            if (parent == null) return children;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    children.Add(typedChild);
                }

                children.AddRange(FindVisualChildren<T>(child));
            }

            return children;
        }

        // Manter método antigo para compatibilidade
        private T? FindDescendant<T>(DependencyObject parent, Func<T, bool>? predicate = null) where T : DependencyObject
        {
            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild && (predicate == null || predicate(typedChild)))
                {
                    return typedChild;
                }

                var descendant = FindDescendant<T>(child, predicate);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        private async Task<ContentDialogResult> ShowErrorDialogAsync(string title, string message)
        {
            return await Helpers.StyledDialogHelper.ShowErrorAsync(title, message, this.Content.XamlRoot);
        }

        private async Task<ContentDialogResult> ShowWarningDialogAsync(string title, string message, 
            string? primaryButtonText = null, string closeButtonText = "OK")
        {
            return await Helpers.StyledDialogHelper.ShowWarningAsync(
                title, message, this.Content.XamlRoot, primaryButtonText, closeButtonText);
        }

        private async Task<ContentDialogResult> ShowInfoDialogAsync(string title, string message)
        {
            return await Helpers.StyledDialogHelper.ShowInfoAsync(title, message, this.Content.XamlRoot);
        }

        private async Task<ContentDialogResult> ShowSuccessDialogAsync(string title, string message)
        {
            return await Helpers.StyledDialogHelper.ShowSuccessAsync(title, message, this.Content.XamlRoot);
        }

        private async Task<ContentDialogResult> ShowQuestionDialogAsync(string title, string message,
            string primaryButtonText = "Sim", string closeButtonText = "Não")
        {
            return await Helpers.StyledDialogHelper.ShowQuestionAsync(
                title, message, this.Content.XamlRoot, primaryButtonText, closeButtonText);
        }

        private string BuildRobocopyCommand()
        {
            var sb = new StringBuilder();
            
            // Get the source folder name to preserve folder structure
            var sourceFolderName = System.IO.Path.GetFileName(SourcePathTextBox.Text.TrimEnd('\\', '/'));
            var destinationWithFolder = System.IO.Path.Combine(DestinationPathTextBox.Text, sourceFolderName);
            
            // Add source and destination (destination includes source folder name)
            sb.Append($"\"{SourcePathTextBox.Text}\" \"{destinationWithFolder}\"");

            // Copy subdirectories
            if (CopySubdirectoriesCheckBox.IsChecked == true)
            {
                sb.Append(" /E");
            }

            // Mirror
            if (MirrorCheckBox.IsChecked == true)
            {
                sb.Append(" /MIR");
            }

            // CRÍTICO: Aplicar flags de conflito de arquivos ANTES de outras flags
            if (_fileConflictChoice == FileConflictChoice.Skip)
            {
                // Para ignorar TODOS os arquivos existentes, usar /XC /XN /XO
                // /XC = Exclui arquivos alterados (Changed)
                // /XN = Exclui arquivos mais novos (Newer) 
                // /XO = Exclui arquivos mais antigos (Older)
                // Juntas, essas flags fazem o Robocopy ignorar qualquer arquivo que já existe
                sb.Append(" /XC /XN /XO");
            }
            else
            {
                // Exclude older (apenas se não estiver no modo Skip)
                if (ExcludeOlderCheckBox.IsChecked == true)
                {
                    sb.Append(" /XO");
                }
            }

            // Copy attributes
            var copyFlags = new StringBuilder();
            if (CopyDataCheckBox.IsChecked == true) copyFlags.Append("D");
            if (CopyAttributesCheckBox.IsChecked == true) copyFlags.Append("A");
            if (CopyTimestampsCheckBox.IsChecked == true) copyFlags.Append("T");
            if (CopySecurityCheckBox.IsChecked == true) copyFlags.Append("S");
            if (CopyOwnerCheckBox.IsChecked == true) copyFlags.Append("O");
            if (CopyAuditCheckBox.IsChecked == true) copyFlags.Append("U");

            if (copyFlags.Length > 0)
            {
                sb.Append($" /COPY:{copyFlags}");
            }

            // Multi-thread
            if (MultiThreadCheckBox.IsChecked == true)
            {
                var threadCount = (int)ThreadCountNumberBox.Value;
                sb.Append($" /MT:{threadCount}");
            }

            // Retry count
            sb.Append($" /R:{(int)RetryCountNumberBox.Value}");

            // Wait time
            sb.Append($" /W:{(int)WaitTimeNumberBox.Value}");

            // SEMPRE adicionar saída detalhada (verbose) para logs completos
            sb.Append(" /V");
            
            // NUNCA adicionar /NP - sempre mostrar porcentagem de progresso

            // Always add log file
            var logPath = GetLogFilePath();
            sb.Append($" /LOG:\"{logPath}\"");
            
            // Store log path for later reference
            CommandPreviewTextBox.Text = $"Log será salvo em: {logPath}";

            return sb.ToString();
        }

        private async Task ExecuteRobocopyAsync(string arguments, CancellationToken cancellationToken)
        {
            string? destinationPath = null;
            
            // Extract destination path from arguments for later use
            try
            {
                var sourceFolderName = System.IO.Path.GetFileName(SourcePathTextBox.Text.TrimEnd('\\', '/'));
                destinationPath = System.IO.Path.Combine(DestinationPathTextBox.Text, sourceFolderName);
            }
            catch
            {
                destinationPath = DestinationPathTextBox.Text;
            }
            
            await Task.Run(() =>
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
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            AppendOutput(e.Data + "\n");
                            
                            // Extract percentage from output (Robocopy shows: "X.X%")
                            var percentMatch = System.Text.RegularExpressions.Regex.Match(e.Data, @"(\d+(?:\.\d+)?)\s*%");
                            if (percentMatch.Success && double.TryParse(percentMatch.Groups[1].Value, out double percent))
                            {
                                UpdateProgress(percent);
                            }
                            
                            // Extract file being copied - Multiple patterns for better detection
                            // Pattern 1: Standard Robocopy output format with file status
                            var filePattern1 = System.Text.RegularExpressions.Regex.Match(
                                e.Data, 
                                @"^\s*(?:New File|Newer|Older|same|modified|\*EXTRA File)\s+[\d\.,]+\s*[KMG]?B?\s+(.+)$",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                            );
                            
                            // Pattern 2: File path at the end of the line
                            var filePattern2 = System.Text.RegularExpressions.Regex.Match(
                                e.Data,
                                @"\s+([^\s]+\.\w{2,4})\s*$"
                            );
                            
                            // Pattern 3: Full path detection
                            var filePattern3 = System.Text.RegularExpressions.Regex.Match(
                                e.Data,
                                @"[A-Za-z]:\\(?:[^\\/:*?""<>|\r\n]+\\)*([^\\/:*?""<>|\r\n]+\.\w+)"
                            );
                            
                            string? fileName = null;
                            
                            if (filePattern1.Success)
                            {
                                fileName = filePattern1.Groups[1].Value.Trim();
                            }
                            else if (filePattern3.Success)
                            {
                                // Get just the filename from full path
                                fileName = filePattern3.Groups[1].Value.Trim();
                            }
                            else if (filePattern2.Success && 
                                    (e.Data.Contains("New File") || 
                                     e.Data.Contains("Newer") || 
                                     e.Data.Contains("Older") ||
                                     e.Data.Contains("modified") ||
                                     e.Data.Contains("same")))
                            {
                                fileName = filePattern2.Groups[1].Value.Trim();
                            }
                            
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                // Increment processed files counter
                                _processedFiles++;
                                
                                // Truncate very long filenames
                                if (fileName.Length > 50)
                                {
                                    fileName = "..." + fileName.Substring(fileName.Length - 47);
                                }
                                UpdateProgressStatus($"📄 Copiando: {fileName}");
                                
                                // Update details if we know total files
                                if (_totalFiles > 0 && ProgressDetailsText != null)
                                {
                                    DispatcherQueue.TryEnqueue(() =>
                                    {
                                        ProgressDetailsText.Text = $"Arquivo {_processedFiles} de {_totalFiles}";
                                        ProgressDetailsText.Visibility = Visibility.Visible;
                                    });
                                }
                            }
                            
                            // Try to extract total files count from summary
                            var totalFilesMatch = System.Text.RegularExpressions.Regex.Match(
                                e.Data,
                                @"Files\s*:\s*(\d+)"
                            );
                            if (totalFilesMatch.Success && int.TryParse(totalFilesMatch.Groups[1].Value, out int total))
                            {
                                if (_totalFiles == 0 && total > 0)
                                {
                                    _totalFiles = total;
                                }
                            }
                });

                    }
                };

                _currentProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            AppendOutput($"ERRO: {e.Data}\n");
                        });
                    }
                };

                try
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        AppendOutput($"Executando: robocopy.exe {arguments}\n");
                        AppendOutput("═══════════════════════════════════════════\n\n");
                        
                        // Extract log path from arguments
                        var logMatch = System.Text.RegularExpressions.Regex.Match(arguments, @"/LOG:""([^""]+)""");
                        if (logMatch.Success)
                        {
                            AppendOutput($"📋 Log será salvo em: {logMatch.Groups[1].Value}\n\n");
                        }
                    });

                    _currentProcess.Start();
                    _currentProcess.BeginOutputReadLine();
                    _currentProcess.BeginErrorReadLine();

                    // Wait for process to exit or cancellation
                    while (!_currentProcess.WaitForExit(100))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    var exitCode = _currentProcess.ExitCode;

                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        AppendOutput($"\n═══════════════════════════════════════════\n");
                        AppendOutput($"Código de saída: {exitCode}\n");
                        AppendOutput(GetRobocopyExitCodeDescription(exitCode) + "\n");

                        if (exitCode < 8)
                        {
                            ShowInfoBar("Operação concluída com sucesso!", InfoBarSeverity.Success);
                            
                            // Show completion dialog with option to open destination folder
                            await ShowCompletionDialogAsync(destinationPath);
                        }
                        else
                        {
                            ShowInfoBar("Operação concluída com erros. Verifique a saída.", InfoBarSeverity.Warning);
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        AppendOutput($"\n\nERRO: {ex.Message}\n");
                        await ShowErrorDialogAsync("Erro durante execução", ex.Message);
                    });
                    throw;
                }
            }, cancellationToken);
        }

        private async Task ShowCompletionDialogAsync(string? destinationPath)
        {
            // Create a beautiful styled content panel
            var contentPanel = new StackPanel
            {
                Spacing = 20,
                Margin = new Thickness(0, 8, 0, 8)
            };

            // Success icon and message
            var messageStack = new StackPanel
            {
                Spacing = 12,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Use FontIcon with Segoe Fluent Icons for proper rendering
            var successIcon = new FontIcon
            {
                Glyph = "\uE73E", // CheckMark icon from Segoe Fluent Icons
                FontSize = 48,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe Fluent Icons"),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.LimeGreen)
            };

            var successMessage = new TextBlock
            {
                Text = "Operação concluída com sucesso!",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            messageStack.Children.Add(successIcon);
            messageStack.Children.Add(successMessage);
            contentPanel.Children.Add(messageStack);

            // Destination path section
            if (!string.IsNullOrEmpty(destinationPath))
            {
                var pathBorder = new Border
                {
                    Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["LayerFillColorDefaultBrush"],
                    BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(16, 12, 16, 12),
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var pathStack = new StackPanel { Spacing = 6 };

                var destinationLabel = new TextBlock
                {
                    Text = "Pasta de Destino:",
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                };

                var destinationPathText = new TextBlock
                {
                    Text = destinationPath,
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    IsTextSelectionEnabled = true,
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
                };

                pathStack.Children.Add(destinationLabel);
                pathStack.Children.Add(destinationPathText);
                pathBorder.Child = pathStack;
                contentPanel.Children.Add(pathBorder);
            }

            var dialog = new ContentDialog
            {
                Title = "Operação Concluída",
                Content = contentPanel,
                PrimaryButtonText = "Abrir Pasta de Destino",
                CloseButtonText = "Fechar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(destinationPath))
            {
                try
                {
                    // Open destination folder in Windows Explorer
                    if (Directory.Exists(destinationPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{destinationPath}\"",
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        ShowInfoBar("A pasta de destino não foi encontrada.", InfoBarSeverity.Warning);
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorDialogAsync("Erro ao abrir pasta", $"Não foi possível abrir a pasta de destino: {ex.Message}");
                }
            }
        }

        private string GetRobocopyExitCodeDescription(int exitCode)
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

        private void AppendOutput(string text)
        {
            OutputTextBlock.Text += text;
            OutputScrollViewer.ChangeView(null, OutputScrollViewer.ScrollableHeight, null);
        }

        private void ShowInfoBar(string message, InfoBarSeverity severity)
        {
            InfoBarMessage.Message = message;
            InfoBarMessage.Severity = severity;
            InfoBarMessage.IsOpen = true;

            // Auto-close after 5 seconds for success messages if enabled
            if (severity == InfoBarSeverity.Success && _autoCloseInfoBarCheckBox?.IsChecked == true)
            {
                var timer = DispatcherQueue.CreateTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += (s, e) =>
                {
                    InfoBarMessage.IsOpen = false;
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void UpdateProgress(double percentage)
        {
            if (ProgressBar != null)
            {
                ProgressBar.Value = Math.Min(100, Math.Max(0, percentage));
            }
            
            if (ProgressPercentageText != null)
            {
                ProgressPercentageText.Text = $"{percentage:F1}%";
            }
            
            // Calculate estimated time remaining
            if (percentage > 0 && percentage < 100)
            {
                var elapsed = DateTime.Now - _operationStartTime;
                var estimatedTotal = elapsed.TotalSeconds / (percentage / 100.0);
                var remaining = TimeSpan.FromSeconds(estimatedTotal - elapsed.TotalSeconds);
                
                if (remaining.TotalSeconds > 0 && ProgressStatusText != null)
                {
                    string timeText;
                    if (remaining.TotalHours >= 1)
                    {
                        timeText = $"{remaining.Hours}h {remaining.Minutes}m restantes";
                    }
                    else if (remaining.TotalMinutes >= 1)
                    {
                        timeText = $"{remaining.Minutes}m {remaining.Seconds}s restantes";
                    }
                    else
                    {
                        timeText = $"{remaining.Seconds}s restantes";
                    }
                    
                    // Only update time estimate if not showing a specific file
                    // This prevents overwriting the "Copiando: arquivo.txt" message
                    if (!ProgressStatusText.Text.StartsWith("📄") && 
                        !ProgressStatusText.Text.Contains("Copiando:"))
                    {
                        ProgressStatusText.Text = $"⏱️ Transferindo... {timeText}";
                    }
                }
            }
            else if (percentage >= 100 && ProgressStatusText != null)
            {
                ProgressStatusText.Text = "✅ Operação concluída!";
            }
        }

        private void UpdateProgressStatus(string status)
        {
            if (ProgressStatusText != null)
            {
                ProgressStatusText.Text = status;
            }
        }

        private void InitializeThreadOptions()
        {
            // Get processor count (logical processors)
            _maxThreadCount = Environment.ProcessorCount;
            
            // Robocopy supports 1-128 threads
            int maxRobocopyThreads = Math.Min(_maxThreadCount * 2, 128);
            
            // Populate ComboBox with thread options
            var threadOptions = new List<ComboBoxItem>();
            
            // Add common presets
            var presets = new[] { 1, 2, 4, 8, 16, 32, 64, 128 };
            
            foreach (var count in presets)
            {
                if (count <= maxRobocopyThreads)
                {
                    var item = new ComboBoxItem
                    {
                        Content = count == _maxThreadCount 
                            ? $"{count} threads (recomendado para este sistema)" 
                            : count == _maxThreadCount * 2
                                ? $"{count} threads (2x núcleos)"
                                : $"{count} threads",
                        Tag = count
                    };
                    
                    threadOptions.Add(item);
                    
                    // Select the processor count as default
                    if (count == _maxThreadCount)
                    {
                        ThreadCountComboBox.SelectedItem = item;
                    }
                }
            }
            
            // If system has unusual thread count, add it
            if (!presets.Contains(_maxThreadCount))
            {
                var systemItem = new ComboBoxItem
                {
                    Content = $"{_maxThreadCount} threads (recomendado para este sistema)",
                    Tag = _maxThreadCount
                };
                
                // Insert in sorted position
                int insertIndex = threadOptions.FindIndex(t => (int)t.Tag > _maxThreadCount);
                if (insertIndex == -1)
                    threadOptions.Add(systemItem);
                else
                    threadOptions.Insert(insertIndex, systemItem);
                    
                ThreadCountComboBox.SelectedItem = systemItem;
            }
            
            // Set items
            ThreadCountComboBox.ItemsSource = threadOptions;
            
            // If nothing selected, select first item
            if (ThreadCountComboBox.SelectedIndex == -1 && threadOptions.Count > 0)
            {
                ThreadCountComboBox.SelectedIndex = 0;
            }
            
            // Update the hidden NumberBox
            UpdateThreadCountValue();
        }

        private void InitializeRetryOptions()
        {
            var retryOptions = new List<ComboBoxItem>();
            
            // Add 1 as first option
            retryOptions.Add(new ComboBoxItem
            {
                Content = "1 tentativa",
                Tag = 1
            });
            
            // Add intervals of 10 from 10 to 100
            for (int i = 10; i <= 100; i += 10)
            {
                retryOptions.Add(new ComboBoxItem
                {
                    Content = $"{i} tentativas",
                    Tag = i
                });
            }
            
            // Set items
            RetryCountComboBox.ItemsSource = retryOptions;
            
            // Select 10 as default (index 1)
            RetryCountComboBox.SelectedIndex = 1;
            
            // Update the hidden NumberBox
            UpdateRetryCountValue();
        }

        private void InitializeWaitTimeOptions()
        {
            var waitTimeOptions = new List<ComboBoxItem>();
            
            // Add common wait time options in seconds
            var waitTimes = new[] { 1, 5, 10, 15, 30, 60, 120, 180, 300, 600 };
            
            foreach (var seconds in waitTimes)
            {
                string displayText;
                if (seconds < 60)
                {
                    displayText = $"{seconds} segundo{(seconds == 1 ? "" : "s")}";
                }
                else
                {
                    int minutes = seconds / 60;
                    displayText = $"{minutes} minuto{(minutes == 1 ? "" : "s")} ({seconds}s)";
                }
                
                waitTimeOptions.Add(new ComboBoxItem
                {
                    Content = displayText,
                    Tag = seconds
                });
            }
            
            // Set items
            WaitTimeComboBox.ItemsSource = waitTimeOptions;
            
            // Select 5 seconds as default (index 1)
            WaitTimeComboBox.SelectedIndex = 1;
            
            // Update the hidden NumberBox
            UpdateWaitTimeValue();
        }

        private void UpdateThreadCountValue()
        {
            if (ThreadCountComboBox.SelectedItem is ComboBoxItem item && item.Tag is int count)
            {
                ThreadCountNumberBox.Value = count;
            }
        }

        private void UpdateRetryCountValue()
        {
            if (RetryCountComboBox.SelectedItem is ComboBoxItem item && item.Tag is int count)
            {
                RetryCountNumberBox.Value = count;
            }
        }

        private void UpdateWaitTimeValue()
        {
            if (WaitTimeComboBox.SelectedItem is ComboBoxItem item && item.Tag is int seconds)
            {
                WaitTimeNumberBox.Value = seconds;
            }
        }

        private void InitializeAdminMode()
        {
            // Verificar se já está rodando como admin
            _wasRunningAsAdminOnStart = Helpers.AdminPrivilegeHelper.IsRunningAsAdmin();
            _isAdminMode = _wasRunningAsAdminOnStart;
            
            // Atualizar UI (será chamado após InitializeComponent)
            UpdateAdminModeUI();
            
            // Se já estiver como admin, configurar o toggle
            if (_wasRunningAsAdminOnStart)
            {
                if (AdminModeToggle != null)
                {
                    AdminModeToggle.IsChecked = true;
                    AdminModeToggle.IsEnabled = false;
                }
                
                var (username, _) = Helpers.AdminPrivilegeHelper.GetCurrentUserInfo();
                
                // Usar DispatcherQueue para mostrar a mensagem após a UI estar pronta
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                {
                    ShowInfoBar($"Aplicação iniciada com privilégios de Administrador (usuário: {username})", 
                               InfoBarSeverity.Informational);
                });
            }
        }

        private void UpdateAdminModeUI()
        {
            if (AdminModeToggle == null) return;
            
            // Atualizar conteúdo visual baseado no estado
            if (_isAdminMode)
            {
                if (AdminModeOnContent != null)
                    AdminModeOnContent.Visibility = Visibility.Visible;
                if (AdminModeOffContent != null)
                    AdminModeOffContent.Visibility = Visibility.Collapsed;
                
                // Atualizar título da janela
                this.Title = "RoboCopy-X (Administrador)";
            }
            else
            {
                if (AdminModeOnContent != null)
                    AdminModeOnContent.Visibility = Visibility.Collapsed;
                if (AdminModeOffContent != null)
                    AdminModeOffContent.Visibility = Visibility.Visible;
                
                // Título normal
                this.Title = "RoboCopy-X";
            }
        }

        private async void AdminModeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton toggle) return;
            
            // Se já estava como admin no início, não permitir desativar
            if (_wasRunningAsAdminOnStart)
            {
                toggle.IsChecked = true;
                await ShowInfoDialogAsync(
                    "Modo Administrador Ativo",
                    "A aplicação foi iniciada com privilégios de administrador.\n\n" +
                    "Para remover os privilégios, feche e inicie a aplicação normalmente.");
                return;
            }
            
            if (toggle.IsChecked == true)
            {
                // Usuário quer ativar modo admin
                await ActivateAdminModeAsync();
            }
            else
            {
                // Usuário quer desativar (teoricamente não deveria chegar aqui se já estava admin)
                await DeactivateAdminModeAsync();
            }
        }

        private async Task ActivateAdminModeAsync()
        {
            // Confirmar com o usuário
            var result = await ShowWarningDialogAsync(
                "Ativar Modo Administrador",
                "Para ativar o modo administrador, a aplicação precisa ser reiniciada.\n\n" +
                "⚠️ Todas as operações em andamento serão canceladas.\n\n" +
                "Deseja continuar?",
                "Sim, Reiniciar",
                "Cancelar");
            
            if (result != ContentDialogResult.Primary)
            {
                // Usuário cancelou, voltar o toggle
                AdminModeToggle.IsChecked = false;
                UpdateAdminModeUI();
                return;
            }
            
            // Tentar reiniciar como admin
            var success = Helpers.AdminPrivilegeHelper.RestartAsAdmin();
            
            if (success)
            {
                // Fechar a aplicação atual (a nova instância admin será aberta)
                Application.Current.Exit();
            }
            else
            {
                // Falhou ao reiniciar
                AdminModeToggle.IsChecked = false;
                UpdateAdminModeUI();
                
                await ShowErrorDialogAsync(
                    "Falha ao Ativar Modo Admin",
                    "Não foi possível reiniciar a aplicação com privilégios de administrador.\n\n" +
                    "Possíveis causas:\n" +
                    "• Operação cancelada pelo controle de conta de usuário (UAC)\n" +
                    "• Permissões insuficientes no sistema\n" +
                    "• Falha ao localizar o executável da aplicação");
            }
        }

        private async Task DeactivateAdminModeAsync()
        {
            // Teoricamente isso não deveria ser chamado, mas vamos tratar
            AdminModeToggle.IsChecked = true;
            
            await ShowInfoDialogAsync(
                "Não é Possível Desativar",
                "Uma vez que a aplicação está rodando com privilégios de administrador, " +
                "não é possível removê-los durante a execução.\n\n" +
                "Para executar sem privilégios de admin, feche e inicie a aplicação normalmente.");
        }

        private void ApplyFileConflictChoice(FileConflictChoice choice)
        {
            // Armazenar a escolha para uso posterior na construção do comando
            _fileConflictChoice = choice;
            
            switch (choice)
            {
                case FileConflictChoice.Overwrite:
                    // Sobrescrever: usa comportamento padrão do Robocopy (substitui)
                    // Desmarcar o ExcludeOlderCheckBox para garantir que sobrescreva
                    if (ExcludeOlderCheckBox != null)
                    {
                        ExcludeOlderCheckBox.IsChecked = false;
                    }
                    ShowInfoBar("Modo: Sobrescrever - Arquivos existentes serão substituídos.", InfoBarSeverity.Informational);
                    break;

                case FileConflictChoice.Skip:
                    // Ignorar: NÃO marcar apenas ExcludeOlderCheckBox
                    // As flags /XC /XN /XO serão adicionadas diretamente no BuildRobocopyCommand
                    ShowInfoBar("Modo: Ignorar - Arquivos existentes serão mantidos, apenas novos serão copiados.", InfoBarSeverity.Informational);
                    break;
            }
        }
    }
}

