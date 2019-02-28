using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;

namespace LMT_Flickr_Downloader
{
    class GetImages
    {
        #region Var
        const string anhDocDao = "flickr.interestingness.getList";
        const string caNhan = "flickr.people.getPublicPhotos";
        const string album = "flickr.photosets.getPhotos";
        private const string hinhAnh = "flickr.photos.getSizes";
        private int page = 1;
        private string extras;
        private string id;
        private string albumID;
        private bool albumOk=true;
        private List<string> ds;

        /// <summary>
        /// Danh sach link download anh
        /// </summary>
        public List<string> Ds
        {
            get => ds;
            set => ds = value;
        }

        public int Page
        {
            get => page;
            set => page = value;
        }

        public string Extras
        {
            get => extras;
            set => extras = value;
        }

        public string Id
        {
            get => id;
            set => id = value;
        }

        public string AlbumID
        {
            get => albumID;
            set => albumID = value;
        }
        public bool AlbumOk { get => albumOk; set => albumOk = value; }

        public bool StopWorker { get; set; } = false;

        #endregion
        /// <summary>
        /// Kiem tra api 
        /// </summary>
        /// <param name="apiFlickr"></param>
        /// <returns></returns>
        public string CheckAPI(string apiFlickr)
        {
            string doc = "";
            WebClient web = new WebClient();
            try
            {
                doc = web.DownloadString("https://api.flickr.com/services/rest/?method=flickr.test.echo&api_key=" + apiFlickr);
            }
            catch (Exception )
            {
                return "NotNetwork";
            }
            if (doc.Contains("Invalid API Key (Key has invalid format)"))
            {

                return "invalid";
            }
            else if (doc.Contains("Invalid API Key (Key has expired)"))
            {

                return "exprired";
            }
            return "OK";
        }


        /// <summary>
        /// Check ID nguoi dung
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public string CheckID(string link)
        {
            string _id = "";
            if (link.Contains("flickr.com"))
            {
                string linkAPI = "https://api.flickr.com/services/rest/?method=flickr.urls.lookupUser" + "&url=" + link + "&api_key=" + MainWindow.apiFlickr;
                var web = new WebClient();
                string doc = web.DownloadString(linkAPI);
                if (doc.Contains("User not found"))
                {
                    return "Notfound";
                }
                else
                {
                    MatchCollection id = Regex.Matches(doc, @"\b(id=)\S+\w");
                    return _id= id[0].Value.Replace("id=\"", "").Replace("\"", "");
                }
            }
            else
                return _id;
        }

        /// <summary>
        /// Lay id cua album
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public string GetIdAlbum(string link)
        {
            string doc=new WebClient().DownloadString(link);
            MatchCollection ms1 = Regex.Matches(doc, @"\b(?:https?://www.flickr.com/photos)\S+\b");
            if (ms1.Count > 0)
            {
                string[] numbers = Regex.Split(ms1[0].Value, @"\D+");
                return numbers[numbers.Count() - 1];
            }
            return "";
        }

        

