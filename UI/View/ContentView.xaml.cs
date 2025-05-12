using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Messaging;
using UI.Model;

namespace UI.View
{
    public partial class ContentView : UserControl
    {
        public ContentView()
        {
            InitializeComponent();
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = (Grid)sender;
            var item = (PathItem)grid.DataContext;
            WeakReferenceMessenger.Default.Send(new CellMouseRightClickMessage(item));
        }
    }
}
