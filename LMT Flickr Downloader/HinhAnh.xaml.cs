using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Path = System.IO.Path;
using WPFFolderBrowser;
using System.Diagnostics;

namespace LMT_Flickr_Downloader
{
    /// <summary>
    /// Interaction logic for CaNhan.xaml
    /// </summary>
    public partial class HinhAnh : MetroWindow
    {
        public HinhAnh(string _link)
        {
            InitializeComponent();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            wc.DownloadProgressChanged += Wc_DownloadProgressChanged1;
            wc.DownloadFileCompleted += Wc_DownloadFileCompletedAsync;
            link = _link;
        }

        
        
        #region Var
        private List<string> dsLink = new List<string>();
        private string fileName;
        private string link;
        private string pathFolder;
        private bool clicked = false;
        private bool workerRun = true;
        BackgroundWorker worker = new BackgroundWorker();
        GetImages images = new GetImages();
        WebClient wc = new WebClient();
        #endregion

        private void CaNhan_OnLoaded(object sender, RoutedEventArgs e)
        {
            ProgressRing.Visibility = Visibility.Visible;
            btnDownload.IsEnabled = SliderChatLuong.IsEnabled= false;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Thay doi lable chat luong khi keo thanh slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderChatLuong_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                switch (SliderChatLuong.Value)
                {
                    case 0:
                        LabelChatLuong.Content = "Trung bình 500";
                        break;
                    case 1:
                        LabelChatLuong.Content = "Trung bình 640";
                        break;
                    case 2:
                        LabelChatLuong.Content = "Trung bình 800";
                        break;
                    case 3:
                        LabelChatLuong.Content = "Lớn 1024";
                        break;
                    case 4:
                        LabelChatLuong.Content = "Lớn 1600";
                        break;
                    case 5:
                        LabelChatLuong.Content = "Lớn 2048";
                        break;
                    case 6:
                        LabelChatLuong.Content = "Ảnh gốc";
                        break;

                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Chon thu muc de luu hinh anh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectFolder_OnClick(object sender, RoutedEventArgs e)
        {
            WPFFolderBrowserDialog dlg = new WPFFolderBrowserDialog();
            dlg.Title = "Chọn thư mục để lưu ảnh!";
            if (dlg.ShowDialog() == true)
            {
                pathFolder = dlg.FileName;
                txbPath.Text = pathFolder;
            }
        }

        #region Worker
        //Worker lay anh
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string id = images.GetIDdPhoto(link);
            dsLink= images.GetImage(id);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressRing.Visibility = Visibility.Hidden;
            btnDownload.IsEnabled = SliderChatLuong.IsEnabled=btnSelectFolder.IsEnabled = true;
            SliderChatLuong.Maximum = dsLink.Count-1;
            if(dsLink.Count==0)
                this.ShowMessageAsync("Lỗi!", "Không tìm thấy ảnh để tải!");
        }
        //Worker tai anh
        

        #endregion


        private void BtnDownload_OnClick(object sender, RoutedEventArgs e)
        {
            if (pathFolder != null)
            {
                if (dsLink.Count > 0)
                {
                    if (!clicked)
                    {
                        btnDownload.Content = "Hủy";
                        clicked = true;
                        btnSelectFolder.IsEnabled = SliderChatLuong.IsEnabled = false;
                        string link = dsLink[(int) SliderChatLuong.Value];
                        fileName = pathFolder + "\\" + Path.GetFileName(new Uri(link).LocalPath);
                        wc.DownloadFileAsync(new Uri(link),fileName);
                    }
                    else
                    {
                        wc.CancelAsync();
                        workerRun = false;
                        btnDownload.Content = "Tải";
                        clicked = false;
                    }
                }
                else this.ShowMessageAsync("Lỗi!", "Không tìm thấy ảnh để tải!");
            }
            else this.ShowMessageAsync("Lỗi!", "Bạn chưa chọn thư mục lưu!");
        }

        private void Wc_DownloadFileCompletedAsync(object sender, AsyncCompletedEventArgs e)
        {
            btnSelectFolder.IsEnabled = SliderChatLuong.IsEnabled = true;
            clicked = false;
            btnDownload.Content = "Tải";
            if (workerRun)
            {
               this.ShowMessageAsync("Thông báo!", "Đã tải về hoàn tất!");
                
                workerRun = true;
                ProgressBar.Value = 0;
                LabelProgress.Content = "0%";
                //Neu chon mo thi tien hanh mo thu muc va chon san file
                if(CkbOpenfolder.IsChecked==true)
                {
                    string args = string.Format("/e, /select, \"{0}\"", fileName);
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.FileName = "explorer";
                    info.Arguments = args;
                    info.WindowStyle = ProcessWindowStyle.Normal;
                    Process.Start(info);
                }
            }
            else
            {
                this.ShowMessageAsync("Thông báo!", "Đã hủy tải về!");
                workerRun = true;
                ProgressBar.Value = 0;
                LabelProgress.Content = "0%";
            }
            if (btnSetBackground.IsChecked == true)
            {
                SetWallpaper(fileName);
            }
        }

        private void Wc_DownloadProgressChanged1(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
            LabelProgress.Content = e.ProgressPercentage+"%";
            
        }

        #region Set hinh nen
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(
            UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        private static readonly UInt32 SPI_SETDESKWALLPAPER = 0x14;
        private static readonly UInt32 SPIF_UPDATEINIFILE = 0x01;
        private static readonly UInt32 SPIF_SENDWININICHANGE = 0x02;

        public void SetWallpaper(String path)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        #endregion
    }
}
