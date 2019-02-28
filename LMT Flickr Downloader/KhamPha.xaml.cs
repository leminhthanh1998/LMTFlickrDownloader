using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using WPFFolderBrowser;
using Path = System.IO.Path;

namespace LMT_Flickr_Downloader
{
    /// <summary>
    /// Interaction logic for AnhThuVi.xaml
    /// </summary>
    public partial class KhamPha : MetroWindow
    {
        public KhamPha()
        {
            InitializeComponent();
            sliderValueChange = false;
            this.PreviewMouseUp += KhamPha_PreviewMouseUp;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.DoWork += Worker_DoWork;
            worker2.RunWorkerCompleted += Worker2_RunWorkerCompleted;
            worker2.DoWork += Worker2_DoWork;
        }

        

        /// <summary>
        /// khoi chay lay link lan dau
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KhamPha_OnLoaded(object sender, RoutedEventArgs e)
        {
            ProgressRing.Visibility = Visibility.Visible;
            btnDownload.IsEnabled = SliderChatLuong.IsEnabled = false;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Chay worker moi khi tha chuot ra
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KhamPha_PreviewMouseUp(object sender, MouseButtonEventArgs e)
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

        #region Private member
        string[] extras = {"", "url_m", "url_z", "url_c", "url_l", "url_h", "url_k", "url_o" };
        private List<string> dsLink= new List<string>();
        private string pathFolder;
        private bool sliderValueChange;
        private bool clicked = false;
        private bool workerRun = true;//Worker co dang chay hay khong
        private bool? isAuto = false;//Tu dong tai chat luong tot nhat
        private BackgroundWorker worker= new BackgroundWorker();
        private BackgroundWorker worker2= new BackgroundWorker();
        private GetImages images= new GetImages();
        private WebClient wc= new WebClient();
        #endregion


        

        
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
            
            string _extras="";
            Dispatcher.Invoke(() =>
            {
                _extras = extras[(int) SliderChatLuong.Value];
                btnDownload.IsEnabled = images.StopWorker = SliderChatLuong.IsEnabled = CkbAuto.IsEnabled = false;
            });
            images.GetDocDao(_extras, isAuto);
            dsLink = images.Ds;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressRing.Visibility = Visibility.Hidden;
            LabelKetQua.Content = string.Format("Đã tìm thấy {0} ảnh có thể tải!", dsLink.Count);
            btnDownload.IsEnabled = SliderChatLuong.IsEnabled=btnSelectFolder.IsEnabled=CkbAuto.IsEnabled = true;
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
                    if(isAuto==true)
                    {
                        var linkDownload = images.GetImage(link);
                        wc.DownloadFileAsync(new Uri(linkDownload[linkDownload.Count-1]), pathFolder + "\\" + Path.GetFileName(new Uri(linkDownload[linkDownload.Count - 1]).LocalPath));
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
            SliderChatLuong.IsEnabled = !(bool)isAuto;
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

        /// <summary>
        /// Tai anh ve
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                btnDownload.IsEnabled=images.StopWorker = false;
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

        /// <summary>
        /// Thay doi label chat luong khi keo thanh slider
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
    }
}
