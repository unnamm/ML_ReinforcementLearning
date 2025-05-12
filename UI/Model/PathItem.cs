using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace UI.Model
{
    public partial class PathItem : ObservableObject
    {
        [ObservableProperty] private Visibility _upArrow = Visibility.Hidden;
        [ObservableProperty] private Visibility _downArrow = Visibility.Hidden;
        [ObservableProperty] private Visibility _leftArrow = Visibility.Hidden;
        [ObservableProperty] private Visibility _rightArrow = Visibility.Hidden;

        [ObservableProperty] private int _upWeight;
        [ObservableProperty] private int _downWeight;
        [ObservableProperty] private int _leftWeight;
        [ObservableProperty] private int _rightWeight;

        [ObservableProperty] private Visibility _position = Visibility.Hidden;
        [ObservableProperty] private PackIconKind _kind = PackIconKind.Accessibility;
        [ObservableProperty] private SolidColorBrush _background = Brushes.Transparent;

        public System.Drawing.Point Coor;
    }
}
