using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace LMT_Flickr_Downloader
{
    /// <summary>
    /// Interaction logic for GetAPI.xaml
    /// </summary>
    public partial class GetAPI : MetroWindow
    {
        public GetAPI()
        {
            InitializeComponent();
        }
        private string pathSettingFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\LMT Flickr Downloader" + "\\api.dat";
        GetImages images = new GetImages();
        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            Check();
        }

        private void Check()
        {
            if (txbAPI.Text != "")
            {
                string check = images.CheckAPI(txbAPI.Text);
                if (check == "invalid")
                    this.ShowMessageAsync("Lỗi", "API của bạn không đúng, vui lòng kiểm tra và nhập API lại!");
                if (check == "exprired")
                    this.ShowMessageAsync("Lỗi", "API của bạn đã hết hạn, vui lòng kiểm tra và nhập API lại!");
                else if (check == "OK")
                {
                    File.WriteAllText(pathSettingFile, txbAPI.Text);
                    MainWindow.apiFlickr = txbAPI.Text;
                    Hide();
                }
            }
            else
            {
                this.ShowMessageAsync("Thông báo!", "Bạn chưa nhập API!");
            }
        }

        private void GetAPI_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TxbAPI_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Check();
            }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
