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
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using IniParser;
using IniParser.Model;
using System.Net;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace CheckSkinLOL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private string version = "2.0";
        private string path = "";
        private string port = "";
        private string password = "";

        private string linkcheckver = "https://tanhiep.000webhostapp.com/ver/version";
        private string homepage = "https://127.0.0.1";
        private string linkme = "/lol-chat/v1/me";
        private string linkiconprofile = "/lol-game-data/assets/v1/profile-icons/";
        private string linkinvent = "/lol-collections/v1/inventories/";
        private string linkwallet = "/lol-store/v1/wallet";

        private int numChamps = 0;
        private int numSkinOwned = 0;

        private bool readLog = false;

        InfoAcc info;

        List<Champions> LChamps;
        List<SkinChamps> LSkins;
        List<DataSkin> dtSkin;
        List<DataChamp> dtChamp;
        ObservableCollection<DataSkin> listImgSkin;
        ObservableCollection<DataChamp> listImgChamp;

        public MainWindow()
        {
            InitializeComponent();

            //if (CheckVersion())
            //{
                CheckFileInfo();
            //    reLoad();
            //}
            //else
            //{
            //    MessageBox.Show("Đã có version mới vui lòng update !!!");
            //    Application.Current.Shutdown();
            //}
           
        }

        private void reLoad()
        {
            if (CheckVersion())
            {
                info = new InfoAcc();

                InitiForm();

                CheckPath(txtBox_Path.Text);
                readPORTANDPASSWORD();

                if (CheckOpenLOL() && readLog == true)
                {
                    getLinkme(getJSON(linkme));
                    //getIPAndRP(getJSON(linkwallet));
                    changeProfile();
                    getJSChamps();
                    processChamptoDataChamp();
                    processSkintoDataSkin();
                    showImageChamp();
                    showImageSkin();
                }
            }
            else
            {
                MessageBox.Show("Đã có version mới vui lòng update !!!");
                Application.Current.Shutdown();
            }            
        }

        private void InitiForm()
        {
            txtBox_Path.Text = path;
            readLog = false;
            port = "";
            password = "";
            LChamps = new List<Champions>();
            LSkins = new List<SkinChamps>();
            dtSkin = new List<DataSkin>();
            dtChamp = new List<DataChamp>();
            listImgSkin = new ObservableCollection<DataSkin>();
            listImgChamp = new ObservableCollection<DataChamp>();

        }

        private bool CheckOpenLOL()
        {
            Process[] processlist = Process.GetProcesses();
            bool flag = false;
            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName == "LeagueClient")
                {
                    flag = true;
                }
            }
            if (flag == false)
            {
                MessageBox.Show("Lỗi 03: Vui lòng mở LOL và nhấn Reload");
            }
            return flag ? true : false;
        }

        private bool CheckVersion()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(linkcheckver);

                return json == version ? true : false;
            }
        }

        private void CheckFileInfo()
        {
            path = readINI("infololvn.ini", "PATH", "PATH_LOL");
        }



        private void CheckPath(string p)
        {
            string pn = p;
            pn += @"\LeagueClient.exe";
            if (!File.Exists(pn))
            {
                img_CheckPath.Source = new BitmapImage(new Uri(@"/Images/notok.png", UriKind.Relative));
            }
            else
            {
                readLog = true;
                path = p;
                writeINI("infololvn.ini", "PATH", "PATH_LOL", p);
                img_CheckPath.Source = new BitmapImage(new Uri(@"/Images/ok.png", UriKind.Relative));
            }
        }

        private string readINI(string namefile, string s1, string s2) //s1 Muc lon s2 muc nho
        {
            string rs = "";
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(namefile);
            rs = data[s1][s2];
            return rs;
        }

        private void writeINI(string namefile, string s1, string s2, string s3) //s1 Muc lon s2 muc nho s3 chuoi can ghi
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(namefile);
            data[s1][s2] = s3;
            parser.WriteFile(namefile, data);
        }

        private void readPORTANDPASSWORD()
        {
            if (readLog)
            {
                string folderPath = path + @"\Logs\LeagueClient Logs";
                bool flag = false;
                int cout = Int32.Parse(Directory.EnumerateFiles(folderPath, "*.log").Count<string>().ToString()) - 1;
                string pathFile = "";
                string pathFileCopy = folderPath + @"\CheckLOLVN_FileCopy.txt";
                bool stopReadPass = false;
                bool stopReadPort = false;
                if (cout > -1)
                {
                    while (flag == false)
                    {
                        if (Directory.EnumerateFiles(folderPath, "*.log").ElementAt<string>(cout).IndexOf("LeagueClientUx") != -1 && Directory.EnumerateFiles(folderPath, "*.log").ElementAt<string>(cout).Length - folderPath.Length <= 50)
                        {
                            pathFile = Directory.EnumerateFiles(folderPath, "*.log").ElementAt<string>(cout);
                            flag = true;
                        }
                        cout--;
                    }

                    File.Copy(pathFile, pathFileCopy, true);

                    try
                    {
                        using (StreamReader sr = new StreamReader(pathFileCopy))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line.IndexOf("--remoting-auth-token=") != -1 && stopReadPass == false)
                                {
                                    for (int i = line.IndexOf("=") + 1; i < line.Length; i++)
                                    {
                                        password += line[i];
                                    }
                                    stopReadPass = true;
                                }

                                if (line.IndexOf("--app-port=") != -1 && stopReadPort == false)
                                {
                                    for (int i = line.IndexOf("=") + 1; i < line.Length; i++)
                                    {

                                        port += line[i];
                                    }
                                    stopReadPort = true;
                                }
                            }
                        }
                    }
                    catch (Exception Exp)
                    {
                        MessageBox.Show("Lỗi 01: Không đọc được file");
                        MessageBox.Show(Exp.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Lỗi 02: LOL vừa cập nhật tắt LOL và mở lại");
                }
            }
            else
            {
                MessageBox.Show("Lỗi 09: Chưa cập nhật đường dẫn LOL");
            }
        }

        private string getJSON(string link)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.ServerCertificateValidationCallback = delegate (
            Object obj, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors errors)
            {
                return (true);
            };


            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=utf-8";
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + Base64Encode("riot:" + password);
                client.Headers[HttpRequestHeader.Referer] = homepage + ":" + port + "/index.html";
                var responseString = client.DownloadString(homepage + ":" + port + link);
                return responseString;
            }

        }

        private void getLinkme(string js)
        {
            dynamic array = JsonConvert.DeserializeObject(js);
            info.name = array.name;
            info.id = array.id;
            info.idProfileIcon = array.icon;
        }

        private void getIPAndRP(string js)
        {
            dynamic array = JsonConvert.DeserializeObject(js);
            info.ip = array.ip;
            info.rp = array.rp;
        }

        private void getJSChamps()
        {
            string linkChamps = linkinvent + info.id + "/champions";


            dynamic array = JsonConvert.DeserializeObject(getJSON(linkChamps));
            numChamps = array.Count;

            foreach (var ch in array)
            {
                Champions champ = new Champions();
                champ.champName = ch.name;
                champ.idChamp = ch.id;
                champ.Owned = getOwned(ch.ownership.ToString());
                champ.numSkin = getNumSkin(ch.skins.ToString());
                getSkin(ch.skins.ToString());
                LChamps.Add(champ);
            }
        }

        private void getSkin(string js)
        {

            dynamic array = JsonConvert.DeserializeObject(js);
            foreach (var sk in array)
            {
                SkinChamps skin = new SkinChamps();
                skin.idChamps = sk.championId;
                skin.idSkin = sk.id;
                skin.nameSkin = sk.name.ToString();
                getChromas(sk.chromas.ToString());
                skin.Owned = getOwned(sk.ownership.ToString());

                if (skin.Owned == "True")
                {
                    numSkinOwned += 1;
                }
                skin.splashPath = sk.splashPath;
                skin.tilePath = sk.tilePath;
                skin.uncenteredSplashPath = sk.uncenteredSplashPath;
                LSkins.Add(skin);

            }
        }

        private void getChromas(string js)
        {
            dynamic array = JsonConvert.DeserializeObject(js);
            foreach (var sk in array)
            {
                SkinChamps skin = new SkinChamps();
                skin.idChamps = sk.championId;
                skin.idSkin = sk.id;
                skin.nameSkin = "Skin Đa Sắc";
                skin.Owned = getOwned(sk.ownership.ToString());

                if (skin.Owned == "True")
                {
                    numSkinOwned += 1;
                }
                skin.splashPath = sk.splashPath;
                skin.tilePath = sk.tilePath;
                skin.uncenteredSplashPath = sk.uncenteredSplashPath;
                LSkins.Add(skin);
            }
        } 

        private string getOwned(string js)
        {
            dynamic array = JsonConvert.DeserializeObject(js);
            return array.owned;
        }

        private int getNumSkin(string js)
        {
            dynamic array = JsonConvert.DeserializeObject(js);
            return array.Count;
        }

        private void changeProfile()
        {
            string linkImgProfile = homepage + ":" + port + linkiconprofile + info.idProfileIcon + ".jpg";
            img_IconProfile.Fill = new ImageBrush(new BitmapImage(new Uri(linkImgProfile, UriKind.Absolute)));
            lbl_name.Content = info.name;
            lbl_ip.Content = info.ip;
            lbl_rp.Content = info.rp;
        }

        private void processSkintoDataSkin()
        {
            string linkimg = "";
            DataSkin dt = new DataSkin();
            numSkinOwned = 0;

            foreach (SkinChamps sk in LSkins)
            {
                linkimg = @"https://tanhiep.000webhostapp.com/skins/championsskin_";
                if (sk.Owned == "True")
                {
                    
                    if ((Int32.Parse(sk.idSkin) % 1000) != 0)
                    {
                        dt.Title = sk.nameSkin;
                        linkimg += sk.idSkin + ".jpg";
                        dt.ImageData = LoadImage(linkimg);
                        dtSkin.Add(dt);
                        listImgSkin.Add(new DataSkin()
                        {
                            ImageData = dt.ImageData,
                            Title = dt.Title
                        });
                    }
                }

                numSkinOwned = dtSkin.Count;
                ShowNumSkin(numSkinOwned);

            };
        }

        private void processChamptoDataChamp()
        {
            string linkimg = "";
            DataChamp dt = new DataChamp();
            numChamps = 0;

            foreach (Champions sk in LChamps)
            {
                linkimg = @"https://tanhiep.000webhostapp.com/champions/";
                if (sk.Owned == "True")
                {
                    dt.Title = sk.champName;
                    linkimg += sk.idChamp + "_Web_0.jpg";
                    Logs(linkimg);
                    dt.ImageData = LoadImage(linkimg);
                    dtChamp.Add(dt);
                    listImgChamp.Add(new DataChamp()
                    {
                        ImageData = dt.ImageData,
                        Title = dt.Title
                    });
                }

                numChamps = dtChamp.Count;
                ShowNumChamp(numChamps);

            };
        }

        private void ShowNumSkin(int num)
        {
            lbl_Numskin.Content = num.ToString();
        }

        private void ShowNumChamp(int num)
        {
            lbl_NumsChamp.Content = num.ToString();
        }

        private BitmapImage LoadImage(string link)
        {
            return new BitmapImage(new Uri(link, UriKind.Absolute));
        }

        private void showImageSkin()
        {
            ViewSkin.ItemsSource = listImgSkin;
        }

        private void showImageChamp()
        {
            ViewChamp.ItemsSource = listImgChamp;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private void btn_Browers_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Search LeagueClient Folder";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                String p = dlg.FileName;
                txtBox_Path.Text = p;
                CheckPath(p);
            }
        }

        private void Logs(string log)
        {
            using (StreamWriter sw = new StreamWriter("Logs.txt", true))
            {
                sw.WriteLine(log + Environment.NewLine);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            numChamps = 0;
            numSkinOwned = 0;
            reLoad();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            File.Delete("idchamp.txt");
            File.Delete("idskin.txt");

            using (StreamWriter sw = new StreamWriter("idchamp.txt", true))
            {
                ;
                foreach (Champions c in LChamps)
                {
                    if (c.Owned == "True")
                    {
                        sw.WriteLine(c.champName);
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter("idskin.txt", true))
            {
                foreach (SkinChamps sk in LSkins)
                {
                    if (sk.Owned == "True")
                    {
                        if ((Int32.Parse(sk.idSkin) % 1000) != 0)
                        {
                            sw.WriteLine("championsskin_" + sk.idSkin + ".jpg");
                        }
                    }
                }
            }
        }
    }
}
