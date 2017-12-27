using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;
using System.Xml.Serialization;


namespace 付箋アプリ
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// タスクトレイに表示するアイコン
        /// </summary>
        private NotifyIconWrapper _notifyIcon;
        private Mutex _mutex;

        //アプリケーション名
        private readonly string _applicationName = 付箋アプリ.Properties.Settings.Default.ApplicationName;
        private string _shutickyNoteApplicationFolderPath = "";
        private string _shutickySettingFilePath = "";
        private readonly string _shutickySettingFileName = 付箋アプリ.Properties.Settings.Default.SettingFileName;
        private readonly string _defaultShutickyName = 付箋アプリ.Properties.Settings.Default.DefaultShutickyName;
        private readonly string _onedriveCommonApplicationFolderName = 付箋アプリ.Properties.Settings.Default.OnedriveCommonApplicationFolderName;

        private List<ShutickyWindow> _shutickyWindows = new List<ShutickyWindow>();

        private double _positionIncrementX = 付箋アプリ.Properties.Settings.Default.PositionIncrementX;
        private double _positionIncrementY = 付箋アプリ.Properties.Settings.Default.PositionIncrementY;
        private double _defaultPositionX = 付箋アプリ.Properties.Settings.Default.DefaultPositionX;
        private double _defaultPositionY = 付箋アプリ.Properties.Settings.Default.DefaultPositionY;
        private double _defaultWidth = 付箋アプリ.Properties.Settings.Default.DefaultWidth;
        private double _defaultHeight = 付箋アプリ.Properties.Settings.Default.DefaultHeight;

        protected override void OnStartup(StartupEventArgs e)
        {
            //二重起動防止
            _mutex = new Mutex(false, _applicationName);
            if (!_mutex.WaitOne(0, false))
            {
                //既に起動しているため、終了させる
                _mutex.Close();
                _mutex = null;
                this.Shutdown();
            }

            base.OnStartup(e);

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this._notifyIcon = new NotifyIconWrapper();
            this._notifyIcon.ContextMenuItem_Exit_Clicked += ContextMenuItem_Exit_Clicked;
            this._notifyIcon.ContextMenuItem_New_Clicked += ContextMenuItem_New_Clicked;
            this._notifyIcon.ContextMenuItem_ShowAll_Clicked += ContextMenuItem_ShowAll_Clicked;
            this._notifyIcon.ContextMenuItem_MinimizeAll_Clicked += ContextMenuItem_MinimizeAll_Clicked;

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
                    _shutickyNoteApplicationFolderPath = Path.Combine(string.Format(oneDrivePath), _onedriveCommonApplicationFolderName, _applicationName);
                    Directory.CreateDirectory(_shutickyNoteApplicationFolderPath);
                }
            }
            catch (Exception)
            {
                //OneDriveフォルダにフォルダを作成できなかった場合、マイドキュメントにフォルダを作る。
                _shutickyNoteApplicationFolderPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), _applicationName);
                Directory.CreateDirectory(_shutickyNoteApplicationFolderPath);
            }

            //設定ファイルのパスを取得
            _shutickySettingFilePath = Path.Combine(_shutickyNoteApplicationFolderPath, _shutickySettingFileName);

            //設定ファイルを読み込む
            //なければ初期化
            List<ShutickySetting> shutickySettings = null;
            try
            {
                if (File.Exists(_shutickySettingFilePath))
                {
                    //RTFファイルが存在するもののみに絞って。
                    shutickySettings = this.ReadShutickySettingListXML(_shutickySettingFilePath).Where(x => File.Exists(x.FilePath)).ToList();
                }
                else
                {
                    shutickySettings = new List<ShutickySetting>();
                }
            }
            catch
            {
                shutickySettings = new List<ShutickySetting>();
            }

            //RTFファイルのみがフォルダ内に置かれた場合、
            //それらが読み込まれるようにする
            //付箋データファイル（つまりrtfファイル）のパスの一覧を取得
            //ただし、セッティングリストには記載されていないファイルのみを取得
            var newRtfFilePathList = Directory.GetFiles(_shutickyNoteApplicationFolderPath, "*.rtf", SearchOption.TopDirectoryOnly)
                                              .Where(rtfPath => shutickySettings.FindIndex(setting => setting.FilePath == rtfPath) == -1)
                                              .ToList();
            foreach (var rtfPath in newRtfFilePathList)
            {
                try
                {
                    var shutickySetting = new ShutickySetting(rtfPath);

                    shutickySettings.Add(shutickySetting);
                }
                catch
                {
                    continue;
                }
            }

            //付箋ウィンドウの生成
            if (shutickySettings.Count > 0)//既存の付箋が１つ以上あった場合。付箋ウィンドウを開く。
            {
                foreach (var shutickySetting in shutickySettings)
                {
                    try
                    {
                        //付箋ウィンドウをインスタンス化
                        var shutickyWindow = new ShutickyWindow(shutickySetting);
                        //イベントハンドラを登録
                        AddEventHandlersToShutickyWindow(shutickyWindow);

                        //付箋ウィンドウのリストに登録
                        _shutickyWindows.Add(shutickyWindow);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            else//なければ空の付箋ウィンドウを開く
            {
                var newRtfPath = Path.Combine(_shutickyNoteApplicationFolderPath, $"{GenerateNewTitle()}.rtf");
                var newShutickySetting = new ShutickySetting(newRtfPath);
                AddNewShutickyWindow(newShutickySetting);
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            this._notifyIcon.Dispose();

            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
            }
        }

        private string GenerateNewTitle()
        {
            var newShutickyName = _defaultShutickyName;

            try
            {
                //デフォルト名のファイルが既に存在する場合は、
                //新しいファイル名を生成する。
                if (_shutickyWindows.FindIndex(x => x.GetShutickySetting().Title == _defaultShutickyName) > -1)
                {
                    //既にある付箋名と被らないようにする
                    int namePostfix = 1;
                    while (true)
                    {
                        if (_shutickyWindows.FindIndex(x => x.GetShutickySetting().Title == $"{_defaultShutickyName}_{namePostfix}") > -1)
                        {
                            namePostfix++;
                        }
                        else
                        {
                            newShutickyName = $"{_defaultShutickyName}_{namePostfix}";

                            break;
                        }
                    }
                }
            }
            catch
            {
                newShutickyName = _defaultShutickyName;
            }

            return newShutickyName;
        }

        private void AddEventHandlersToShutickyWindow(ShutickyWindow shutickyWindow)
        {
            if (shutickyWindow == null)
            {
                return;
            }

            shutickyWindow.Closed += ShutickyWindowClosed;
            shutickyWindow.Deactivated += ShutickyWindowDeactivated;
            shutickyWindow.DeleteButtonClicked += ShutickyWindowDeleteButtonClicked;
            shutickyWindow.TitleLostFocus += ShutickyWindowTitleLostFocus;
            shutickyWindow.SaveButtonClicked += ShutickyWindowSaveButtonClicked;
            shutickyWindow.CloseButtonClicked += ShutickyWindowCloseButtonClicked;
            shutickyWindow.NewShutickyButtonClicked += ShutickyWindowNewButtonClicked;
            shutickyWindow.MinimizeButtonClicked += ShutickyWindowMinimizeButtonClicked;
        }

        private void ShutickyWindowMinimizeButtonClicked(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;

            senderWindow.SetDisplayStatus(DisplayStatus.Minimize);
        }
        private void ShutickyWindowNewButtonClicked(object sender, EventArgs e)
        {
            var newRtfPath = Path.Combine(_shutickyNoteApplicationFolderPath, $"{GenerateNewTitle()}.rtf");
            var newShutickySetting = new ShutickySetting(newRtfPath);

            //新規付箋ボタンが押されたWindowから少しずらした位置に表示させる
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();
            newShutickySetting.Position_X = senderSetting.Position_X + _positionIncrementX;
            newShutickySetting.Position_Y = senderSetting.Position_Y + _positionIncrementY;

            AddNewShutickyWindow(newShutickySetting);
        }
        private void ShutickyWindowClosed(object sender, EventArgs e)
        {

        }
        private void ShutickyWindowDeactivated(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();

            //現在の設定内容でセッティングリストの該当データを更新
            UpdateShutickyWindows(senderSetting);

            //セッティングリストを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);

            senderWindow.SaveRTF();
        }
        private void ShutickyWindowDeleteButtonClicked(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();

            //RTFファイルを削除
            //新規作成されたShutickyウィンドウが、RTFファイルが作成される前に閉じられることもある。
            //ファイルの存在確認を行う。
            if (File.Exists(senderSetting.FilePath))
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(senderSetting.FilePath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            }

            //リストから消す
            var shutickySettingIdx = _shutickyWindows.FindIndex(x => x.GetShutickySetting().FilePath == senderSetting.FilePath);
            if (shutickySettingIdx > -1)
            {
                _shutickyWindows.RemoveAt(shutickySettingIdx);
            }

            //セッティングリストを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);

            senderWindow.Close();
        }
        private void ShutickyWindowSaveButtonClicked(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();
            UpdateShutickyWindows(senderSetting);

            //セッティングリストを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);

            senderWindow.SaveRTF();
        }
        private void ShutickyWindowTitleLostFocus(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();


            string newTitle = senderWindow.textBox_Title.Text;
            string newFilePath = Path.Combine(_shutickyNoteApplicationFolderPath, $"{newTitle}.rtf");


            //そもそもファイル名に変更がなかった場合
            //何もしない。
            if (newTitle == senderSetting.Title)
            {
                return;
            }


            //ファイル名として使用できない文字のパターン
            System.Text.RegularExpressions.Regex reCantUseCharAsFileName = new System.Text.RegularExpressions.Regex(@"[/\\<>\*\?""\|:;]");
            if (reCantUseCharAsFileName.IsMatch(newTitle) == true)
            {
                MessageBox.Show(@"/ \ < > * ? "" | : ;　はタイトルとして使用できない文字です");

                //titleを元に戻す
                senderWindow.textBox_Title.Text = senderSetting.Title;

                return;
            }


            //同名のファイルが存在するかチェック
            if (File.Exists(newFilePath) == true)
            {
                MessageBox.Show("同名の付箋が既に存在します。");

                //titleを元に戻す
                senderWindow.textBox_Title.Text = senderSetting.Title;

                return;
            }

            //ファイル名を変更
            File.Move(senderSetting.FilePath, newFilePath);

            //付箋データファイル名を変更
            senderSetting.Title = newTitle;

            //付箋データのファイルパスを変更。
            senderSetting.FilePath = newFilePath;


            //現在の設定内容でセッティングリストの該当データを更新
            UpdateShutickyWindows(senderSetting);

            //セッティングリストを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);

            senderWindow.SaveRTF();
        }
        private void ShutickyWindowCloseButtonClicked(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();

            //Windowを非表示に
            senderWindow.SetDisplayStatus(DisplayStatus.Hidden);

            //現在の設定内容でセッティングリストの該当データを更新
            UpdateShutickyWindows(senderSetting);

            //セッティングリストを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);

            //RTFを保存
            senderWindow.SaveRTF();

            senderWindow.Close();
        }



        private void AddNewShutickyWindow(ShutickySetting shutickySetting)
        {
            if (shutickySetting == null)
            {
                return;
            }

            //他の付箋と左上角の座標が重ならないように新規座標を計算
            //ただし、画面外に行ってしまわないように、
            var newPos = GetNewPositionTuple(shutickySetting.Position_X, shutickySetting.Position_Y);
            shutickySetting.Position_X = newPos.x;
            shutickySetting.Position_Y = newPos.y;

            var newShutickyWindow = new ShutickyWindow(shutickySetting);
            AddEventHandlersToShutickyWindow(newShutickyWindow);

            _shutickyWindows.Add(newShutickyWindow);

            //セッティングファイルを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);
        }
        private void UpdateShutickyWindows(ShutickySetting shutickySetting)
        {
            var shutickySettingIdx = _shutickyWindows.FindIndex(x => x.GetShutickySetting().FilePath == shutickySetting.FilePath);

            if (shutickySettingIdx > -1)
            {
                try
                {
                    _shutickyWindows[shutickySettingIdx].SetShutickySetting(shutickySetting);
                }
                catch
                {
                    return;
                }
            }
        }
        /// <summary>
        /// 付箋情報リストの読み込み
        /// </summary>
        /// <param name="_filePath"></param>
        /// <returns></returns>
        public IEnumerable<ShutickySetting> ReadShutickySettingListXML(string _filePath)
        {
            var shutickySettings = new List<ShutickySetting>();

            if (string.IsNullOrWhiteSpace(_filePath))
            {
                return shutickySettings;
            }

            //XmlSerializerオブジェクトを作成
            XmlSerializer serializer = null;// new System.Xml.Serialization.XmlSerializer(typeof(List<ShutickySetting>));
            //読み込むファイルを開く
            StreamReader strmReader = null;// new StreamReader(_filePath, new System.Text.UTF8Encoding(false));
            try
            {
                serializer = new XmlSerializer(typeof(List<ShutickySetting>));
                using (strmReader = new StreamReader(_filePath, new System.Text.UTF8Encoding(false)))
                {
                    //XMLファイルから読み込み、逆シリアル化する
                    shutickySettings = (List<ShutickySetting>)serializer.Deserialize(strmReader);
                }
            }
            catch
            {
                strmReader.Dispose();
            }

            return shutickySettings;
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

            List<ShutickySetting> shutickySettings=null;
            XmlSerializer serializer = null;
            StreamWriter strmWriter = null;
            try
            {
                shutickySettings = _shutickyWindows.Select(w => w.GetShutickySetting()).ToList();
                //XmlSerializerオブジェクトを作成
                //オブジェクトの型を指定する
                serializer = new XmlSerializer(typeof(List<ShutickySetting>));
            }
            catch
            {
                return;
            }

            try
            {
                //書き込むファイルを開く（UTF-8 BOM無し）
                using (strmWriter = new StreamWriter(_filePath, false, new System.Text.UTF8Encoding(false)))
                {
                    //シリアル化し、XMLファイルに保存する
                    serializer.Serialize(strmWriter, shutickySettings);
                }
            }
            catch
            {
                strmWriter.Dispose();

                return;
            }
        }



        private void ContextMenuItem_New_Clicked(object sender, EventArgs e)
        {
            var newRtfPath = Path.Combine(_shutickyNoteApplicationFolderPath, $"{GenerateNewTitle()}.rtf");
            var newShutickySetting = new ShutickySetting(newRtfPath);

            AddNewShutickyWindow(newShutickySetting);
        }
        private (double x, double y) GetNewPositionTuple(double baseX, double baseY)
        {
            double newX = baseX;
            double newY = baseY;

            try
            {
                foreach (var shutickyWindow in _shutickyWindows)
                {
                    var shutickySetting = shutickyWindow.GetShutickySetting();
                    if (newX == shutickySetting.Position_X || newX == shutickySetting.Position_Y)
                    {
                        newX += _positionIncrementX;
                        newY += _positionIncrementY;

                        //画面外にはみ出さないようにする。
                        if (newX + _defaultWidth + 50 > System.Windows.SystemParameters.WorkArea.Width)
                        {
                            newX = _defaultPositionX;
                        }
                        if (newY + _defaultHeight + 50 > System.Windows.SystemParameters.WorkArea.Height)
                        {
                            newY = _defaultPositionY;
                        }
                    }
                }
            }
            catch
            {
                newX = baseX;
                newY = baseY;
            }

            return (newX, newY);
        }

        private void ContextMenuItem_Exit_Clicked(object sender, EventArgs e)
        {
            this.Shutdown();
        }
        private void ContextMenuItem_ShowAll_Clicked(object sender, EventArgs e)
        {
            foreach (var shutickyWindow in _shutickyWindows)
            {
                try
                {
                    shutickyWindow.SetDisplayStatus(DisplayStatus.Visible);
                }
                catch
                {
                    continue;
                }
            }
        }
        private void ContextMenuItem_MinimizeAll_Clicked(object sender, EventArgs e)
        {
            foreach (var shutickyWindow in _shutickyWindows)
            {
                try
                {
                    shutickyWindow.SetDisplayStatus(DisplayStatus.Minimize);
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
