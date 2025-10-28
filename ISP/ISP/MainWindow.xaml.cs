using ISP.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ISP
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        internal ISPViewModel viewModel;
        View.SetComPage setComPage = new View.SetComPage();
        View.ProgrammingPage programmingPage  = new View.ProgrammingPage();
        View.BinWindow binWindow = new View.BinWindow();


        public MainWindow()
        {
            InitializeComponent();
            viewModel = new ISPViewModel();
            this.DataContext = viewModel;
                             
            if (Directory.Exists(viewModel.AppPath + @"\util") == false) Directory.CreateDirectory(viewModel.AppPath + @"\util");
            if (Directory.Exists(viewModel.AppPath + @"\data") == false) Directory.CreateDirectory(viewModel.AppPath + @"\data");

            GridPrincipal.Children.Clear();
            GridPrincipal.Children.Add(setComPage);
            GridPrincipal.HorizontalAlignment = HorizontalAlignment.Stretch;
            GridPrincipal.VerticalAlignment = VerticalAlignment.Stretch;

            this.Loaded += new RoutedEventHandler(Window_Loaded);
        }

        private void bt_setdevice_Click(object sender, RoutedEventArgs e)
        {
            GridPrincipal.Children.Clear();
            GridPrincipal.Children.Add(setComPage);
            GridPrincipal.HorizontalAlignment = HorizontalAlignment.Stretch;
            GridPrincipal.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void bt_params_Click(object sender, RoutedEventArgs e)
        {
            GridPrincipal.Children.Clear();
            GridPrincipal.Children.Add(programmingPage);
            GridPrincipal.HorizontalAlignment = HorizontalAlignment.Stretch;
            GridPrincipal.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void bt_binfile_Click(object sender, RoutedEventArgs e)
        {
            GridPrincipal.Children.Clear();
            GridPrincipal.Children.Add(binWindow);
            GridPrincipal.HorizontalAlignment = HorizontalAlignment.Stretch;
            GridPrincipal.VerticalAlignment = VerticalAlignment.Stretch;

        }

        private void bt_program_Click(object sender, RoutedEventArgs e)
        {
            GridPrincipal.Children.Clear();
            GridPrincipal.Children.Add(binWindow);
            GridPrincipal.HorizontalAlignment = HorizontalAlignment.Stretch;
            GridPrincipal.VerticalAlignment = VerticalAlignment.Stretch;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(((App.Current) as App).PATHMAP);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
