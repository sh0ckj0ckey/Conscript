using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PowerShortcut.Core;
using PowerShortcut.Models;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace PowerShortcut.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private static Lazy<MainViewModel> _lazyVM = new Lazy<MainViewModel>(() => new MainViewModel());
        public static MainViewModel Instance => _lazyVM.Value;

        public Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue = null;

        public SettingsService AppSettings { get; set; } = new SettingsService();

        /// <summary>
        /// 控制主窗口根据当前的主题进行切换
        /// </summary>
        public Action ActSwitchAppTheme { get; set; } = null;

        /// <summary>
        /// 控制主窗口根据当前的设置更改背景材质
        /// </summary>
        public Action ActChangeBackdrop { get; set; } = null;


        public ObservableCollection<MainNavigationBase> MainNavigationItems = new ObservableCollection<MainNavigationBase>();

        public ObservableCollection<MainNavigationBase> MainNavigationFooterItems = new ObservableCollection<MainNavigationBase>();

        /// <summary>
        /// 全部脚本列表
        /// </summary>
        public ObservableCollection<ShortcutModel> AllShortcuts = new ObservableCollection<ShortcutModel>();

        public ObservableCollection<ShortcutModel> Ps1Shortcuts = new ObservableCollection<ShortcutModel>();
        public ObservableCollection<ShortcutModel> BatShortcuts = new ObservableCollection<ShortcutModel>();

        #region 选择脚本

        /// <summary>
        /// 当前选中的脚本
        /// </summary>
        private ShortcutModel _currentShortcut = null;
        public ShortcutModel CurrentShortcut
        {
            get => _currentShortcut;
            set => SetProperty(ref _currentShortcut, value);
        }

        /// <summary>
        /// 当前选择的脚本是否正在运行
        /// </summary>
        private bool _shortcutRunning = false;
        public bool ShortcutRunning
        {
            get => _shortcutRunning;
            set => SetProperty(ref _shortcutRunning, value);
        }

        /// <summary>
        /// 当前选择的脚本是否存在文件
        /// </summary>
        private bool _shortcutFileExists = true;
        public bool ShortcutFileExists
        {
            get => _shortcutFileExists;
            set => SetProperty(ref _shortcutFileExists, value);
        }

        /// <summary>
        /// 当前执行的脚本的输出内容
        /// </summary>
        private string _shortcutOutput = string.Empty;
        public string ShortcutOutput
        {
            get => _shortcutOutput;
            set => SetProperty(ref _shortcutOutput, value);
        }

        /// <summary>
        /// 当前执行的脚本的错误内容
        /// </summary>
        private string _shortcutError = string.Empty;
        public string ShortcutError
        {
            get => _shortcutError;
            set => SetProperty(ref _shortcutError, value);
        }

        /// <summary>
        /// 当前执行的脚本的返回值
        /// </summary>
        private string _shortcutExitCode = string.Empty;
        public string ShortcutExitCode
        {
            get => _shortcutExitCode;
            set => SetProperty(ref _shortcutExitCode, value);
        }

        #endregion

        public MainViewModel()
        {
            AppSettings.OnAppearanceSettingChanged += (index) => { ActSwitchAppTheme?.Invoke(); };
            AppSettings.OnBackdropSettingChanged += (index) => { ActChangeBackdrop?.Invoke(); };

            MainNavigationItems.Add(new MainNavigationHeader("Power Shortcuts"));
            MainNavigationItems.Add(new MainNavigationItem("所有脚本", "all", "\uE74C"));
            MainNavigationItems.Add(new MainNavigationItem("添加", "add", "\uE109"));
            MainNavigationItems.Add(new MainNavigationHeader("分类"));
            MainNavigationItems.Add(new MainNavigationItem("PowerShell", "ps", "\uE62F"));
            MainNavigationItems.Add(new MainNavigationItem("批处理", "bat", "\uE756"));

            MainNavigationFooterItems.Add(new MainNavigationSeparator());
            MainNavigationFooterItems.Add(new MainNavigationSettingItem());

            LoadTest();
        }

        private void LoadTest()
        {
            AllShortcuts.Add(new ShortcutModel()
            {
                ShortcutColor = ShortcutColorEnum.Blue,
                ShortcutIcon = "\uE678",
                ShortcutName = "禁用摄像头",
                ShortcutType = ShortcutTypeEnum.Ps1
            });

            AllShortcuts.Add(new ShortcutModel()
            {
                ShortcutColor = ShortcutColorEnum.Gray,
                ShortcutIcon = "\uE114",
                ShortcutName = "开启摄像头",
                ShortcutType = ShortcutTypeEnum.Ps1
            });

            AllShortcuts.Add(new ShortcutModel()
            {
                ShortcutColor = ShortcutColorEnum.Purple,
                ShortcutIcon = "\uE12B",
                ShortcutName = "断网",
                ShortcutType = ShortcutTypeEnum.Bat,
                ScriptFilePath = @"C:\Users\Shock Jockey\Documents\NoMewing\PowerShortcut\a.bat"
            });

            AllShortcuts.Add(new ShortcutModel()
            {
                ShortcutColor = ShortcutColorEnum.Yellow,
                ShortcutIcon = "\uE1D5",
                ShortcutName = "开始录音开始录音开始录音开始录音开始录音",
                ShortcutType = ShortcutTypeEnum.Ps1,
                ScriptFilePath = @"C:\Users\Shock Jockey\Documents\NoMewing\PowerShortcut\b.ps1"
            });
        }

        public void SelectShortcut(ShortcutModel shortcut)
        {
            CurrentShortcut = shortcut;
            ShortcutFileExists = (!string.IsNullOrWhiteSpace(shortcut.ScriptFilePath) && File.Exists(shortcut.ScriptFilePath));
            ShortcutRunning = false;
            ShortcutOutput = string.Empty;
            ShortcutError = string.Empty;
            ShortcutExitCode = string.Empty;
        }

        public async void LaunchShortcut(ShortcutModel shortcut)
        {
            if (shortcut != null)
            {
                ShortcutRunning = true;
                ShortcutOutput = string.Empty;
                ShortcutError = string.Empty;
                ShortcutExitCode = string.Empty;

                await Task.Run(() =>
                {
                    try
                    {
                        var processInfo = new ProcessStartInfo();
                        processInfo.CreateNoWindow = true;
                        processInfo.UseShellExecute = false;
                        processInfo.WorkingDirectory = Path.GetDirectoryName(shortcut.ScriptFilePath);
                        processInfo.RedirectStandardError = true;
                        processInfo.RedirectStandardOutput = true;

                        if (shortcut.ShortcutType == ShortcutTypeEnum.Ps1)
                        {
                            processInfo.FileName = "powershell.exe";
                            processInfo.Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{shortcut.ScriptFilePath}\"";
                        }
                        else if (shortcut.ShortcutType == ShortcutTypeEnum.Bat)
                        {
                            processInfo.FileName = shortcut.ScriptFilePath;
                        }

                        var process = Process.Start(processInfo);
                        process.WaitForExit();

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        string exitCode = process.ExitCode.ToString();

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            ShortcutOutput = output;
                            ShortcutError = error;
                            ShortcutExitCode = exitCode;
                        });

                        process.Close();
                    }
                    catch { }
                    finally
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            ShortcutRunning = false;
                        });
                    }
                });
            }
        }

        public void UpdatePs1Shortcuts()
        {
            Ps1Shortcuts.Clear();

            foreach (var item in AllShortcuts)
            {
                if (item.ShortcutType == ShortcutTypeEnum.Ps1)
                {
                    Ps1Shortcuts.Add(item);
                }
            }
        }

        public void UpdateBatShortcuts()
        {
            BatShortcuts.Clear();

            foreach (var item in AllShortcuts)
            {
                if (item.ShortcutType == ShortcutTypeEnum.Bat)
                {
                    BatShortcuts.Add(item);
                }
            }
        }
    }
}
