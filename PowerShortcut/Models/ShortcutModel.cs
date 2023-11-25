using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;

namespace PowerShortcut.Models
{
    public class ShortcutModel : ObservableObject
    {
        /// <summary>
        /// 脚本文件路径
        /// </summary>
        public string ScriptFilePath { get; set; } = string.Empty;

        /// <summary>
        /// 脚本名称
        /// </summary>
        private string _shortcutName = string.Empty;
        public string ShortcutName
        {
            get => _shortcutName;
            set => SetProperty(ref _shortcutName, value);
        }

        /// <summary>
        /// 脚本图标
        /// </summary>
        private string _shortcutIcon = string.Empty;
        public string ShortcutIcon
        {
            get => _shortcutIcon;
            set => SetProperty(ref _shortcutIcon, value);
        }

        /// <summary>
        /// 脚本类型
        /// </summary>
        private ShortcutTypeEnum _shortcutType = ShortcutTypeEnum.Ps1;
        public ShortcutTypeEnum ShortcutType
        {
            get => _shortcutType;
            set => SetProperty(ref _shortcutType, value);
        }

        /// <summary>
        /// 脚本颜色
        /// </summary>
        private ShortcutColorEnum _shortcutColor = ShortcutColorEnum.Transparent;
        public ShortcutColorEnum ShortcutColor
        {
            get => _shortcutColor;
            set => SetProperty(ref _shortcutColor, value);
        }
    }
}