        /// <summary>
        /// Lay cac hinh anh tu trang kham pha cua flickr
        /// </summary>
        /// <param name="extras"></param>
        public void GetDocDao(string extras, bool? isAuto)
        {
            try
            {
                Ds = new List<string>();
                if (isAuto == true)
                {
                    //Tao 1 danh sach chua id cua hinh anh
                    List<string> DsID = new List<string>();

                    //Neu auto thi dau tien tao 1 link api khong co thuoc tinh kich thuoc
                    string linkAPI = "https://api.flickr.com/services/rest/?method=" + anhDocDao +
                                 "&api_key=" + MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                    var web = new WebClient();
                    string doc = web.DownloadString(linkAPI);

                    MatchCollection pages = Regex.Matches(doc, @"\b(pages=)\S+\w");
                    int trang = Convert.ToInt32(pages[0].Value.Replace("pages=\"", ""));
                    if (trang == 1)
                    {
                        //Lay id
                        MatchCollection ms1 = Regex.Matches(doc, @"\b(id=)\S+\b");
                        foreach (Match item in ms1)
                        {
                            string id = item.Value.Replace("id=\"", "");
                            DsID.Add(id);
                        }
                    }
                    else
                    {
                        batdau:
                        string linkAPI2 = "https://api.flickr.com/services/rest/?method=" + anhDocDao + "&api_key=" + MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                        string doc2 = web.DownloadString(linkAPI2);
                        //Lay id
                        MatchCollection ms = Regex.Matches(doc2, @"\b(id=)\S+\b");
                        foreach (Match item in ms)
                        {
                            string id = item.Value.Replace("id=\"", "");
                            DsID.Add(id);
                        }
                        Page++;
                        if (Page <= trang)
                        {
                            Thread.Sleep(3000);
                            goto batdau;
                        }
                    }
                    Page = 1;
                    //Sau khi lay cac id thi tien hanh lay link tai co kich thuoc lon nhat
                    //foreach (var id in DsID)
                    //{
                    //    var linkDownload = GetImage(id);
                    //    Ds.Add(linkDownload[linkDownload.Count - 1]);
                    //    if (StopWorker) break;
                    //}
                    Ds = DsID;

                }
                //Neu khong auto thi tien hanh lay cach link anh theo kich thuoc da chon
                else
                {
                    string linkAPI = "https://api.flickr.com/services/rest/?method=" + anhDocDao + "&extras=" + extras +
                                 "&api_key=" + MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                    var web = new WebClient();
                    string doc = web.DownloadString(linkAPI);

                    MatchCollection pages = Regex.Matches(doc, @"\b(pages=)\S+\w");
                    int trang = Convert.ToInt32(pages[0].Value.Replace("pages=\"", ""));
                    if (trang == 1)
                    {
                        MatchCollection ms1 = Regex.Matches(doc, @"\b(?:https?://)\S+\b");
                        foreach (Match item in ms1)
                        {
                            string text = item.Value.Replace("\"", "").Replace("\"", "");
                            Ds.Add(text);
                        }
                    }
                    else
                    {
                        batdau:
                        string linkAPI2 = "https://api.flickr.com/services/rest/?method=" + anhDocDao + "&extras=" +
                                          extras + "&api_key=" + MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                        string doc2 = web.DownloadString(linkAPI2);
                        MatchCollection ms = Regex.Matches(doc2, @"\b(?:https?://)\S+\b");
                        foreach (Match item in ms)
                        {
                            string text = item.Value.Replace("\"", "").Replace("\"", "");
                            Ds.Add(text);
                        }
                        Page++;
                        if (Page <= trang)
                        {
                            Thread.Sleep(3000);
                            goto batdau;
                        }
                    }
                    Page = 1;
                }
            }
            catch (Exception)
            {
                
            }

        }

