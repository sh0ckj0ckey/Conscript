﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PowerShortcut.Core;
using PowerShortcut.Models;

namespace PowerShortcut.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private static Lazy<MainViewModel> _lazyVM = new Lazy<MainViewModel>(() => new MainViewModel());
        public static MainViewModel Instance => _lazyVM.Value;

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

        public MainViewModel()
        {
            AppSettings.OnAppearanceSettingChanged += (index) => { ActSwitchAppTheme?.Invoke(); };
            AppSettings.OnBackdropSettingChanged += (index) => { ActChangeBackdrop?.Invoke(); };

            MainNavigationItems.Add(new MainNavigationHeader("Power Shortcut"));
            MainNavigationItems.Add(new MainNavigationItem("所有脚本", "all", "\uE74C"));
            MainNavigationItems.Add(new MainNavigationItem("添加", "add", "\uE109"));
            MainNavigationItems.Add(new MainNavigationHeader("分类"));
            MainNavigationItems.Add(new MainNavigationItem("PowerShell", "ps", "\uE62F"));
            MainNavigationItems.Add(new MainNavigationItem("批处理", "bat", "\uE756"));

            MainNavigationFooterItems.Add(new MainNavigationSeparator());
            MainNavigationFooterItems.Add(new MainNavigationSettingItem());

            LoadTest();
        }

        private async void LoadTest()
        {
            await Task.Delay(2000);

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
                ShortcutType = ShortcutTypeEnum.Bat
            });

            AllShortcuts.Add(new ShortcutModel()
            {
                ShortcutColor = ShortcutColorEnum.Yellow,
                ShortcutIcon = "\uE1D5",
                ShortcutName = "开始录音开始录音开始录音开始录音开始录音",
                ShortcutType = ShortcutTypeEnum.Ps1
            });
        }
    }
}
