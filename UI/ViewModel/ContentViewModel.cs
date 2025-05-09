using Common;
using Common.Config;
using Common.Message;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UI.ViewModel
{
    public partial class ContentViewModel : ObservableObject
    {
        public Log LogInstance { get; }

        public ContentViewModel(Log log)
        {
            LogInstance = log;
        }

    }
}