        /// <summary>
        /// Lay cac hinh anh tu trang ca nhan
        /// </summary>
        /// <param name="id"></param>
        /// <param name="extras"></param>
        public void GetCaNhan(string id, string extras, bool? isAuto)
        {
            try
            {
                Ds = new List<string>();
                if (isAuto == true)
                {
                    //Tao danh sach id
                    List<string> DsID = new List<string>();

                    string linkAPI = "https://api.flickr.com/services/rest/?method=" + caNhan + "&user_id=" + id + "&api_key=" + MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                    var web = new WebClient();
                    string doc = web.DownloadString(linkAPI);

                    MatchCollection pages = Regex.Matches(doc, @"\b(pages=)\S+\w");
                    int trang = Convert.ToInt32(pages[0].Value.Replace("pages=\"", ""));
                    if (trang == 1)
                    {
                        MatchCollection ms1 = Regex.Matches(doc, @"\b(id=)\S+\b");
                        foreach (Match item in ms1)
                        {
                            string idImg = item.Value.Replace("id=\"", "");
                            DsID.Add(idImg);
                        }
                    }
                    else
                    {
                        batdau:
                        string linkAPI2 = "https://api.flickr.com/services/rest/?method=" + caNhan + "&user_id=" + id + "&api_key=" + MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                        string doc2 = web.DownloadString(linkAPI2);
                        MatchCollection ms = Regex.Matches(doc2, @"\b(id=)\S+\b");
                        foreach (Match item in ms)
                        {
                            string idImg = item.Value.Replace("id=\"", "");
                            DsID.Add(idImg);
                        }
                        Page++;
                        if (Page <= trang)
                        {
                            Thread.Sleep(3000);
                            goto batdau;
                        }
                    }
                    Page = 1;
                    //Sau khi lay cac id thi tien hanh lay link tai co kich thuoc lon nhat
                    Ds = DsID;

                }
                //Neu khong auto thi thuc hien binh thuong
                else
                {
                    string linkAPI = "https://api.flickr.com/services/rest/?method=" + caNhan + "&user_id=" + id + "&extras=" +
                                 extras + "&api_key=" + MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                    var web = new WebClient();
                    string doc = web.DownloadString(linkAPI);

                    MatchCollection pages = Regex.Matches(doc, @"\b(pages=)\S+\w");
                    int trang = Convert.ToInt32(pages[0].Value.Replace("pages=\"", ""));
                    if (trang == 1)
                    {
                        MatchCollection ms1 = Regex.Matches(doc, @"\b(?:https?://)\S+\b");
                        foreach (Match item in ms1)
                        {
                            string text = item.Value.Replace("\"", "").Replace("\"", "");
                            Ds.Add(text);
                        }
                    }
                    else
                    {
                        batdau:
                        string linkAPI2 = "https://api.flickr.com/services/rest/?method=" + caNhan + "&user_id=" + id +
                                          "&extras=" + extras + "&api_key=" + MainWindow.apiFlickr + "&per_page=500" +
                                          "&page=" + Page;
                        string doc2 = web.DownloadString(linkAPI2);
                        MatchCollection ms = Regex.Matches(doc2, @"\b(?:https?://)\S+\b");
                        foreach (Match item in ms)
                        {
                            string text = item.Value.Replace("\"", "").Replace("\"", "");
                            Ds.Add(text);
                        }
                        Page++;
                        if (Page <= trang)
                        {
                            Thread.Sleep(3000);
                            goto batdau;
                        }
                    }
                    Page = 1;
                }
            }
            catch (Exception)
            {
                
            }

        }

