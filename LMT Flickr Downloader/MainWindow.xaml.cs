using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CefSharp;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace LMT_Flickr_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            CommandSaveFile.InputGestures.Add(new KeyGesture(Key.F1));
            CommandExplor.InputGestures.Add(new KeyGesture(Key.F2));
            CommandAlbum.InputGestures.Add(new KeyGesture(Key.F3));
            CommandAccount.InputGestures.Add(new KeyGesture(Key.F4));
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

            if (File.Exists(pathSettingFile))
                apiFlickr = File.ReadAllText(pathSettingFile);
            else
            {
                Directory.CreateDirectory(pathSettingFile.Replace(@"\api.dat", ""));
                File.Create(pathSettingFile).Close();
            }
            if (string.IsNullOrEmpty(apiFlickr) && File.Exists(pathSettingFile))
            {
                GetAPI api = new GetAPI();
                api.Owner = this;
                api.ShowDialog();
            }
            string check = images.CheckAPI(apiFlickr);
            if (check == "invalid")
            {
                GetAPI api = new GetAPI();
                api.LabelThongBao.Text = "API của bạn không đúng, vui lòng kiểm tra và nhập API lại!";
                api.ShowDialog();
            }
            if (check == "exprired")
            {
                GetAPI api = new GetAPI();
                api.LabelThongBao.Text = "API của bạn đã hết hạn, vui lòng kiểm tra và nhập API lại!";
                api.ShowDialog();
            }
            if (check == "NotNetwork")
            {
                NotNetwork notNetwork = new NotNetwork();
                notNetwork.ShowDialog();
            }
        }

        #region Var

        private string pathSettingFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\LMT Flickr Downloader" + "\\api.dat";
        public static string apiFlickr;
        GetImages images = new GetImages();
        public static RoutedCommand CommandSaveFile = new RoutedCommand();
        public static RoutedCommand CommandExplor = new RoutedCommand();
        public static RoutedCommand CommandAlbum = new RoutedCommand();
        public static RoutedCommand CommandAccount = new RoutedCommand();

        /// <summary>
        /// Link de kiem tra update
        /// </summary>
        private string updateLink = "https://xn--lminhthnh-w1a7h.vn/lmt-flickr-downloader/file/ver.txt";

        /// <summary>
        /// Link download
        /// </summary>
        private string downloadLink = "http://lêminhthành.vn/lmt-flickr-downloader";

        #endregion

        #region WebNavigate

        private void BtnBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (Browser.CanGoBack)
                Browser.Back();
        }

        private void BtnForward_OnClick(object sender, RoutedEventArgs e)
        {
            if (Browser.CanGoForward)
                Browser.Forward();
        }


        private void TxtBoxAddress_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (txtBoxAddress.Text.Contains("flickr.com") || txtBoxAddress.Text.Contains("flic.kr"))
                {
                    Browser.Address = txtBoxAddress.Text;
                }
                else this.ShowMessageAsync("Thông báo!", "Không thể truy cập các trang ngoài Flickr!");
            }

        }

        #endregion

        #region Fun usefull
        /// <summary>
        /// Tai anh tu trang kham pha
        /// </summary>
        private void Explore()
        {
            Browser.Address = "https://www.flickr.com/explore";
            if (Browser.Address.Contains("flickr.com/explore") || Browser.Address.Contains("flickr.com/photos"))
            {
                KhamPha explore = new KhamPha();
                explore.Owner = this;
                explore.Show();
            }
        }

        /// <summary>
        /// Tai anh cua nguoi dung
        /// </summary>
        private void Account()
        {
            string id = images.CheckID(Browser.Address);
            if (id == "Notfound")
                this.ShowMessageAsync("Lỗi!", "Không tìm thấy tài khoản nguời dùng!");
            else
            {
                CaNhan cn = new CaNhan(id);
                cn.Owner = this;
                cn.Show();
            }
        }

        /// <summary>
        /// tai anh album
        /// </summary>
        private void Album()
        {
            if (Browser.Address.Contains("flickr.com") && (Browser.Address.Contains("album") || Browser.Address.Contains("sets")))
            {
                //string idPhotosets = images.GetIdAlbum(Browser.Address);
                string id = images.CheckID(Browser.Address);
                if (id != "Notfound")
                {
                    Album album = new Album(id, Browser.Address);
                    album.Owner = this;
                    album.Show();
                }
                else this.ShowMessageAsync("Lỗi!", "Không tìm thấy album để tải!");
            }
            else this.ShowMessageAsync("Lỗi!", "Không tìm thấy album để tải!");
        }

        /// <summary>
        /// tai anh dang xem
        /// </summary>
        private void SaveImage()
        {

            HinhAnh hinhAnh = new HinhAnh(Browser.Address);
            hinhAnh.Owner = this;
            hinhAnh.Show();
            //else this.ShowMessageAsync("Lỗi!", "Không tìm thấy ảnh để lưu!");
        }
        #endregion

        #region Button

        /// <summary>
        /// Tai anh ve tu trang Kham pha của Flickr
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAllImageExplo_OnClick(object sender, RoutedEventArgs e)
        {
            Explore();
        }

        /// <summary>
        /// Tai anh cua tai khoan FLickr
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAllImageAcc_OnClick(object sender, RoutedEventArgs e)
        {
            Account();
        }

        /// <summary>
        /// Tai anh tu album cua nguoi dung
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAllImageAlbum_OnClick(object sender, RoutedEventArgs e)
        {
            Album();
        }

        private void BtnSaveImage_OnClick(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        #endregion

        #region Phim tac

        private void CommandSaveBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveImage();
        }

        private void CommandExplorBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Explore();
        }

        private void CommandAlbumBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Album();
        }

        private void CommandAccBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Account();
        }

        #endregion

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void BtnHelo_OnClick(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Owner = this;
            about.Show();
        }

        /// <summary>
        /// Kiem tra cap nhat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            await CheckUpdate();
        }

        /// <summary>
        /// Kiem tra xem co update cho phan mem khong
        /// </summary>
        private async Task CheckUpdate()
        {
            try
            {
                string version = await GetDoc(updateLink);
                if (Convert.ToDouble(version) > 11)
                {
                    var result = await this.ShowMessageAsync("Thông báo","Đã có bản cập nhật mới, bạn có muốn tải về không?", MessageDialogStyle.AffirmativeAndNegative);
                    bool yes = result == MessageDialogResult.Affirmative;
                    if (yes)
                    {
                        Process.Start(downloadLink);
                    }
                }
                else await this.ShowMessageAsync("Thông báo","Bạn đang dùng phiên bản mới nhất!");
            }
            catch
            {
                await this.ShowMessageAsync("Thông báo","Đã có lỗi khi kiểm tra cập nhật!");
            }
        }

        private async Task<string> GetDoc(string link)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(new Uri(link));
            }
        }
    }
}
