using HttpServerUtils;
using QWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

namespace HttpPublish
{

    public enum DisplayHttpItemType
    {
        Text, Html, FileStream, Video, Image
    }
    public class DisplayHttpItem
    {
        private static int _ID { get; set; }
        public static int AutoID { get => ++_ID; }
        public int ID { get; set; } = AutoID;
        public String Url { get => ("http://" + MainWindow.usingIpAddresss.Last().ToString() + "/" + ID+"/"); }
        public DisplayHttpItemType ItemType { get; set; }
        public String Title { get; set; }
        public FileInfo File { get; set; }
        public string Data { get; set; }
        public DisplayHttpItem() { }
        public DisplayHttpItem(FileInfo file, DisplayHttpItemType type)
        {
            this.Title = file.Name;
            this.File = file;
            this.ItemType = type;
        }
        public DisplayHttpItem(string title, string data,  DisplayHttpItemType type)
        {
            this.Title = title;
            this.Data = data;
            this.ItemType = type;
        }
        public DisplayHttpItem(string data, DisplayHttpItemType type)
        {
            this.Data = data;
            this.ItemType = type;
        }
        public static DisplayHttpItem CreateImage(FileInfo file, string url)
        {
            return new DisplayHttpItem(file, DisplayHttpItemType.Image);
        }
        public static DisplayHttpItem CreateHtml(string str, string url)
        {
            return new DisplayHttpItem(str, DisplayHttpItemType.Html);
        }
        public static DisplayHttpItem CreateText(String text, string url) => new DisplayHttpItem(text, DisplayHttpItemType.Text);
        public static DisplayHttpItem CreateVideo(FileInfo file, string url) => new DisplayHttpItem(file, DisplayHttpItemType.Video);


    }
    public partial class MainWindow : WindowBasic
    {
        public static HttpServer server = new HttpServer();
        public static int ipPort = 8003;
        public static string ipPortStr => ipPort.ToString();
        public static ObservableCollection<IPEndPoint> usingIpAddresss = new ObservableCollection<IPEndPoint>();
        public static string usingIpAddresssStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var i in usingIpAddresss)
                {
                    sb.Append(i.Address + "   ");
                }
                return sb.ToString();
            }
        }
        static MainWindow()
        {
            var ipv4 = HttpUtils.GetIpv4IpAddresss();
            int count = 0;
            foreach (var i in ipv4)
            {
                var u = "http://" + i.ToString() + ":" + ipPort + "/";
                server.AddListenUrl(u);
                usingIpAddresss.Add(new IPEndPoint(i, ipPort));
                count += 1;
            }
        }



        private ObservableCollection<FileInfo> NowDragFiles = new ObservableCollection<FileInfo>();
        public bool IsNowDropFile
        {
            get { return (bool)GetValue(IsNowDropFileProperty); }
            set { SetValue(IsNowDropFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNowDropFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNowDropFileProperty =
            DependencyProperty.Register("IsNowDropFile", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));





        public MainWindow()
        {
            InitializeComponent();
            //server.AddListenUrl("http://127.0.0.1:8003/");
            //server.AddListenUrl("http://127.0.0.1:8004/");
            //server.AddContextWriterImage("http://127.0.0.1:8004/", new System.IO.FileInfo("C:\\Users\\quxin\\Desktop\\AAAAAA\\新建文件夹\\202381\\正常\\d.png"), false);
            //server.AddContextWriterImage("http://127.0.0.1:8004/1", new System.IO.FileInfo("C:\\Users\\quxin\\Desktop\\AAAAAA\\新建文件夹\\202381\\正常\\d.png"), true);
            //LIST_Display.Items.Add(new DisplayHttpItem()
            //{
            //    Url = "http://127.0.0.1:8004/1",
            //    Title = "图片1",
            //    ItemType = DisplayHttpItemType.Image,
            //    File = new System.IO.FileInfo("C:\\Users\\quxin\\Desktop\\AAAAAA\\新建文件夹\\202381\\正常\\d.png")
            //}); LIST_Display.Items.Add(new DisplayHttpItem()
            //{
            //    Url = "http://127.0.0.1:8004/1",
            //    Title = "图片1",
            //    ItemType = DisplayHttpItemType.Image,
            //    File = new System.IO.FileInfo("C:\\Users\\quxin\\Desktop\\AAAAAA\\新建文件夹\\202381\\正常\\d.png")
            //});
            AddItem(new FileInfo("C:\\Users\\quxin\\Desktop\\AAAAAA\\新建文件夹\\202381\\正常\\d.png"));
        }

        private void BT_ControlButton_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            DisplayHttpItem item = bt.DataContext as dynamic;
            if (item != null)
            {
                switch (bt.Tag.ToString())
                {
                    case "WEB": OpenUrl(item.Url); break;
                    case "LINK": Clipboard.SetText(item.Url); break;
                    case "OPEN": Process.Start(new ProcessStartInfo("explorer.exe", item.File.Directory.FullName) { UseShellExecute = true }); break;
                }
            }
        }
        private void OpenUrl(String url)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //不使用shell启动
            p.StartInfo.RedirectStandardInput = true;//喊cmd接受标准输入
            p.StartInfo.RedirectStandardOutput = false;//不想听cmd讲话所以不要他输出
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示窗口
            p.Start();

            //向cmd窗口发送输入信息 后面的&exit告诉cmd运行好之后就退出
            p.StandardInput.WriteLine("start " + url + "&exit");
            p.StandardInput.AutoFlush = true;
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }


        protected override void OnDragEnter(DragEventArgs e)
        {
            IDataObject data = e.Data;
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                var fl = data.GetData((DataFormats.FileDrop)) as String[];
                DropingFiles(fl);
            }

            IsNowDropFile = true;
            base.OnDragEnter(e);
        }
        protected override void OnDragLeave(DragEventArgs e)
        {
            IsNowDropFile = false;
            base.OnDragLeave(e);
        }
        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
        }
        protected override void OnPreviewDrop(DragEventArgs e)
        {
            DropedFiles();
            IsNowDropFile = false;
            base.OnPreviewDrop(e);
        }
        private void DropedFiles()
        {
            foreach (var i in NowDragFiles)
            {
                if(i.Exists)
                AddItem(i);
            }
        }

        private void DropingFiles(String[] fs)
        {
            NowDragFiles.Clear();
            TEXT_DragFileTitle.Text = "";
            foreach (var i in fs)
            {
                var f = new FileInfo(i);
                NowDragFiles.Add(f);
                TEXT_DragFileTitle.Text += f.Name;
            }
        }
        public void AddItem(FileInfo file)
        {
            string[] imge = new string[] { ".png", ".jpg", ".bmp" };
            string[] video = new string[] { ".mp4", ".flv" };
            var e = file.Extension.ToLower();
            DisplayHttpItemType t = imge.Any(a => a == e) ? DisplayHttpItemType.Image : video.Any(a => a == e) ? DisplayHttpItemType.Video : DisplayHttpItemType.FileStream;
            int id = DisplayHttpItem.AutoID;
            //var url = "http://"+usingIpAddresss.Last().ToString()+"/"+id+"/";
            var item = new DisplayHttpItem(file, t);
            if (PublishIteme(item))
            {
                LIST_Display.Items.Add(item);
            }
        }
        private bool PublishIteme(DisplayHttpItem item)
        {
            try
            {
                if (!server.AddListenUrl(item.Url)) return false;
                switch (item.ItemType)
                {
                    case DisplayHttpItemType.Text:
                        server.AddContextWriterString(item.Url, item.Data);
                        break;
                    case DisplayHttpItemType.Html:
                        server.AddContextWriterHtml(item.Url, item.Data);
                        break;
                    case DisplayHttpItemType.FileStream:
                        server.AddContextWriterFile(item.Url, item.File, true);
                        break;
                    case DisplayHttpItemType.Video:
                        server.AddContextWriterVideo(item.Url, item.File, false);
                        break;
                    case DisplayHttpItemType.Image:
                        server.AddContextWriterImage(item.Url, item.File, false);
                        break;
                    default:
                        throw new Exception("未定义类型");
                        break;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IsNowDropFileProperty && NowDragFiles != null)
            {
                this.BORDER_DragFileMark.Visibility = IsNowDropFile ? Visibility.Visible : Visibility.Collapsed;
            }
            base.OnPropertyChanged(e);
        }
    }
}
