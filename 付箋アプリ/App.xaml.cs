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
using System.Reflection;
using System.Windows.Input;
using System.Windows.Threading;
using System.Text.RegularExpressions;


namespace 付箋アプリ
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private DispatcherTimer _remindTimer;


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
        private readonly string _appTrashcanFolderName = 付箋アプリ.Properties.Settings.Default.AppTrashcanName;
        private string _helpFilePath = "";
        private string _shutickyTrashcanFolderPath = "";


        //生成された付箋ウィンドウのリスト。
        private List<ShutickyWindow> _shutickyWindows = new List<ShutickyWindow>();


        private double _positionIncrementX = 付箋アプリ.Properties.Settings.Default.PositionIncrementX;
        private double _positionIncrementY = 付箋アプリ.Properties.Settings.Default.PositionIncrementY;
        private double _defaultPositionX = 付箋アプリ.Properties.Settings.Default.DefaultPositionX;
        private double _defaultPositionY = 付箋アプリ.Properties.Settings.Default.DefaultPositionY;
        private double _defaultWidth = 付箋アプリ.Properties.Settings.Default.DefaultWidth;
        private double _defaultHeight = 付箋アプリ.Properties.Settings.Default.DefaultHeight;


        #region イベント
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
            this._notifyIcon.ContextMenuItem_Help_Clicked += ContextMenuItem_Help_Clicked;
            this._notifyIcon.ContextMenuItem_ClearTrash_Clicked += ContextMenuItem_ClearTrash_Clicked;

            //インストール先のフォルダにヘルプファイル(html)を配置する
            var asm = Assembly.GetEntryAssembly();
            var appExeFilePath = asm.Location;
            var appExeFolderPath = Path.GetDirectoryName(appExeFilePath);
            _helpFilePath = Path.Combine(appExeFolderPath, "help.html");
            File.WriteAllText(_helpFilePath, 付箋アプリ.Properties.Resources.Help);


            //アプリケーション用のフォルダを作成
            try
            {
                //OneDriveのフォルダのパスを取得
                string userRoot = "HKEY_CURRENT_USER";
                string subkey = @"Software\Microsoft\OneDrive";
                string keyName = Path.Combine(userRoot, subkey);

                //OneDriveフォルダのパスを取得
                string oneDrivePath = (string)Microsoft.Win32.Registry.GetValue(keyName, "UserFolder", "Return this default if NoSuchName does not exist.");

                //Shuticky用のフォルダを作成。
                if (string.IsNullOrEmpty(oneDrivePath) == false)
                {
                    _shutickyNoteApplicationFolderPath = Path.Combine(oneDrivePath, _onedriveCommonApplicationFolderName, _applicationName);
                }
            }
            catch (Exception)
            {
                //OneDriveフォルダにフォルダを作成できなかった場合、マイドキュメントにフォルダを作る。
                _shutickyNoteApplicationFolderPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), _applicationName);
            }
            finally
            {
                //Shuticky用のフォルダを作成。
                Directory.CreateDirectory(_shutickyNoteApplicationFolderPath);

                //アプリ用のゴミ箱フォルダを作成
                _shutickyTrashcanFolderPath = Path.Combine(_shutickyNoteApplicationFolderPath, _appTrashcanFolderName);
                Directory.CreateDirectory(_shutickyTrashcanFolderPath);
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

            //TODO:
            //アプリのゴミ箱(Trashフォルダ)内のRTFファイルのリストを作成
            //コンテキストメニューのゴミ箱項目内に追加する
            var trashedRtfFilePaths = Directory.GetFiles(_shutickyTrashcanFolderPath, "*.rtf", SearchOption.TopDirectoryOnly);
            foreach (var trashedRtfFilePath in trashedRtfFilePaths)
            {
                var trashedShutickyTitle = Path.GetFileNameWithoutExtension(trashedRtfFilePath);
                AddContextMenuItem_Trash(trashedShutickyTitle);
            }

            //付箋ウィンドウの生成
            if (shutickySettings.Count > 0)//既存の付箋が１つ以上あった場合。すでにある付箋を開く。
            {
                foreach (var shutickySetting in shutickySettings)
                {
                    try
                    {
                        //既存の付箋の表示
                        AddExistShutickyWindow(shutickySetting);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            else//なければ空の付箋ウィンドウを開く
            {
                var newRtfPath = Path.Combine(_shutickyNoteApplicationFolderPath, $"{GenerateNewTitle(_defaultShutickyName)}.rtf");
                var newShutickySetting = new ShutickySetting(newRtfPath);
                AddNewShutickyWindow(newShutickySetting);
            }



            //すべてのリマインダを表示
            ShowAllReminder();

            //リマインダータイマーをスタート
            _remindTimer = new DispatcherTimer();
            _remindTimer.Interval = TimeSpan.FromMinutes(1);
            _remindTimer.Tick += RemindTimer_Tick;
            _remindTimer.Start();
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

        private void RemindTimer_Tick(object sender, EventArgs e)
        {
            ShowNearlyReminder();
        }
        private void ShutickyWindowMinimizeButtonClicked(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;

            senderWindow.SetDisplayStatus(DisplayStatus.Minimize);
        }
        private void ShutickyWindowNewButtonClicked(object sender, EventArgs e)
        {
            var newRtfPath = Path.Combine(_shutickyNoteApplicationFolderPath, $"{GenerateNewTitle(_defaultShutickyName)}.rtf");
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

            //セッティングリストを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);


            //デリートボタンが押されてWindowが閉じられた場合は保存は行わない。
            if (!_isShutickyDeleting)
            {
                senderWindow.SaveRTF();
            }
            //付箋削除中フラグをfalseに。
            _isShutickyDeleting = false;
        }

        private bool _isShutickyDeleting = false;
        private void ShutickyWindowDeleteButtonClicked(object sender, EventArgs e)
        {
            //デリート中フラグを立てる。
            //削除された後にShutickyWindowがDeactivatedの時に保存されないようにするため。
            _isShutickyDeleting = true;

            try
            {
                var senderWindow = sender as ShutickyWindow;
                //最終状態を保存
                senderWindow.SaveRTF();

                var senderSetting = senderWindow.GetShutickySetting();

                //Ctrlを押しながらクリックされた場合、直接Windowsのゴミ箱に送る
                if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
                {
                    //OSのゴミ箱へ移動。
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(senderSetting.FilePath,
                                                                       Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                                                       Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin
                                                                       );
                }
                else
                {
                    var trashedFileName = Path.GetFileName(senderSetting.FilePath);
                    var trashedFilePath = Path.Combine(_shutickyTrashcanFolderPath, trashedFileName);
                    var trashedTitle = Path.GetFileNameWithoutExtension(senderSetting.FilePath);

                    //RTFファイルをアプリのTrashフォルダに移動する。
                    //ゴミ箱ではない。
                    //ファイルの存在確認を行う。
                    if (File.Exists(trashedFilePath))
                    {
                        //アプリのゴミ箱内に同名のファイルがある場合はファイル名を変える必要がある。
                        var newTrashedTitle = GenerateNewTrashedTitle(trashedTitle);// "";

                        trashedFileName = $"{newTrashedTitle}.rtf";
                        trashedFilePath = Path.Combine(_shutickyTrashcanFolderPath, trashedFileName);
                        trashedTitle = Path.GetFileNameWithoutExtension(trashedFilePath);
                    }

                    File.Move(senderSetting.FilePath, trashedFilePath);//Trashフォルダに移動する

                    AddContextMenuItem_Trash(trashedTitle);
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
            catch
            {
                _isShutickyDeleting = false;
            }

        }


        private void ShutickyWindowSaveButtonClicked(object sender, EventArgs e)
        {
            var senderWindow = sender as ShutickyWindow;
            var senderSetting = senderWindow.GetShutickySetting();

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
            //付箋WindowのTitleを変更。
            senderWindow.Title = senderWindow.textBox_Title.Text;

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

            //セッティングリストを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);

            //RTFを保存
            senderWindow.SaveRTF();

            senderWindow.Close();
        }

        private void ContextMenuItem_New_Clicked(object sender, EventArgs e)
        {
            var newRtfPath = Path.Combine(_shutickyNoteApplicationFolderPath, $"{GenerateNewTitle(_defaultShutickyName)}.rtf");
            var newShutickySetting = new ShutickySetting(newRtfPath);

            AddNewShutickyWindow(newShutickySetting);
        }
        /// <summary>
        /// 既存の付箋Windowの座標と重ならない座標を返す
        /// </summary>
        /// <param name="baseX"></param>
        /// <param name="baseY"></param>
        /// <returns></returns>
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
                        if (newX + _defaultWidth + 50 > SystemParameters.VirtualScreenWidth)//マルチモニター全体の幅
                        {
                            newX = _defaultPositionX;
                        }
                        if (newY + _defaultHeight + 50 > SystemParameters.VirtualScreenHeight)//マルチモニター全体の高さ。
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
        /// <summary>
        /// 付箋が画面外にある場合は現在のウィンドウ内に収まるような座標を返す
        /// </summary>
        /// <param name="baseX"></param>
        /// <param name="baseY"></param>
        /// <returns></returns>
        private (double x, double y) AdjustWindowPositionTuple(double baseX, double baseY)
        {
            double newX = baseX;
            double newY = baseY;

            //ウィンドウの座標が画面外の場合がある。
            //マルチウィンドウで終了し、シングルウィンドウで起動したときなど。
            //この場合は、座標を修正する。
            if (baseX + 100 > SystemParameters.VirtualScreenWidth)
            {
                newX = baseX % SystemParameters.VirtualScreenWidth;
            }
            if (baseY + 100 > SystemParameters.VirtualScreenHeight)
            {
                newY = baseY % SystemParameters.VirtualScreenHeight;
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
        private void ContextMenuItem_Help_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_helpFilePath);
        }
        private void ContextMenuItem_TrashedRtf_Clicked(object sender, EventArgs e)
        {
            var senderContextMenu = (System.Windows.Forms.ToolStripMenuItem)sender;
            var trashedRtfFileName = $"{senderContextMenu.Text}.rtf";
            var trashedRtfFilePath = Path.Combine(_shutickyTrashcanFolderPath, trashedRtfFileName);
            var rtfFileName = $"{GenerateNewTitle(senderContextMenu.Text)}.rtf";
            var rtfFilePath = Path.Combine(_shutickyNoteApplicationFolderPath, rtfFileName);

            File.Move(trashedRtfFilePath, rtfFilePath);//アプリのゴミ箱からRTFを戻す。
            //そして開く。
            var shutickySetting = new ShutickySetting(rtfFilePath);
            AddNewShutickyWindow(shutickySetting);

            //コンテキストメニューから削除する。
            _notifyIcon.toolStripMenuItem_Trash.DropDownItems.Remove(senderContextMenu);
        }
        /// <summary>
        /// アプリのゴミ箱内のファイルをすべて削除する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuItem_ClearTrash_Clicked(object sender, EventArgs e)
        {

            //アプリのゴミ箱内のファイルのパスを取得
            var trashedFilePaths = Directory.GetFiles(_shutickyTrashcanFolderPath, "*.rtf", SearchOption.TopDirectoryOnly).ToList();

            if (trashedFilePaths.Count == 0)
            {
                return;
            }

            var result = MessageBox.Show("ShutickyNoteのゴミ箱内の付箋を削除します。\r\nよろしいですか?\r\n(RTFファイルはWindowsのゴミ箱に移動されます。)",
                                "ゴミ箱を空にする",
                                MessageBoxButton.OKCancel,
                                MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                foreach (var trashedFilePath in trashedFilePaths)
                {
                    try
                    {
                        //OSのゴミ箱へ移動。
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(trashedFilePath,
                                                                           Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                                                           Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin
                                                                           );
                    }
                    catch
                    {
                        continue;
                    }
                }

                //コンテキストメニューのゴミ箱を空にする。
                _notifyIcon.toolStripMenuItem_Trash.DropDownItems.Clear();
            }

        }
        #endregion



        private void ShowAllReminder()
        {
            //リマインダデータを取得し、それを時間順に並べる
            List<ReminderData> allReminders = new List<ReminderData>();
            foreach (var window in _shutickyWindows)
            {
                allReminders.AddRange(window.Reminders);
            }

            var notExpiredReminders = allReminders.Where(remind => DateTime.Now <= remind.DateAndTime)
                                              .OrderBy(reminder => reminder.DateAndTime)
                                              .ThenBy(reminder => reminder.Title);
            var expiredReminders = allReminders.Where(remind => remind.DateAndTime < DateTime.Now)
                                               .OrderBy(reminder => reminder.DateAndTime)
                                               .ThenBy(reminder => reminder.Title);

            var remindContents = "今後の予定\r\n" + string.Join("\r\n", notExpiredReminders.Select(reminder => $"{reminder.DateAndTime.ToString("yyyy年MM月dd日HH時mm分")}：{reminder.Content}")) + "\r\n";
            remindContents += "期限切れ\r\n" + string.Join("\r\n", expiredReminders.Select(reminder => $"{reminder.DateAndTime.ToString("yyyy年MM月dd日HH時mm分")}：{reminder.Content}"));
            MessageBox.Show(remindContents, "予定一覧", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void ShowNearlyReminder()
        {
            //リマインダデータを取得し、それを時間順に並べる
            List<ReminderData> allReminders = new List<ReminderData>();
            foreach (var window in _shutickyWindows)
            {
                allReminders.AddRange(window.Reminders);
            }

            var nearlyReminders = allReminders.Where(remind => remind.DateAndTime - remind.RemindBefore <= DateTime.Now && DateTime.Now <= remind.DateAndTime)
                                              .OrderBy(reminder => reminder.DateAndTime)
                                              .ThenBy(reminder => reminder.Title);
            var expiredReminders = allReminders.Where(remind => remind.DateAndTime < DateTime.Now)
                                               .OrderBy(reminder => reminder.DateAndTime)
                                               .ThenBy(reminder => reminder.Title);

            var remindContents = "もうすぐです\r\n" + string.Join("\r\n", nearlyReminders.Select(reminder => $"{reminder.DateAndTime.ToString("yyyy年MM月dd日HH時mm分")}：{reminder.Content}")) + "\r\n";
            remindContents += "期限切れ\r\n" + string.Join("\r\n", expiredReminders.Select(reminder => $"{reminder.DateAndTime.ToString("yyyy年MM月dd日HH時mm分")}：{reminder.Content}"));
            MessageBox.Show(remindContents, "予定一覧", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        }

        private void AddContextMenuItem_Trash(string trashedRtfFileNameWithoutExtension)
        {
            var toolstripMenuItem_trashedRtf = new System.Windows.Forms.ToolStripMenuItem(trashedRtfFileNameWithoutExtension);
            toolstripMenuItem_trashedRtf.Click += ContextMenuItem_TrashedRtf_Clicked;
            _notifyIcon.toolStripMenuItem_Trash.DropDownItems.Add(toolstripMenuItem_trashedRtf);
        }
        private string GenerateNewTitle(string defaultTitle)
        {
            var newShutickyTitle = defaultTitle;

            try
            {
                //デフォルト名のファイルが既に存在する場合は、
                //新しいファイル名を生成する。
                //if (_shutickyWindows.FindIndex(x => x.GetShutickySetting().Title == _defaultShutickyName) > -1)
                //{
                //既にある付箋名と被らないようにする
                int namePostfix = 1;
                while (true)
                {
                    if (_shutickyWindows.FindIndex(x => x.GetShutickySetting().Title == $"{defaultTitle}_{namePostfix}") > -1)
                    {
                        namePostfix++;
                    }
                    else
                    {
                        newShutickyTitle = $"{defaultTitle}_{namePostfix}";

                        break;
                    }
                }
                //}
            }
            catch
            {
                newShutickyTitle = _defaultShutickyName;
            }

            return newShutickyTitle;
        }
        private string GenerateNewTrashedTitle(string trashedTitle)
        {
            string newTrashedTitle = "";
            var trashedShutickyTitles = Directory.GetFiles(_shutickyTrashcanFolderPath, "*.rtf")
                                                    .Select(file => Path.GetFileNameWithoutExtension(file))
                                                    .ToList();

            try
            {
                int namePostfix = 1;
                while (true)
                {
                    if (trashedShutickyTitles.FindIndex(title => title == $"{trashedTitle}_{namePostfix}") > -1)
                    {
                        namePostfix++;
                    }
                    else
                    {
                        newTrashedTitle = $"{trashedTitle}_{namePostfix}";

                        break;
                    }
                }
            }
            catch
            {
                newTrashedTitle = $"{trashedTitle}_{DateTime.Now.ToString("yyyyMMddhhmmss")}";
            }

            return newTrashedTitle;
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
            //イベントハンドラをセット
            AddEventHandlersToShutickyWindow(newShutickyWindow);
            //付箋ウィンドウのリストに登録
            _shutickyWindows.Add(newShutickyWindow);

            //セッティングファイルを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);

            //RTFを作成。
            newShutickyWindow.SaveRTF();
        }
        private void AddExistShutickyWindow(ShutickySetting shutickySetting)
        {
            if (shutickySetting == null)
            {
                return;
            }

            //付箋が画面外にある場合は現在のウィンドウ内に収まるように調整
            var newPos = AdjustWindowPositionTuple(shutickySetting.Position_X, shutickySetting.Position_Y);
            shutickySetting.Position_X = newPos.x;
            shutickySetting.Position_Y = newPos.y;

            //付箋ウィンドウをインスタンス化
            var shutickyWindow = new ShutickyWindow(shutickySetting);
            //イベントハンドラを登録
            AddEventHandlersToShutickyWindow(shutickyWindow);
            //付箋ウィンドウのリストに登録
            _shutickyWindows.Add(shutickyWindow);

            //セッティングファイルを書き込み
            WriteShutickySettingListXML(_shutickySettingFilePath);
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
            XmlSerializer serializer = null;
            //読み込むファイルを開く
            StreamReader strmReader = null;
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

            List<ShutickySetting> shutickySettings = null;
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
                if (strmWriter != null)
                    strmWriter.Dispose();

                return;
            }
        }

    }
}
