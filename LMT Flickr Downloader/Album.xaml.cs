using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using WPFFolderBrowser;
using Path = System.IO.Path;

namespace LMT_Flickr_Downloader
{
    /// <summary>
    /// Interaction logic for CaNhan.xaml
    /// </summary>
    public partial class Album : MetroWindow
    {
        public Album(string _id, string _link)
        {
            InitializeComponent();
            sliderValueChange = false;
            this.PreviewMouseUp += CaNhan_PreviewMouseUp;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker2.DoWork += Worker2_DoWork;
            worker2.RunWorkerCompleted += Worker2_RunWorkerCompleted;
            id = _id;
            link = _link;
        }



        private void CaNhan_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sliderValueChange == true)
            {
                ProgressRing.Visibility = Visibility.Visible;
                btnDownload.IsEnabled = SliderChatLuong.IsEnabled = false;
                worker.RunWorkerAsync();
                sliderValueChange = false;
                e.Handled = true;
            }
        }

        #region Var
        string[] extras = { "", "url_m", "url_z", "url_c", "url_l", "url_h", "url_k", "url_o" };
        private List<string> dsLink = new List<string>();
        private string id;
        private string idAlbum;
        private string link;
        private string pathFolder;
        private bool sliderValueChange;
        private bool clicked = false;
        private bool workerRun = true;
        private bool? isAuto =false;
        BackgroundWorker worker = new BackgroundWorker();
        BackgroundWorker worker2 = new BackgroundWorker();
        GetImages images = new GetImages();
        WebClient wc = new WebClient();
        #endregion

        private void CaNhan_OnLoaded(object sender, RoutedEventArgs e)
        {
            ProgressRing.Visibility = Visibility.Visible;
            btnDownload.IsEnabled = SliderChatLuong.IsEnabled = false;
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
                sliderValueChange = true;
                switch (SliderChatLuong.Value)
                {
                    case 1:
                        LabelChatLuong.Content = "Trung bình 500";
                        break;
                    case 2:
                        LabelChatLuong.Content = "Trung bình 640";
                        break;
                    case 3:
                        LabelChatLuong.Content = "Trung bình 800";
                        break;
                    case 4:
                        LabelChatLuong.Content = "Lớn 1024";
                        break;
                    case 5:
                        LabelChatLuong.Content = "Lớn 1600";
                        break;
                    case 6:
                        LabelChatLuong.Content = "Lớn 2048";
                        break;
                    case 7:
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
            //FolderSelectDialog dlg = new FolderSelectDialog();
            WPFFolderBrowserDialog dlg = new WPFFolderBrowserDialog();
            dlg.Title = "Chọn thư mục để lưu ảnh!";
            if (dlg.ShowDialog()==true)
            {
                pathFolder = dlg.FileName;
                txbPath.Text = pathFolder;
            }
        }

        #region Worker
        //Worker lay anh
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            string _extras = "";
            Dispatcher.Invoke(() =>
            {
                _extras = extras[(int)SliderChatLuong.Value];
                btnDownload.IsEnabled = images.StopWorker = SliderChatLuong.IsEnabled = CkbAuto.IsEnabled = false;
            });
            if (string.IsNullOrEmpty(idAlbum))
                idAlbum = images.GetIdAlbum(link);
            images.GetAlbum(id, idAlbum, _extras, isAuto);
            if (images.AlbumOk) dsLink = images.Ds;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressRing.Visibility = Visibility.Hidden;
            LabelKetQua.Content = string.Format("Đã tìm thấy {0} ảnh có thể tải!", dsLink.Count);
            btnDownload.IsEnabled = SliderChatLuong.IsEnabled = btnSelectFolder.IsEnabled=CkbAuto.IsEnabled = true;
            SliderChatLuong.IsEnabled = !(bool)isAuto;
        }
        //Worker tai anh
        private void Worker2_DoWork(object sender, DoWorkEventArgs e)
        {
            double i = 0;
            foreach (string link in dsLink)
            {
                if (workerRun)
                {
                    //Neu co chon auto thi luc nay link trong dsLink chinh la ID
                    if (isAuto == true)
                    {
                        var linkDownload = images.GetImage(link);
                        wc.DownloadFileAsync(new Uri(linkDownload[linkDownload.Count - 1]), pathFolder + "\\" + Path.GetFileName(new Uri(linkDownload[linkDownload.Count - 1]).LocalPath));
                        i++;
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar.Value = Math.Truncate(i / dsLink.Count * 100);
                            LabelProgress.Content = string.Format("{0}%", ProgressBar.Value);
                        });
                        while (wc.IsBusy)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        wc.DownloadFileAsync(new Uri(link), pathFolder + "\\" + Path.GetFileName(new Uri(link).LocalPath));
                        i++;
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar.Value = Math.Truncate(i / dsLink.Count * 100);
                            LabelProgress.Content = string.Format("{0}%", ProgressBar.Value);
                        });
                        while (wc.IsBusy)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        private void Worker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnSelectFolder.IsEnabled = SliderChatLuong.IsEnabled=CkbAuto.IsEnabled = true;
            clicked = false;
            btnDownload.Content = "Tải";
            if (workerRun)
            {
                this.ShowMessageAsync("Thông báo!", "Đã tải về hoàn tất!");
                workerRun = true;
                ProgressBar.Value = 0;
                LabelProgress.Content = "0%";

                if (CkbOpenfolder.IsChecked == true)
                    Process.Start(pathFolder);
            }
            else
            {
                this.ShowMessageAsync("Thông báo!", "Đã hủy tải về!");
                workerRun = true;
                ProgressBar.Value = 0;
                LabelProgress.Content = "0%";
            }
        }

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
                        btnSelectFolder.IsEnabled = SliderChatLuong.IsEnabled=CkbAuto.IsEnabled = false;
                        worker2.RunWorkerAsync();
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

        //Kich hoat hoac vo hieu hoa sliderchatluong khi check va bo check
        private void CkbAuto_Checked(object sender, RoutedEventArgs e)
        {
            SliderChatLuong.IsEnabled = !SliderChatLuong.IsEnabled;
            isAuto = CkbAuto.IsChecked;
            //Neu chon auto thi tien hanh...
            if (isAuto == true)
            {
                dsLink.Clear();
                ProgressRing.Visibility = Visibility.Visible;
                btnDownload.IsEnabled = images.StopWorker = false;
                worker.RunWorkerAsync();
            }
        }

        private void CkbAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            SliderChatLuong.IsEnabled = !SliderChatLuong.IsEnabled;
            isAuto = CkbAuto.IsChecked;
            if (isAuto == false)
            {
                images.StopWorker = true;
                dsLink.Clear();
                ProgressRing.Visibility = Visibility.Visible;
                
                worker.RunWorkerAsync();
            }
        }
    }
}
