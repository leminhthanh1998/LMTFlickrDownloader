using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace LMT_Flickr_Downloader
{
    /// <summary>
    /// Interaction logic for GetAPI.xaml
    /// </summary>
    public partial class NotNetwork : MetroWindow
    {
        public NotNetwork()
        {
            InitializeComponent();
        }
        
        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NotNetwork_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
