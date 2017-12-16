using System;
using System.Collections.Generic;
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

namespace 付箋アプリ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string ApplicationName = "Shuticky";
        public string StickyNoteApplicationFolderPath = "";

        public string StickyNoteListFilePath = "";
        public const string StickyNoteListFileName = "ShutickySetting.xml";

        public List<StickyNote> StickyNoteList = new List<StickyNote>();


        public List<string> StickyNoteFilePathListInDirectory = new List<string>();

        public List<string> ClosedStickyNameList = new List<string>();


        public MainWindow()
        {
            InitializeComponent();

            try
            {
                //OneDriveのフォルダのパスを取得
                const string userRoot = "HKEY_CURRENT_USER";
                const string subkey = @"Software\Microsoft\OneDrive";
                const string keyName = userRoot + "\\" + subkey;

                string oneDrivePath = (string)Microsoft.Win32.Registry.GetValue(keyName,
                   "UserFolder",
                  "Return this default if NoSuchName does not exist.");
                //Console.WriteLine("\r\n OneDrivePath : {0}", oneDrivePath);

                if (string.IsNullOrEmpty(oneDrivePath) == false)
                {
                    StickyNoteApplicationFolderPath = string.Format(oneDrivePath) + "\\" + "アプリ" + "\\" + ApplicationName;
                    System.IO.Directory.CreateDirectory(StickyNoteApplicationFolderPath);
                }

            }
            catch (Exception)
            {
                StickyNoteApplicationFolderPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + ApplicationName;
                System.IO.Directory.CreateDirectory(StickyNoteApplicationFolderPath);
            }

            StickyNoteListFilePath = StickyNoteApplicationFolderPath + "\\" + StickyNoteListFileName;

            StickyNoteFilePathListInDirectory = System.IO.Directory.GetFiles(StickyNoteApplicationFolderPath, "*.rtf", System.IO.SearchOption.TopDirectoryOnly).ToList();


            List<StickyNote> stickyNoteList_XML = new List<StickyNote>();
            if (System.IO.File.Exists(StickyNoteListFilePath) == true)
            {
                stickyNoteList_XML = this.ReadStickyNoteListXML(StickyNoteListFilePath);
            }


            StickyNoteList.AddRange(
                stickyNoteList_XML.FindAll(x =>
                {
                    string filePath = x.FilePath;
                    if (StickyNoteFilePathListInDirectory.Find(y => y == filePath) != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                })
                );

            foreach (string filePath in StickyNoteFilePathListInDirectory)
            {
                if (stickyNoteList_XML.Find(x => x.FilePath == filePath) == null)
                {
                    StickyNote newStkyNote = new StickyNote();
                    newStkyNote.FilePath = filePath;
                    StickyNoteList.Add(newStkyNote);
                }
            }



            if (StickyNoteList.Count > 0)
            {
                foreach (StickyNote stkyNote in StickyNoteList)
                {
                    付箋Window newStickyNote = new 付箋Window(stkyNote);

                    newStickyNote.G_MainWindow = this;
                }
            }
            else
            {
                付箋Window newStickyNote = new 付箋Window();

                newStickyNote.G_MainWindow = this;
                newStickyNote.InitNew付箋Window();
            }

        }


        /// <summary>
        /// 付箋情報リストの読み込み
        /// </summary>
        /// <param name="_filePath"></param>
        /// <returns></returns>
        public List<StickyNote> ReadStickyNoteListXML(string _filePath)
        {
            List<StickyNote> stickyNoteList = new List<StickyNote>();

            if (string.IsNullOrWhiteSpace(_filePath))
            {
                return stickyNoteList;
            }



            //XmlSerializerオブジェクトを作成
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<StickyNote>));
            //読み込むファイルを開く
            System.IO.StreamReader sr = new System.IO.StreamReader(_filePath, new System.Text.UTF8Encoding(false));
            //XMLファイルから読み込み、逆シリアル化する
            stickyNoteList = (List<StickyNote>)serializer.Deserialize(sr);
            //ファイルを閉じる
            sr.Close();

            return stickyNoteList;
        }

        /// <summary>
        /// 付箋情報リストの書き込み
        /// </summary>
        /// <param name="_filePath"></param>
        public void WriteStickyNoteListXML(string _filePath)
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                return;
            }

            if (this == null)
            {
                return;
            }

            //XmlSerializerオブジェクトを作成
            //オブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<StickyNote>));
            //書き込むファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(_filePath, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(sw, StickyNoteList);
            //ファイルを閉じる
            sw.Close();
        }

        /// <summary>
        /// メインウィンドウを閉じるときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }


    public class StickyNote
    {
        public string FilePath
        { get; set; }
        public string Title
        { get; set; }
        //public string Body
        //{ get; set; }

        public string ColorCode
        { get; set; }
        public int ColorNumber
        { get; set; }
        public double Position_X
        { get; set; }
        public double Position_Y
        { get; set; }
        public double Size_Width
        { get; set; }
        public double Size_Height
        { get; set; }

        public StickyNote()
        {
            FilePath = "";
            Title = "新規メモ";
            //Body = "";
            ColorCode = "";
            ColorNumber = 0;

            Position_X = 20;
            Position_Y = 20;

            Size_Width = 280;
            Size_Height = 250;
        }

    }

    public class BackGroundColorSet
    {
        public string TitleColor
        { get; set; }
        public string NoteColor
        { get; set; }

        public BackGroundColorSet()
        {
            TitleColor = "";
            NoteColor = "";
        }
    }
}