        /// <summary>
        /// Lay cac hinh anh trong album
        /// </summary>
        /// <param name="id"></param>
        /// <param name="albumID"></param>
        /// <param name="extras"></param>
        public void GetAlbum(string id, string albumID, string extras, bool? isAuto)
        {
            try
            {
                Ds = new List<string>();
                if(isAuto==true)
                {
                    //Tao danh sach id
                    List<string> DsID = new List<string>();

                    string linkAPI = "https://api.flickr.com/services/rest/?method=" + album + "&user_id=" + id +
                                 "&photoset_id=" + albumID + "&api_key=" + MainWindow.apiFlickr +
                                 "&per_page=500" + "&page=" + Page;
                    var web = new WebClient();
                    string doc = web.DownloadString(linkAPI);
                    if (doc.Contains("Photoset not found"))
                    {
                        AlbumOk = false;
                        return;
                    }
                    MatchCollection pages = Regex.Matches(doc, @"\b(pages=)\S+\w");
                    int trang = Convert.ToInt32(pages[0].Value.Replace("pages=\"", ""));
                    if (trang == 1)
                    {
                        MatchCollection ms1 = Regex.Matches(doc, @"\b(id=)\S+\b");
                        foreach (Match item in ms1)
                        {
                            string text = item.Value.Replace("id=\"", "");
                            DsID.Add(text);
                        }
                    }
                    else
                    {
                        batdau:
                        string linkAPI2 = "https://api.flickr.com/services/rest/?method=" + album + "&user_id=" + Id +
                                          "&photoset_id=" + AlbumID + "&extras=" + Extras + "&api_key=" +
                                          MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                        string doc2 = web.DownloadString(linkAPI2);
                        MatchCollection ms = Regex.Matches(doc2, @"\b(?:https?://)\S+\b");
                        foreach (Match item in ms)
                        {
                            string text = item.Value.Replace("\"", "").Replace("\"", "");
                            Ds.Add(text);
                        }
                        Page++;
                        if (Page <= trang)
                        {
                            Thread.Sleep(3000);
                            goto batdau;
                        }
                    }
                    Page = 1;
                    //Sau khi lay cac id thi tien hanh lay link tai co kich thuoc lon nhat
                    Ds = DsID;
                    Ds.RemoveAt(0);
                    AlbumOk = true;
                }
                //Neu khong auto thi thuc hien nhu binh thuong
                else
                {
                    string linkAPI = "https://api.flickr.com/services/rest/?method=" + album + "&user_id=" + id +
                                 "&photoset_id=" + albumID + "&extras=" + extras + "&api_key=" + MainWindow.apiFlickr +
                                 "&per_page=500" + "&page=" + Page;
                    var web = new WebClient();
                    string doc = web.DownloadString(linkAPI);
                    if (doc.Contains("Photoset not found"))
                    {
                        AlbumOk = false;
                        return;
                    }
                    MatchCollection pages = Regex.Matches(doc, @"\b(pages=)\S+\w");
                    int trang = Convert.ToInt32(pages[0].Value.Replace("pages=\"", ""));
                    if (trang == 1)
                    {
                        MatchCollection ms1 = Regex.Matches(doc, @"\b(?:https?://)\S+\b");
                        foreach (Match item in ms1)
                        {
                            string text = item.Value.Replace("\"", "").Replace("\"", "");
                            Ds.Add(text);
                        }
                    }
                    else
                    {
                        batdau:
                        string linkAPI2 = "https://api.flickr.com/services/rest/?method=" + album + "&user_id=" + Id +
                                          "&photoset_id=" + AlbumID + "&extras=" + Extras + "&api_key=" +
                                          MainWindow.apiFlickr + "&per_page=500" + "&page=" + Page;
                        string doc2 = web.DownloadString(linkAPI2);
                        MatchCollection ms = Regex.Matches(doc2, @"\b(?:https?://)\S+\b");
                        foreach (Match item in ms)
                        {
                            string text = item.Value.Replace("\"", "").Replace("\"", "");
                            Ds.Add(text);
                        }
                        Page++;
                        if (Page <= trang)
                        {
                            Thread.Sleep(3000);
                            goto batdau;
                        }
                    }
                    Page = 1;
                    AlbumOk = true;
                }
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// Lay id cua anh dang xem
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public  string GetIDdPhoto(string link)
        {
            string doc = new WebClient().DownloadString(link);
            MatchCollection ms1 = Regex.Matches(doc, @"\b(?:https?://www.flickr.com/photos)\S+\b");
            if (ms1.Count > 0)
            {
                string[] numbers = Regex.Split(ms1[0].Value, @"\D+");
                return numbers[numbers.Count() - 1];
            }
            return "";
        }

        /// <summary>
        /// Lay link tai anh, tra ra cac link anh voi kich thuoc khac nhau
        /// </summary>
        /// <param name="id"></param>
        public List<string> GetImage(string id)
        {
            List<string> DsLink = new List<string>();

            try
            {
                string linkAPI = "https://api.flickr.com/services/rest/?method=" + hinhAnh +
                                 "&api_key=" + MainWindow.apiFlickr + "&photo_id=" + id;
                var web = new WebClient();
                string doc = web.DownloadString(linkAPI);
                MatchCollection ms1 = Regex.Matches(doc, @"\b(?:https?://)\S+\b");
                foreach (Match item in ms1)
                {
                    string text = item.Value.Replace("\"", "").Replace("\"", "");
                    if (text.Contains("staticflickr") && !(text.Contains("_s") || text.Contains("_q") || text.Contains("_t") || text.Contains("_m") || text.Contains("_n")))
                        DsLink.Add(text);
                }
                return DsLink;
            }
            catch (Exception)
            {
                
            }
            return DsLink;

        }
    }
}
