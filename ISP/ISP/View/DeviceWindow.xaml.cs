using ISP.ViewModel;
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
using System.Windows.Shapes;
using ISP.ViewModel;
namespace ISP.View
{
    /// <summary>
    /// DeviceWindow.xaml 的互動邏輯
    /// </summary>
    /// 

    public partial class DeviceWindow : Window
    {
       
        public DeviceWindow(ISPViewModel pviewModel)
        {
            InitializeComponent();
            this.DataContext = pviewModel;
        }
        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
      
            this.Close();
        }
    }
}
