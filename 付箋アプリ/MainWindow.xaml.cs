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
using System.IO;

namespace 付箋アプリ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //アプリケーション名
        public readonly string ApplicationName = "Shuticky";
        public string ShutickyNoteApplicationFolderPath = "";
        public string ShutickySettingFilePath = "";
        public readonly string ShutickySettingFileName = "ShutickySetting.xml";

        public List<ShutickySetting> ShutickySettingList = new List<ShutickySetting>();

        //public List<string> ShutickyRtfFilePathList = new List<string>();

        public List<string> ClosedShutickyNameList = new List<string>();


        public MainWindow()
        {
            InitializeComponent();

            try
            {
                //OneDriveのフォルダのパスを取得
                const string userRoot = "HKEY_CURRENT_USER";
                const string subkey = @"Software\Microsoft\OneDrive";
                const string keyName = userRoot + "\\" + subkey;

                //OneDriveフォルダのパスを取得
                string oneDrivePath = (string)Microsoft.Win32.Registry.GetValue(keyName, "UserFolder", "Return this default if NoSuchName does not exist.");

                //Shuticky用のフォルダを作成。
                if (string.IsNullOrEmpty(oneDrivePath) == false)
                {
                    ShutickyNoteApplicationFolderPath = Path.Combine(string.Format(oneDrivePath), "アプリ", ApplicationName);
                    Directory.CreateDirectory(ShutickyNoteApplicationFolderPath);
                }

            }
            catch (Exception)
            {
                //OneDriveフォルダにフォルダを作成できなかった場合、マイドキュメントにフォルダを作る。
                ShutickyNoteApplicationFolderPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), ApplicationName);
                Directory.CreateDirectory(ShutickyNoteApplicationFolderPath);
            }


            //設定ファイルのパスを取得
            ShutickySettingFilePath = Path.Combine(ShutickyNoteApplicationFolderPath, ShutickySettingFileName);
            //設定ファイルを読み込む
            //なければ初期化
            if (File.Exists(ShutickySettingFilePath))
            {
                //RTFファイルが存在するもののみに絞って。
                ShutickySettingList = this.ReadShutickySettingListXML(ShutickySettingFilePath).Where(x=>File.Exists(x.FilePath)).ToList();
            }
            else
            {
                ShutickySettingList = new List<ShutickySetting>();
            }

            //RTFファイルのみがフォルダ内に置かれた場合、
            //それらが読み込まれるようにする
            //付箋データファイル（つまりrtfファイル）のパスの一覧を取得
            //ただし、セッティングリストには記載されていないファイルのみを取得
            var newRtfFilePathList = Directory.GetFiles(ShutickyNoteApplicationFolderPath, "*.rtf", SearchOption.TopDirectoryOnly)
                                              .Where(rtfPath=>ShutickySettingList.FindIndex(setting=>setting.FilePath==rtfPath)==-1)
                                              .ToList();
            foreach(var rtfPath in newRtfFilePathList)
            {
                var shutickySetting = new ShutickySetting(rtfPath);

                ShutickySettingList.Add(shutickySetting);
            }


            //付箋ウィンドウの生成
            if (ShutickySettingList.Count > 0)//既存の付箋が１つ以上あった場合。付箋ウィンドウを開く。
            {
                foreach (var stkySetting in ShutickySettingList)
                {
                    var newShutickyWindow = new ShutickyWindow(stkySetting);
                    AddEventHandlersToShutickyWindow(newShutickyWindow);
                }
            }
            else//なければ空の付箋ウィンドウを開く
            {
                var newRtfPath = Path.Combine(ShutickyNoteApplicationFolderPath,$"{GenerateNewTitle()}.rtf");
                var newShutickySetting = new ShutickySetting(newRtfPath);
                var newShutickyWindow = new ShutickyWindow(newShutickySetting);
                AddEventHandlersToShutickyWindow(newShutickyWindow);
            }

        }



        public string GenerateNewTitle()
        {
            //初期付箋名をつける
            string defaultFileName = "新規メモ";
            string defaultFilePath = Path.Combine(ShutickyNoteApplicationFolderPath, $"{defaultFileName}.rtf");
            if (File.Exists(defaultFilePath) == true)
            {
                //既にある付箋名と被らないようにする
                int namePostfix = 1;
                while (true)
                {
                    defaultFilePath = Path.Combine(ShutickyNoteApplicationFolderPath, $"{defaultFileName}_{namePostfix}.rtf");
                    if (File.Exists(defaultFilePath) == true)
                    {
                        namePostfix++;
                    }
                    else
                    {
                        defaultFileName = $"{defaultFileName}_{namePostfix}";

                        break;
                    }
                }
            }

            return defaultFileName;
        }


        private void AddEventHandlersToShutickyWindow(ShutickyWindow shutickyWindow)
        {
            shutickyWindow.Closed += ShutickyWindowClosed;
            shutickyWindow.Deactivated += ShutickyWindowDeactivated;
            shutickyWindow.DeleteButtonClicked += ShutickyWindowDeleteButtonClicked;
            shutickyWindow.TitleLostFocus += ShutickyWindowTitleLostFocus;
            shutickyWindow.SaveButtonClicked += ShutickyWindowSaveButtonClicked;
            shutickyWindow.CloseButtonClicked += ShutickyWindowCloseButtonClicked;
            shutickyWindow.NewShutickyButtonClicked += ShutickyWindowNewButtonClicked;
        }
        private void ShutickyWindowNewButtonClicked(object sender,EventArgs e)
        {
            var newRtfPath = Path.Combine(ShutickyNoteApplicationFolderPath, $"{GenerateNewTitle()}.rtf");
            var newShutickySetting = new ShutickySetting(newRtfPath);

            //新規付箋ボタンが押されたWindowから少しずらした位置に表示させる
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();
            newShutickySetting.Position_X = senderSetting.Position_X + 20;
            newShutickySetting.Position_Y = senderSetting.Position_Y + 20;

            //色もランダムに変える。
            var r = new System.Random(DateTime.Now.Second);
            var newColorNumber = r.Next(ShutickyWindow.PresetBackGroundColorSet.Count);
            newShutickySetting.ColorNumber = newColorNumber;

            var newShutickyWindow =new ShutickyWindow(newShutickySetting);
            AddEventHandlersToShutickyWindow(newShutickyWindow);

            //セッティングリストに追加
            ShutickySettingList.Add(newShutickySetting);

            //セッティングファイルを書き込み
            WriteShutickySettingListXML(ShutickySettingFilePath);
        }
        private void ShutickyWindowClosed(object sender,EventArgs e)
        {

        }
        private void ShutickyWindowDeactivated(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();

            //現在の設定内容でセッティングリストの該当データを更新
            UpdateShutickySettingList(senderSetting);

            //セッティングリストを書き込み
            WriteShutickySettingListXML(ShutickySettingFilePath);
        }
        private void ShutickyWindowDeleteButtonClicked(object sender,EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();

            //RTFファイルを削除
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(senderSetting.FilePath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

            //リストから消す
            var shutickySettingIdx = ShutickySettingList.FindIndex(x => x.FilePath == senderSetting.FilePath);
            if (shutickySettingIdx > -1)
            {
                ShutickySettingList.RemoveAt(shutickySettingIdx);
            }

            //セッティングリストを書き込み
            WriteShutickySettingListXML(ShutickySettingFilePath);

            senderWindow.Close();
        }
        private void ShutickyWindowSaveButtonClicked(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();
            UpdateShutickySettingList(senderSetting);

            //セッティングリストを書き込み
            WriteShutickySettingListXML(ShutickySettingFilePath);
        }
        private void ShutickyWindowTitleLostFocus(object sender,EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();

            //現在の設定内容でセッティングリストの該当データを更新
            UpdateShutickySettingList(senderSetting);

            //セッティングリストを書き込み
            WriteShutickySettingListXML(ShutickySettingFilePath);
        }
        private void ShutickyWindowCloseButtonClicked(object sender,EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();

            //現在の設定内容でセッティングリストの該当データを更新
            UpdateShutickySettingList(senderSetting);

            //セッティングリストを書き込み
            WriteShutickySettingListXML(ShutickySettingFilePath);

            //閉じられた付箋リストに登録
            ClosedShutickyNameList.Add(senderSetting.Title);

            senderWindow.Close();
        }


        private void UpdateShutickySettingList(ShutickySetting setting)
        {
            var stickyNoteListIdx = ShutickySettingList.FindIndex(x => x.FilePath == setting.FilePath);

            if (stickyNoteListIdx > -1)
            {
                ShutickySettingList[stickyNoteListIdx] = setting;
            }
            else
            {
                ShutickySettingList.Add(setting);
            }
        }
        /// <summary>
        /// 付箋情報リストの読み込み
        /// </summary>
        /// <param name="_filePath"></param>
        /// <returns></returns>
        public IEnumerable<ShutickySetting> ReadShutickySettingListXML(string _filePath)
        {
            var stickyNoteList = new List<ShutickySetting>();

            if (string.IsNullOrWhiteSpace(_filePath))
            {
                return stickyNoteList;
            }

            //XmlSerializerオブジェクトを作成
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ShutickySetting>));
            //読み込むファイルを開く
            var sr = new StreamReader(_filePath, new System.Text.UTF8Encoding(false));
            //XMLファイルから読み込み、逆シリアル化する
            stickyNoteList = (List<ShutickySetting>)serializer.Deserialize(sr);
            //ファイルを閉じる
            sr.Close();

            return stickyNoteList;
        }
        /// <summary>
        /// 付箋情報リストの書き込み
        /// </summary>
        /// <param name="_filePath"></param>
        public void WriteShutickySettingListXML(string _filePath)
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
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ShutickySetting>));
            //書き込むファイルを開く（UTF-8 BOM無し）
            var sw = new StreamWriter(_filePath, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(sw, ShutickySettingList);
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





}
