using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace LMT_Flickr_Downloader
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : MetroWindow
    {
        public About()
        {
            InitializeComponent();
        }

        
        private void HyperlinktacGia_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void HyperlinkHuongDan_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        
        private async void Hyperlink_ClickAsync(object sender, RoutedEventArgs e)
        {
            string pathSettingFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\LMT Flickr Downloader" + "\\api.dat";
            try
            {
                if (File.Exists(pathSettingFile))
                    File.Delete(pathSettingFile);
                var result = await this.ShowMessageAsync("Thông báo", "Đã xóa API, công cụ sẽ đóng!",
                    MessageDialogStyle.Affirmative);
                bool delete = result == MessageDialogResult.Affirmative;
                if (delete == true)
                    Application.Current.Shutdown();
            }
            catch (Exception)
            {
                await this.ShowMessageAsync("Lỗi!", "Đã có lỗi khi xóa!");
            }
        }


    }
}
