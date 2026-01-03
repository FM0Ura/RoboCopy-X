using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using RoboCopy_X.Services;
using RoboCopy_X.Helpers;
using IOPath = System.IO.Path;

namespace RoboCopy_X
{
    /// <summary>
    /// Janela principal da aplicação RoboCopy-X.
    /// Fornece interface gráfica moderna para o utilitário Robocopy do Windows.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        #region Private Fields

        // Process management
        private Process? _currentProcess;
        private CancellationTokenSource? _cancellationTokenSource;
        private int _maxThreadCount;

        // Services
        private UIService? _uiService;
        private ProgressTracker? _progressTracker;
        private DragDropHandler? _sourceDragDropHandler;
        private DragDropHandler? _destinationDragDropHandler;

        // File conflict management
        private FileConflictChoice? _fileConflictChoice = null;

        // Admin mode state
        private bool _isAdminMode = false;
        private bool _wasRunningAsAdminOnStart = false;

        // Settings UI controls
        private ComboBox? _themeComboBox;
        private CheckBox? _micaBackdropCheckBox;
        private CheckBox? _autoCloseInfoBarCheckBox;
        private CheckBox? _confirmExecutionCheckBox;
        private CheckBox? _saveLastPathsCheckBox;
        private ComboBox? _defaultThreadsComboBox;
        private CheckBox? _defaultCopySubdirsCheckBox;
        private CheckBox? _defaultMultiThreadCheckBox;
        private TextBlock? _systemInfoTextBlock;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa uma nova instância da janela principal.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            ConfigureWindow();
            InitializeServices();
            InitializeAdminMode();
            EnsureLogsDirectoryExists();
            InitializeComboBoxes();
        }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Configura propriedades da janela.
        /// </summary>
        private void ConfigureWindow()
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);

            // Configurar tamanho mínimo e comportamento da janela
            var appWindow = AppWindow;
            if (appWindow?.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = true;
                presenter.IsMaximizable = true;
                presenter.IsMinimizable = true;
            }
        }

        /// <summary>
        /// Inicializa os serviços da aplicação.
        /// </summary>
        private void InitializeServices()
        {
            // UI Service
            _uiService = new UIService(
                InfoBarMessage,
                OutputScrollViewer,
                OutputTextBlock,
                DispatcherQueue,
                autoCloseSuccessMessages: true
            );

            // Progress Tracker
            _progressTracker = new ProgressTracker(
                ProgressBar,
                ProgressPercentageText,
                ProgressStatusText,
                ProgressDetailsText,
                ProgressSection
            );

            // Drag & Drop Handlers
            _sourceDragDropHandler = new DragDropHandler(
                onPathSelected: SetSourcePath,
                onError: (msg, severity) => _uiService?.ShowInfoBar(msg, severity)
            );

            _destinationDragDropHandler = new DragDropHandler(
                onPathSelected: SetDestinationPath,
                onError: (msg, severity) => _uiService?.ShowInfoBar(msg, severity)
            );
        }

        /// <summary>
        /// Inicializa as ComboBoxes de opções.
        /// </summary>
        private void InitializeComboBoxes()
        {
            InitializeThreadOptions();
            InitializeRetryOptions();
            InitializeWaitTimeOptions();
            UpdateThreadCountState();
        }

        #endregion

        #region Log Management

        /// <summary>
        /// Garante que o diretório de logs existe.
        /// </summary>
        private void EnsureLogsDirectoryExists()
        {
            try
            {
                var logsPath = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logsPath))
                {
                    Directory.CreateDirectory(logsPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create logs directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Gera um caminho único para o arquivo de log.
        /// </summary>
        /// <returns>Caminho completo para o arquivo de log</returns>
        private string GetLogFilePath()
        {
            var logsPath = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return IOPath.Combine(logsPath, $"robocopy_log_{timestamp}.txt");
        }

        #endregion

        #region Event Handlers

        #region UI Event Handlers

        /// <summary>
        /// Manipula o clique no botão de configurações.
        /// </summary>
        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = CreateSettingsDialog();
            settingsDialog.XamlRoot = this.Content.XamlRoot;
            await settingsDialog.ShowAsync();
        }

        /// <summary>
        /// Manipula a marcação da checkbox de modo espelho.
        /// Exibe aviso sobre a exclusão de arquivos.
        /// </summary>
        private void MirrorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _uiService?.ShowInfoBar(
                "Atenção: O modo espelho (/MIR) irá deletar arquivos no destino que não existem na origem!",
                InfoBarSeverity.Warning);
        }

        /// <summary>
        /// Manipula a marcação da checkbox de multi-thread.
        /// </summary>
        private void MultiThreadCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateThreadCountState();
        }

        /// <summary>
        /// Manipula a desmarcação da checkbox de multi-thread.
        /// </summary>
        private void MultiThreadCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateThreadCountState();
        }

        /// <summary>
        /// Manipula mudança de seleção no ComboBox de threads.
        /// </summary>
        private void ThreadCountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateThreadCountValue();
        }

        /// <summary>
        /// Manipula mudança de seleção no ComboBox de tentativas.
        /// </summary>
        private void RetryCountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRetryCountValue();
        }

        /// <summary>
        /// Manipula mudança de seleção no ComboBox de tempo de espera.
        /// </summary>
        private void WaitTimeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateWaitTimeValue();
        }

        /// <summary>
        /// Manipula o clique no botão de limpar saída.
        /// </summary>
        private void ClearOutputButton_Click(object sender, RoutedEventArgs e)
        {
            _uiService?.SetOutput("Pronto para executar...");
        }

        /// <summary>
        /// Manipula o clique no botão de fechar do card de espaço em disco.
        /// </summary>
        private void DiskSpaceCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiskSpaceInfoCard != null)
            {
                DiskSpaceInfoCard.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Atualiza o estado habilitado/desabilitado dos controles de thread count.
        /// </summary>
        private void UpdateThreadCountState()
        {
            bool isEnabled = MultiThreadCheckBox.IsChecked == true;

            if (ThreadCountComboBox != null)
            {
                ThreadCountComboBox.IsEnabled = isEnabled;
            }

            if (ThreadCountNumberBox != null)
            {
                ThreadCountNumberBox.IsEnabled = isEnabled;
            }
        }

        #endregion

        #endregion

        #region Settings Dialog

        /// <summary>
        /// Cria o diálogo de configurações com todas as opções.
        /// </summary>
        /// <returns>ContentDialog configurado</returns>
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
            mainStack.Children.Add(new Microsoft.UI.Xaml.Shapes.Rectangle 
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
            mainStack.Children.Add(new Microsoft.UI.Xaml.Shapes.Rectangle 
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
            mainStack.Children.Add(new Microsoft.UI.Xaml.Shapes.Rectangle 
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
            mainStack.Children.Add(new Microsoft.UI.Xaml.Shapes.Rectangle 
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

        /// <summary>
        /// Manipula mudança de tema.
        /// </summary>
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

        /// <summary>
        /// Manipula mudança do efeito Mica.
        /// </summary>
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

        /// <summary>
        /// Manipula clique no botão de resetar configurações.
        /// </summary>
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
                _uiService?.ShowInfoBar("Configurações restauradas para os valores padrão.", InfoBarSeverity.Success);
            }
        }

        /// <summary>
        /// Reseta todas as configurações para os valores padrão.
        /// </summary>
        private void ResetToDefaultSettings()
        {
            if (_themeComboBox != null)
                _themeComboBox.SelectedIndex = 2;
            
            if (_micaBackdropCheckBox != null)
                _micaBackdropCheckBox.IsChecked = true;
            
            if (_autoCloseInfoBarCheckBox != null)
                _autoCloseInfoBarCheckBox.IsChecked = true;
            if (_confirmExecutionCheckBox != null)
                _confirmExecutionCheckBox.IsChecked = false;
            if (_saveLastPathsCheckBox != null)
                _saveLastPathsCheckBox.IsChecked = true;
            
            if (_defaultThreadsComboBox != null)
                _defaultThreadsComboBox.SelectedIndex = 0;
            if (_defaultCopySubdirsCheckBox != null)
                _defaultCopySubdirsCheckBox.IsChecked = false;
            if (_defaultMultiThreadCheckBox != null)
                _defaultMultiThreadCheckBox.IsChecked = true;
            
            // Apply defaults to main form
            MultiThreadCheckBox.IsChecked = true;
            CopySubdirectoriesCheckBox.IsChecked = false;
        }

        #endregion

        #region Thread/Retry/Wait Options

        /// <summary>
        /// Inicializa as opções de threads baseado no hardware do sistema.
        /// </summary>
        private void InitializeThreadOptions()
        {
            _maxThreadCount = Environment.ProcessorCount;
            int maxRobocopyThreads = Math.Min(_maxThreadCount * 2, 128);
            var threadOptions = new List<ComboBoxItem>();
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
                    
                    if (count == _maxThreadCount)
                    {
                        ThreadCountComboBox.SelectedItem = item;
                    }
                }
            }
            
            if (!presets.Contains(_maxThreadCount))
            {
                var systemItem = new ComboBoxItem
                {
                    Content = $"{_maxThreadCount} threads (recomendado para este sistema)",
                    Tag = _maxThreadCount
                };
                
                int insertIndex = threadOptions.FindIndex(t => (int)t.Tag > _maxThreadCount);
                if (insertIndex == -1)
                    threadOptions.Add(systemItem);
                else
                    threadOptions.Insert(insertIndex, systemItem);
                    
                ThreadCountComboBox.SelectedItem = systemItem;
            }
            
            ThreadCountComboBox.ItemsSource = threadOptions;
            
            if (ThreadCountComboBox.SelectedIndex == -1 && threadOptions.Count > 0)
            {
                ThreadCountComboBox.SelectedIndex = 0;
            }
            
            UpdateThreadCountValue();
        }

        /// <summary>
        /// Inicializa as opções de número de tentativas.
        /// </summary>
        private void InitializeRetryOptions()
        {
            var retryOptions = new List<ComboBoxItem>();
            
            retryOptions.Add(new ComboBoxItem
            {
                Content = "1 tentativa",
                Tag = 1
            });
            
            for (int i = 10; i <= 100; i += 10)
            {
                retryOptions.Add(new ComboBoxItem
                {
                    Content = $"{i} tentativas",
                    Tag = i
                });
            }
            
            RetryCountComboBox.ItemsSource = retryOptions;
            RetryCountComboBox.SelectedIndex = 1;
            UpdateRetryCountValue();
        }

        /// <summary>
        /// Inicializa as opções de tempo de espera entre tentativas.
        /// </summary>
        private void InitializeWaitTimeOptions()
        {
            var waitTimeOptions = new List<ComboBoxItem>();
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
            
            WaitTimeComboBox.ItemsSource = waitTimeOptions;
            WaitTimeComboBox.SelectedIndex = 1;
            UpdateWaitTimeValue();
        }

        /// <summary>
        /// Atualiza o valor do NumberBox de threads baseado na seleção do ComboBox.
        /// </summary>
        private void UpdateThreadCountValue()
        {
            if (ThreadCountComboBox.SelectedItem is ComboBoxItem item && item.Tag is int count)
            {
                ThreadCountNumberBox.Value = count;
            }
        }

        /// <summary>
        /// Atualiza o valor do NumberBox de tentativas baseado na seleção do ComboBox.
        /// </summary>
        private void UpdateRetryCountValue()
        {
            if (RetryCountComboBox.SelectedItem is ComboBoxItem item && item.Tag is int count)
            {
                RetryCountNumberBox.Value = count;
            }
        }

        /// <summary>
        /// Atualiza o valor do NumberBox de tempo de espera baseado na seleção do ComboBox.
        /// </summary>
        private void UpdateWaitTimeValue()
        {
            if (WaitTimeComboBox.SelectedItem is ComboBoxItem item && item.Tag is int seconds)
            {
                WaitTimeNumberBox.Value = seconds;
            }
        }

        #endregion

        #region Admin Mode

        /// <summary>
        /// Inicializa o modo administrador verificando se a aplicação já está rodando como admin.
        /// </summary>
        private void InitializeAdminMode()
        {
            _wasRunningAsAdminOnStart = AdminPrivilegeHelper.IsRunningAsAdmin();
            _isAdminMode = _wasRunningAsAdminOnStart;
            
            UpdateAdminModeUI();
            
            if (_wasRunningAsAdminOnStart)
            {
                if (AdminModeToggle != null)
                {
                    AdminModeToggle.IsChecked = true;
                    AdminModeToggle.IsEnabled = false;
                }
                
                var (username, _) = AdminPrivilegeHelper.GetCurrentUserInfo();
                
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                {
                    _uiService?.ShowInfoBar($"Aplicação iniciada com privilégios de Administrador (usuário: {username})", 
                               InfoBarSeverity.Informational);
                });
            }
        }

        /// <summary>
        /// Atualiza a UI para refletir o estado do modo administrador.
        /// </summary>
        private void UpdateAdminModeUI()
        {
            if (AdminModeToggle == null) return;
            
            if (_isAdminMode)
            {
                if (AdminModeOnContent != null)
                    AdminModeOnContent.Visibility = Visibility.Visible;
                if (AdminModeOffContent != null)
                    AdminModeOffContent.Visibility = Visibility.Collapsed;
                
                this.Title = "RoboCopy-X (Administrador)";
            }
            else
            {
                if (AdminModeOnContent != null)
                    AdminModeOnContent.Visibility = Visibility.Collapsed;
                if (AdminModeOffContent != null)
                    AdminModeOffContent.Visibility = Visibility.Visible;
                
                this.Title = "RoboCopy-X";
            }
        }

        /// <summary>
        /// Manipula o clique no toggle de modo administrador.
        /// </summary>
        private async void AdminModeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton toggle) return;
            
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
                await ActivateAdminModeAsync();
            }
            else
            {
                await DeactivateAdminModeAsync();
            }
        }

        /// <summary>
        /// Ativa o modo administrador reiniciando a aplicação com privilégios elevados.
        /// </summary>
        private async Task ActivateAdminModeAsync()
        {
            var result = await ShowWarningDialogAsync(
                "Ativar Modo Administrador",
                "Para ativar o modo administrador, a aplicação precisa ser reiniciada.\n\n" +
                "⚠️ Todas as operações em andamento serão canceladas.\n\n" +
                "Deseja continuar?",
                "Sim, Reiniciar",
                "Cancelar");
            
            if (result != ContentDialogResult.Primary)
            {
                AdminModeToggle.IsChecked = false;
                UpdateAdminModeUI();
                return;
            }
            
            var success = AdminPrivilegeHelper.RestartAsAdmin();
            
            if (success)
            {
                Application.Current.Exit();
            }
            else
            {
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

        /// <summary>
        /// Tentativa de desativar o modo administrador (não é possível sem reiniciar).
        /// </summary>
        private async Task DeactivateAdminModeAsync()
        {
            AdminModeToggle.IsChecked = true;
            
            await ShowInfoDialogAsync(
                "Não é Possível Desativar",
                "Uma vez que a aplicação está rodando com privilégios de administrador, " +
                "não é possível removê-los durante a execução.\n\n" +
                "Para executar sem privilégios de admin, feche e inicie a aplicação normalmente.");
        }

        #endregion

        #region Robocopy Command Building

        /// <summary>
        /// Constrói o comando do Robocopy baseado nas opções selecionadas.
        /// </summary>
        /// <returns>String com o comando completo</returns>
        private string BuildRobocopyCommand()
        {
            var sb = new StringBuilder();
            
            var sourceFolderName = IOPath.GetFileName(SourcePathTextBox.Text.TrimEnd('\\', '/'));
            var destinationWithFolder = IOPath.Combine(DestinationPathTextBox.Text, sourceFolderName);
            
            sb.Append($"\"{SourcePathTextBox.Text}\" \"{destinationWithFolder}\"");

            if (CopySubdirectoriesCheckBox.IsChecked == true)
            {
                sb.Append(" /E");
            }

            if (MirrorCheckBox.IsChecked == true)
            {
                sb.Append(" /MIR");
            }

            if (_fileConflictChoice == FileConflictChoice.Skip)
            {
                sb.Append(" /XC /XN /XO");
            }
            else
            {
                if (ExcludeOlderCheckBox.IsChecked == true)
                {
                    sb.Append(" /XO");
                }
            }

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

            if (MultiThreadCheckBox.IsChecked == true)
            {
                var threadCount = (int)ThreadCountNumberBox.Value;
                sb.Append($" /MT:{threadCount}");
            }

            sb.Append($" /R:{(int)RetryCountNumberBox.Value}");
            sb.Append($" /W:{(int)WaitTimeNumberBox.Value}");
            sb.Append(" /V");

            var logPath = GetLogFilePath();
            sb.Append($" /LOG:\"{logPath}\"");
            
            CommandPreviewTextBox.Text = $"Log será salvo em: {logPath}";

            return sb.ToString();
        }

        #endregion

        #region Robocopy Execution

        /// <summary>
        /// Executa o comando Robocopy de forma assíncrona.
        /// </summary>
        /// <param name="arguments">Argumentos do comando</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        private async Task ExecuteRobocopyAsync(string arguments, CancellationToken cancellationToken)
        {
            string? destinationPath = null;
            
            try
            {
                var sourceFolderName = IOPath.GetFileName(SourcePathTextBox.Text.TrimEnd('\\', '/'));
                destinationPath = IOPath.Combine(DestinationPathTextBox.Text, sourceFolderName);
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

                _currentProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            _uiService?.AppendOutput(e.Data + Environment.NewLine);
                            
                            var percentMatch = System.Text.RegularExpressions.Regex.Match(e.Data, @"(\d+(?:\.\d+)?)\s*%");
                            if (percentMatch.Success && double.TryParse(percentMatch.Groups[1].Value, out double percent))
                            {
                                _progressTracker?.UpdateProgress(percent);
                            }
                            
                            var filePattern1 = System.Text.RegularExpressions.Regex.Match(
                                e.Data, 
                                @"^\s*(?:New File|Newer|Older|same|modified|\*EXTRA File)\s+[\d\.,]+\s*[KMG]?B?\s+(.+)$",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                            );
                            
                            var filePattern2 = System.Text.RegularExpressions.Regex.Match(
                                e.Data,
                                @"\s+([^\s]+\.\w{2,4})\s*$"
                            );
                            
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
                                _progressTracker?.IncrementProcessedFiles(fileName);
                            }
                            
                            var totalFilesMatch = System.Text.RegularExpressions.Regex.Match(
                                e.Data,
                                @"Files\s*:\s*(\d+)"
                            );
                            if (totalFilesMatch.Success && int.TryParse(totalFilesMatch.Groups[1].Value, out int total))
                            {
                                if (_progressTracker != null && _progressTracker.TotalFiles == 0 && total > 0)
                                {
                                    _progressTracker.TotalFiles = total;
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
                            _uiService?.AppendOutput($"ERRO: {e.Data}{Environment.NewLine}");
                        });
                    }
                };

                try
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        _uiService?.AppendOutput($"Executando: robocopy.exe {arguments}{Environment.NewLine}");
                        _uiService?.AppendOutput("═══════════════════════════════════════════" + Environment.NewLine + Environment.NewLine);
                        
                        var logMatch = System.Text.RegularExpressions.Regex.Match(arguments, @"/LOG:""([^""]+)""");
                        if (logMatch.Success)
                        {
                            _uiService?.AppendOutput($"📋 Log será salvo em: {logMatch.Groups[1].Value}{Environment.NewLine}{Environment.NewLine}");
                        }
                    });

                    _currentProcess.Start();
                    _currentProcess.BeginOutputReadLine();
                    _currentProcess.BeginErrorReadLine();

                    while (!_currentProcess.WaitForExit(100))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    var exitCode = _currentProcess.ExitCode;

                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        _uiService?.AppendOutput(Environment.NewLine + "═══════════════════════════════════════════" + Environment.NewLine);
                        _uiService?.AppendOutput("Código de saída: " + exitCode + Environment.NewLine);
                        _uiService?.AppendOutput(GetRobocopyExitCodeDescription(exitCode) + Environment.NewLine);

                        if (exitCode < 8)
                        {
                            _uiService?.ShowInfoBar("Operação concluída com sucesso!", InfoBarSeverity.Success);
                            await ShowCompletionDialogAsync(destinationPath);
                        }
                        else
                        {
                            _uiService?.ShowInfoBar("Operação concluída com erros. Verifique a saída.", InfoBarSeverity.Warning);
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
                        _uiService?.AppendOutput(Environment.NewLine + Environment.NewLine + "ERRO: " + ex.Message + Environment.NewLine);
                        await ShowErrorDialogAsync("Erro durante execução", ex.Message);
                    });
                    throw;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Exibe diálogo de conclusão com opção de abrir a pasta de destino.
        /// </summary>
        /// <param name="destinationPath">Caminho da pasta de destino</param>
        private async Task ShowCompletionDialogAsync(string? destinationPath)
        {
            var contentPanel = new StackPanel
            {
                Spacing = 20,
                Margin = new Thickness(0, 8, 0, 8)
            };

            var messageStack = new StackPanel
            {
                Spacing = 12,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var successIcon = new FontIcon
            {
                Glyph = "\uE73E",
                FontSize = 48,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe Fluent Icons"),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LimeGreen)
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
                        _uiService?.ShowInfoBar("A pasta de destino não foi encontrada.", InfoBarSeverity.Warning);
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorDialogAsync("Erro ao abrir pasta", $"Não foi possível abrir a pasta de destino: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Obtém a descrição do código de saída do Robocopy.
        /// </summary>
        /// <param name="exitCode">Código de saída</param>
        /// <returns>Descrição do código</returns>
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

        #endregion

        #region Dialog Helpers

        /// <summary>
        /// Exibe um diálogo de erro estilizado.
        /// </summary>
        private async Task<ContentDialogResult> ShowErrorDialogAsync(string title, string message)
        {
            return await StyledDialogHelper.ShowErrorAsync(title, message, this.Content.XamlRoot);
        }

        /// <summary>
        /// Exibe um diálogo de aviso estilizado.
        /// </summary>
        private async Task<ContentDialogResult> ShowWarningDialogAsync(string title, string message, 
            string? primaryButtonText = null, string closeButtonText = "OK")
        {
            return await StyledDialogHelper.ShowWarningAsync(
                title, message, this.Content.XamlRoot, primaryButtonText, closeButtonText);
        }

        /// <summary>
        /// Exibe um diálogo informativo estilizado.
        /// </summary>
        private async Task<ContentDialogResult> ShowInfoDialogAsync(string title, string message)
        {
            return await StyledDialogHelper.ShowInfoAsync(title, message, this.Content.XamlRoot);
        }

        /// <summary>
        /// Exibe um diálogo de sucesso estilizado.
        /// </summary>
        private async Task<ContentDialogResult> ShowSuccessDialogAsync(string title, string message)
        {
            return await StyledDialogHelper.ShowSuccessAsync(title, message, this.Content.XamlRoot);
        }

        /// <summary>
        /// Exibe um diálogo de pergunta estilizado.
        /// </summary>
        private async Task<ContentDialogResult> ShowQuestionDialogAsync(string title, string message,
            string primaryButtonText = "Sim", string closeButtonText = "Não")
        {
            return await StyledDialogHelper.ShowQuestionAsync(
                title, message, this.Content.XamlRoot, primaryButtonText, closeButtonText);
        }

        #endregion

        #region File Conflict Management

        /// <summary>
        /// Escolha do usuário sobre como lidar com conflitos de arquivos.
        /// </summary>
        private enum FileConflictChoice
        {
            /// <summary>
            /// Sobrescrever todos os arquivos existentes no destino.
            /// </summary>
            Overwrite,
            
            /// <summary>
            /// Ignorar/pular TODOS os arquivos que já existem no destino.
            /// </summary>
            Skip,
            
            /// <summary>
            /// Cancelar a operação.
            /// </summary>
            Cancel
        }

        /// <summary>
        /// Exibe diálogo para resolver conflitos de arquivos.
        /// </summary>
        /// <param name="conflictInfo">Informações sobre os conflitos</param>
        /// <returns>Escolha do usuário</returns>
        private async Task<FileConflictChoice> ShowFileConflictDialogAsync(string conflictInfo)
        {
            var contentPanel = new StackPanel
            {
                Spacing = 16,
                Margin = new Thickness(0, 8, 0, 8)
            };

            var warningStack = new StackPanel
            {
                Spacing = 12,
                Orientation = Orientation.Horizontal
            };

            var warningIcon = new FontIcon
            {
                Glyph = "\uE7BA",
                FontSize = 32,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe Fluent Icons"),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 4, 0, 0)
            };

            var messageStack = new StackPanel { Spacing = 8 };
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

            var result = await dialog.ShowAsync();

            return result switch
            {
                ContentDialogResult.Primary => FileConflictChoice.Overwrite,
                ContentDialogResult.Secondary => FileConflictChoice.Skip,
                _ => FileConflictChoice.Cancel
            };
        }

        /// <summary>
        /// Aplica a escolha do usuário sobre conflito de arquivos.
        /// </summary>
        /// <param name="choice">Escolha do usuário</param>
        private void ApplyFileConflictChoice(FileConflictChoice choice)
        {
            _fileConflictChoice = choice;
            
            switch (choice)
            {
                case FileConflictChoice.Overwrite:
                    if (ExcludeOlderCheckBox != null)
                    {
                        ExcludeOlderCheckBox.IsChecked = false;
                    }
                    _uiService?.ShowInfoBar("Modo: Sobrescrever - Arquivos existentes serão substituídos.", InfoBarSeverity.Informational);
                    break;

                case FileConflictChoice.Skip:
                    _uiService?.ShowInfoBar("Modo: Ignorar - Arquivos existentes serão mantidos, apenas novos serão copiados.", InfoBarSeverity.Informational);
                    break;
            }
        }

        #endregion

        #region Path Management

        /// <summary>
        /// Define o caminho de origem e atualiza a UI.
        /// </summary>
        /// <param name="path">Caminho da pasta de origem</param>
        private void SetSourcePath(string path)
        {
            SourcePathTextBox.Text = path;

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

        /// <summary>
        /// Define o caminho de destino e atualiza a UI.
        /// </summary>
        /// <param name="path">Caminho da pasta de destino</param>
        private void SetDestinationPath(string path)
        {
            DestinationPathTextBox.Text = path;

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

        #endregion

        #region Drag and Drop Handlers

        /// <summary>
        /// Manipula o evento DragOver da área de origem.
        /// </summary>
        private void SourceDropBorder_DragOver(object sender, DragEventArgs e)
        {
            _sourceDragDropHandler?.HandleDragOver(sender, e, "Soltar para definir origem");
        }

        /// <summary>
        /// Manipula o evento Drop da área de origem.
        /// </summary>
        private async void SourceDropBorder_Drop(object sender, DragEventArgs e)
        {
            if (_sourceDragDropHandler != null)
            {
                await _sourceDragDropHandler.HandleDropAsync(sender, e);
            }
        }

        /// <summary>
        /// Manipula o evento DragLeave da área de origem.
        /// </summary>
        private void SourceDropBorder_DragLeave(object sender, DragEventArgs e)
        {
            _sourceDragDropHandler?.HandleDragLeave(sender, e);
        }

        /// <summary>
        /// Manipula o clique na área de origem para abrir o seletor de pastas.
        /// </summary>
        private async void SourceDropBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var folder = await PickFolderAsync();
            if (folder != null)
            {
                SetSourcePath(folder.Path);
            }
        }

        /// <summary>
        /// Manipula o evento DragOver da área de destino.
        /// </summary>
        private void DestinationDropBorder_DragOver(object sender, DragEventArgs e)
        {
            _destinationDragDropHandler?.HandleDragOver(sender, e, "Soltar para definir destino");
        }

        /// <summary>
        /// Manipula o evento Drop da área de destino.
        /// </summary>
        private async void DestinationDropBorder_Drop(object sender, DragEventArgs e)
        {
            if (_destinationDragDropHandler != null)
            {
                await _destinationDragDropHandler.HandleDropAsync(sender, e);
            }
        }

        /// <summary>
        /// Manipula o evento DragLeave da área de destino.
        /// </summary>
        private void DestinationDropBorder_DragLeave(object sender, DragEventArgs e)
        {
            _destinationDragDropHandler?.HandleDragLeave(sender, e);
        }

        /// <summary>
        /// Manipula o clique na área de destino para abrir o seletor de pastas.
        /// </summary>
        private async void DestinationDropBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var folder = await PickFolderAsync();
            if (folder != null)
            {
                SetDestinationPath(folder.Path);
            }
        }

        /// <summary>
        /// Abre o seletor de pastas do sistema.
        /// </summary>
        /// <returns>Pasta selecionada ou null se cancelado</returns>
        private async Task<StorageFolder?> PickFolderAsync()
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            return await folderPicker.PickSingleFolderAsync();
        }

        #endregion

        #region Execution Handlers

        /// <summary>
        /// Manipula o clique no botão de executar.
        /// Valida entradas, constrói o comando e executa o Robocopy.
        /// </summary>
        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _fileConflictChoice = null;

                if (!await ValidateInputsAsync())
                {
                    return;
                }

                var command = BuildRobocopyCommand();
                CommandPreviewTextBox.Text = command;

                if (_confirmExecutionCheckBox?.IsChecked == true)
                {
                    var result = await ShowQuestionDialogAsync(
                        "Confirmar Execução",
                        $"Deseja executar o seguinte comando?\n\n{command}",
                        "Executar",
                        "Cancelar");

                    if (result != ContentDialogResult.Primary)
                    {
                        _fileConflictChoice = null;
                        return;
                    }
                }

                ExecuteButton.IsEnabled = false;
                CancelButton.IsEnabled = true;

                _progressTracker?.StartOperation();
                _uiService?.SetOutput("Iniciando Robocopy...\n\n");

                _cancellationTokenSource = new CancellationTokenSource();
                await ExecuteRobocopyAsync(command, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _uiService?.AppendOutput("\n\n=== OPERAÇÃO CANCELADA ===\n");
                _uiService?.ShowInfoBar("Operação cancelada pelo usuário.", InfoBarSeverity.Warning);
            }
            catch (Exception ex)
            {
                _uiService?.AppendOutput($"\n\n=== ERRO ===\n{ex.Message}\n");
                await ShowErrorDialogAsync("Erro ao executar Robocopy", ex.Message);
            }
            finally
            {
                ExecuteButton.IsEnabled = true;
                CancelButton.IsEnabled = false;

                _progressTracker?.StopOperation();

                _currentProcess = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                _fileConflictChoice = null;
            }
        }

        /// <summary>
        /// Manipula o clique no botão de cancelar.
        /// Cancela a operação em andamento.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _cancellationTokenSource?.Cancel();

                if (_currentProcess != null && !_currentProcess.HasExited)
                {
                    _currentProcess.Kill(true);
                    _uiService?.AppendOutput("\n\nCancelando operação...\n");
                }
            }
            catch (Exception ex)
            {
                _ = ShowErrorDialogAsync("Erro ao cancelar", ex.Message);
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Valida entradas do usuário antes da execução do Robocopy.
        /// </summary>
        /// <returns>Verdadeiro se as entradas são válidas</returns>
        private async Task<bool> ValidateInputsAsync()
        {
            // Implementar validação das entradas do usuário
            return true;
        }

        #endregion
    }
}

