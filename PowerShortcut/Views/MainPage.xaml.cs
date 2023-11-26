using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PowerShortcut.Models;
using PowerShortcut.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.ApplicationSettings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerShortcut.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainViewModel MainViewModel = null;

        // 导航栏项的Tag对应的Page
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("all", typeof(ShortcutsPage)),
            ("ps", typeof(PowershellsPage)),
            ("bat", typeof(BatsPage)),
            ("add", typeof(AddingPage)),
            ("settings", typeof(SettingsPage)),
        };

        public MainPage()
        {
            this.InitializeComponent();

            MainViewModel = MainViewModel.Instance;

            MainViewModel.Instance.ActSwitchAppTheme?.Invoke();
        }

        private void MainNavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 页面发生导航时，更新侧边栏的选中项
                MainFrame.Navigated += MainFrame_Navigated;

                MainFramNavigateToPage("all", null, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
            }
            catch { }
        }

        private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            try
            {
                if (args?.InvokedItemContainer?.Tag == null || string.IsNullOrWhiteSpace(args?.InvokedItemContainer?.Tag?.ToString())) return;

                if (args.InvokedItemContainer != null)
                {
                    var navItemTag = args.InvokedItemContainer.Tag.ToString();
                    MainFramNavigateToPage(navItemTag, args.RecommendedNavigationTransitionInfo);
                }

                // 清除返回
                MainFrame.BackStack.Clear();
                MainFrame.ForwardStack.Clear();
            }
            catch { }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
                if (MainFrame.SourcePageType != null)
                {
                    string tag = (_pages.FirstOrDefault(p => p.Page == e.SourcePageType)).Tag;

                    // 遍历侧栏找到匹配的选项，将侧栏的选中项对应到当前页面
                    MainNavigationBase select = null;
                    if (select is null)
                    {
                        foreach (var menuItem in MainViewModel.Instance.MainNavigationItems)
                        {
                            if (menuItem is MainNavigationItem menu && menu?.Tag?.Equals(tag) == true)
                            {
                                select = menuItem;
                                break;
                            }
                        }
                    }
                    if (select is null)
                    {
                        foreach (var footerMenuItem in MainViewModel.Instance.MainNavigationFooterItems)
                        {
                            if (tag == "settings" && footerMenuItem is MainNavigationSettingItem menu)
                            {
                                select = footerMenuItem;
                                break;
                            }
                            else if (footerMenuItem is MainNavigationItem footer && footer?.Tag?.Equals(tag) == true)
                            {
                                select = footerMenuItem;
                                break;
                            }
                        }
                    }
                    //if (select is null)
                    //{
                    //    foreach (var menuItem in MainViewModel.Instance.MainNavigationRecentClassesItems)
                    //    {
                    //        if (menuItem is MainNavigationItem menu && menu?.sTag?.Equals(tag) == true)
                    //        {
                    //            select = menuItem;
                    //            break;
                    //        }
                    //    }
                    //}
                    MainNavigationView.SelectedItem = select;

                    if (tag == "settings")
                    {
                        ColorLogoImage.Opacity = 0;
                    }
                    else
                    {
                        ColorLogoImage.Opacity = 1;
                    }
                }
            }
            catch { }
        }

        private void MainFramNavigateToPage(string navItemTag, object parameter = null, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo = null)
        {
            try
            {
                Type page = null;

                var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                page = item.Page;

                var preNavPageType = MainFrame.CurrentSourcePageType;
                if (page is not null && !Type.Equals(preNavPageType, page))
                {
                    if (parameter != null || transitionInfo != null)
                    {
                        MainFrame.Navigate(page, parameter, transitionInfo);
                    }
                    else
                    {
                        MainFrame.Navigate(page);
                    }
                }
            }
            catch { }
        }
    }
}
