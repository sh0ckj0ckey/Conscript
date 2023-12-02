using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;
using Conscript.Core;
using Conscript.Helpers;
using Conscript.Models;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using static System.Net.Mime.MediaTypeNames;
using static Conscript.Helpers.InstalledFont;

namespace Conscript.ViewModels
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

        public ObservableCollection<MainNavigationBase> MainNavigationItems { get; private set; } = new ObservableCollection<MainNavigationBase>();

        public ObservableCollection<MainNavigationBase> MainNavigationFooterItems { get; private set; } = new ObservableCollection<MainNavigationBase>();

        /// <summary>
        /// 全部脚本列表
        /// </summary>
        public ObservableCollection<ShortcutModel> AllShortcuts { get; private set; } = new ObservableCollection<ShortcutModel>();

        public ObservableCollection<ShortcutModel> Ps1Shortcuts { get; private set; } = new ObservableCollection<ShortcutModel>();
        public ObservableCollection<ShortcutModel> BatShortcuts { get; private set; } = new ObservableCollection<ShortcutModel>();

        public ObservableCollection<Character> AllIcons { get; set; } = new ObservableCollection<Character>();

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

            MainNavigationItems.Add(new MainNavigationHeader("Scripts"));
            MainNavigationItems.Add(new MainNavigationItem("所有脚本", "all", "\uE74C"));
            MainNavigationItems.Add(new MainNavigationItem("添加", "add", "\uE109"));
            MainNavigationItems.Add(new MainNavigationHeader("分类"));
            MainNavigationItems.Add(new MainNavigationItem("PowerShell", "ps", "\uE62F"));
            MainNavigationItems.Add(new MainNavigationItem("批处理", "bat", "\uE756"));

            MainNavigationFooterItems.Add(new MainNavigationSeparator());
            MainNavigationFooterItems.Add(new MainNavigationSettingItem());

            LoadShortcuts();
        }

        /// <summary>
        /// 选择查看指定的脚本快捷项
        /// </summary>
        /// <param name="shortcut"></param>
        public void SelectShortcut(ShortcutModel shortcut)
        {
            CurrentShortcut = shortcut;
            ShortcutFileExists = (!string.IsNullOrWhiteSpace(shortcut.ScriptFilePath) && File.Exists(shortcut.ScriptFilePath));
            ShortcutRunning = false;
            ShortcutOutput = string.Empty;
            ShortcutError = string.Empty;
            ShortcutExitCode = string.Empty;
        }

        /// <summary>
        /// 启动指定的脚本快捷项
        /// </summary>
        /// <param name="shortcut"></param>
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
                        processInfo.UseShellExecute = shortcut.ShortcutRunas;
                        processInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(shortcut.ScriptFilePath);
                        processInfo.RedirectStandardError = !shortcut.ShortcutRunas;
                        processInfo.RedirectStandardOutput = !shortcut.ShortcutRunas;

                        if (shortcut.ShortcutType == ShortcutTypeEnum.Ps1)
                        {
                            processInfo.FileName = "powershell.exe";
                            processInfo.Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{shortcut.ScriptFilePath}\"";
                        }
                        else if (shortcut.ShortcutType == ShortcutTypeEnum.Bat)
                        {
                            processInfo.FileName = shortcut.ScriptFilePath;
                        }

                        if (shortcut.ShortcutRunas)
                        {
                            processInfo.Verb = "runas";
                        }

                        var process = Process.Start(processInfo);
                        process.WaitForExit();

                        if (!shortcut.ShortcutRunas)
                        {
                            string output = process.StandardOutput.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();
                            string exitCode = process.ExitCode.ToString();

                            DispatcherQueue.TryEnqueue(() =>
                            {
                                ShortcutOutput = output;
                                ShortcutError = error;
                                ShortcutExitCode = exitCode;
                            });
                        }

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

        /// <summary>
        /// 从所有脚本快捷项中筛选出 PowerShell 类型
        /// </summary>
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

        /// <summary>
        /// 从所有脚本快捷项中筛选出批处理类型
        /// </summary>
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

        /// <summary>
        /// 加载本机 Segoe Fluent Icons 字体内的所有图标
        /// </summary>
        public void LoadSegoeFluentIcons()
        {
            if (AllIcons.Count <= 0)
            {
                var icons = FontHelper.GetAllSegoeFluentIcons();
                foreach (var icon in icons)
                {
                    AllIcons.Add(icon);
                }
            }
        }

        /// <summary>
        /// 加载保存的脚本快捷项
        /// </summary>
        private async void LoadShortcuts()
        {
            try
            {
                try
                {
                    string shortcutsJson = await StorageFilesService.ReadFileAsync("shortcuts.json");
                    if (!string.IsNullOrWhiteSpace(shortcutsJson))
                    {
                        var shortcuts = JsonSerializer.Deserialize<ObservableCollection<ShortcutModel>>(shortcutsJson);

                        foreach (var shortcut in shortcuts)
                        {
                            AllShortcuts.Add(shortcut);
                        }
                    }
                }
                catch (Exception e) { Debug.WriteLine(e.Message); }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 添加新的脚本快捷项并保存
        /// </summary>
        /// <param name="color"></param>
        /// <param name="iconIndex"></param>
        /// <param name="name"></param>
        /// <param name="ext"></param>
        public async void AddShortcut(int color, int iconIndex, string name, string ext, string path, bool runas)
        {
            try
            {
                this.AllShortcuts.Insert(0, new ShortcutModel()
                {
                    ShortcutColor = (ShortcutColorEnum)(Math.Max(1, Math.Min(color, 9))),
                    ShortcutIcon = this.AllIcons[iconIndex].Char,
                    ShortcutName = name,
                    ShortcutType = ext == ".ps1" ? ShortcutTypeEnum.Ps1 : ShortcutTypeEnum.Bat,
                    ScriptFilePath = path,
                    ShortcutRunas = runas
                });

                string shortcutsSaveJson = JsonSerializer.Serialize(AllShortcuts);
                _ = await StorageFilesService.WriteFileAsync("shortcuts.json", shortcutsSaveJson);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 移除指定脚本快捷项并保存
        /// </summary>
        /// <param name="deletingShortcut"></param>
        public async void DelShortcut(ShortcutModel deletingShortcut)
        {
            try
            {
                string deleteFilePath = deletingShortcut.ScriptFilePath;

                this.AllShortcuts.Remove(deletingShortcut);

                if (File.Exists(deleteFilePath))
                {
                    File.Delete(deleteFilePath);
                }

                string shortcutsSaveJson = JsonSerializer.Serialize(AllShortcuts);
                _ = await StorageFilesService.WriteFileAsync("shortcuts.json", shortcutsSaveJson);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
