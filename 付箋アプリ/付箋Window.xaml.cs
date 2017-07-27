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
using System.Windows.Shapes;

namespace 付箋アプリ
{
    /// <summary>
    /// 各付箋を表すクラス
    /// </summary>
    public partial class 付箋Window : Window
    {
        #region メンバ変数
        public MainWindow G_MainWindow;//メインウィンドウへのアクセス用

        //こりゃなんだ？
        private StickyNote StkyNote = new StickyNote();

        //閉じる処理中であることを示すフラグ
        private bool ClosingFlag = false;

        //付箋カラーの初期化。タイトル用の濃い色と編集領域用の薄い色をセットにして登録
        public List<BackGroundColorSet> PresetBackGroundColorSet = new List<BackGroundColorSet>()
                                                                    {
                                                                        new BackGroundColorSet{TitleColor="#FFFFDF67",NoteColor="#FFFDF5D6"},
                                                                        new BackGroundColorSet{TitleColor="#FFFCA8A8",NoteColor="#FFFDD6D6"},
                                                                        new BackGroundColorSet{TitleColor="#FFAFE780",NoteColor="#FFCFFBAA"},
                                                                        new BackGroundColorSet{TitleColor="#FF9BD8FA",NoteColor="#FFD6EFFD"},
                                                                        new BackGroundColorSet{TitleColor="#FFC1BAF7",NoteColor="#FFDAD6FD"}
                                                                    };
        #endregion









        /// <summary>
        /// 付箋ウィンドウのコンストラクタ.
        /// 空の付箋用。
        /// 新規作成などで。
        /// </summary>
        public 付箋Window()
        {
            InitializeComponent();

            textBox_Title.Text = StkyNote.Title;
            this.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 付箋ウィンドウのコンストラクタ
        /// 既存の付箋用。
        /// 付箋オブジェクトを受け取って、それを表示する。
        /// </summary>
        /// <param name="_stkyNote">付箋オブジェクト</param>
        public 付箋Window(StickyNote _stkyNote)
        {
            InitializeComponent();

            StkyNote = _stkyNote;

            LoadStickyNote();

            this.Title = StkyNote.Title;
            this.Height = StkyNote.Size_Height;
            this.Width = StkyNote.Size_Width;
            this.Top = StkyNote.Position_Y;
            this.Left = StkyNote.Position_X;

            dockPanel_TitleBar.Background = new SolidColorBrush(GetArbgColor(PresetBackGroundColorSet[StkyNote.ColorNumber].TitleColor, 0));
            richTextBox_Body.Background = new SolidColorBrush(GetArbgColor(PresetBackGroundColorSet[StkyNote.ColorNumber].NoteColor, 0));

            this.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// ARGB16進カラーcodeをColorに変換する
        /// </summary>
        /// <param name="colorCode">#00000000</param>
        /// <returns></returns>
        public static Color GetArbgColor(string colorCode, int offset)
        {
            try
            {
                // #で始まっているか
                var index = colorCode.IndexOf("#", StringComparison.Ordinal);
                // 文字数の確認と#がおかしな位置にいないか
                if (colorCode.Length != 9 || index != 0)
                {
                    // 例外を投げる
                    throw new ArgumentOutOfRangeException();
                }

                // 分解する
                var alpha = Convert.ToByte(Convert.ToInt32(colorCode.Substring(1, 2), 16));
                var red = Convert.ToByte(Convert.ToInt32(colorCode.Substring(3, 2), 16));
                var green = Convert.ToByte(Convert.ToInt32(colorCode.Substring(5, 2), 16) + offset);
                var blue = Convert.ToByte(Convert.ToInt32(colorCode.Substring(7, 2), 16) - offset);

                return Color.FromArgb(alpha, red, green, blue);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("GetArbgColor : colorCode OutOfRange");
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentOutOfRangeException("GetArbgColor : \"#\" not found");
            }
            catch (AggregateException)
            {
                throw new ArgumentOutOfRangeException("GetArbgColor : \"#\" not found");
            }
        }

        /// <summary>
        /// リッチテキストファイルを、
        /// リッチテキストコントロールにリッチテキストとして読み込む。
        /// </summary>
        public void LoadStickyNote()
        {

            if (System.IO.File.Exists(this.StkyNote.FilePath) == true)
            {
                this.textBox_Title.Text = this.StkyNote.Title;


                TextRange range_Body;
                range_Body = new TextRange(this.richTextBox_Body.Document.ContentStart, this.richTextBox_Body.Document.ContentEnd);

                using (System.IO.FileStream fStream = new System.IO.FileStream(StkyNote.FilePath, System.IO.FileMode.OpenOrCreate))
                {
                    range_Body.Load(fStream, DataFormats.Rtf);
                }

            }
        }

        /// <summary>
        /// 付箋を保存する。
        /// </summary>
        public void SaveStickyNote()
        {
            if (string.IsNullOrEmpty(this.StkyNote.FilePath) == true)
            {
                this.StkyNote.FilePath = G_MainWindow.StickyNoteApplicationFolderPath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + this.textBox_Title.Text + ".rtf";
            }

            this.StkyNote.Title = this.textBox_Title.Text;

            TextRange range_Body = new TextRange(this.richTextBox_Body.Document.ContentStart, this.richTextBox_Body.Document.ContentEnd);
            this.StkyNote.Body = range_Body.Text;

            using (System.IO.FileStream fStream = new System.IO.FileStream(this.StkyNote.FilePath, System.IO.FileMode.Create))
            {
                range_Body.Save(fStream, DataFormats.Rtf);
            }

        }


        /// <summary>
        /// 内部の付箋リストを更新する。
        /// </summary>
        private void UpdateStickyNoteList()
        {
            this.StkyNote.Title = this.textBox_Title.Text;
            this.StkyNote.Position_X = this.Left;
            this.StkyNote.Position_Y = this.Top;
            this.StkyNote.Size_Height = this.Height;
            this.StkyNote.Size_Width = this.Width;
            this.StkyNote.ColorCode = this.richTextBox_Body.Background.ToString();


            int stickyNoteListIdx = -1;
            stickyNoteListIdx = G_MainWindow.StickyNoteList.FindIndex(x => x.FilePath == StkyNote.FilePath);

            if (stickyNoteListIdx > -1)
            {
                G_MainWindow.StickyNoteList[stickyNoteListIdx] = StkyNote;
            }
            else
            {
                G_MainWindow.StickyNoteList.Add(StkyNote);
            }
        }



        /// <summary>
        /// タイトルバーをドラッグすることで、ウィンドウを移動させるため
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            {

            }
        }





        /// <summary>
        /// 付箋のタイトル部分がダブルクリックされたとき、
        /// タイトルを編集可能にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label_TitleCover_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                label_TitleCover.Visibility = Visibility.Collapsed;
                if (textBox_Title.IsFocused == false)
                {
                    textBox_Title.IsReadOnly = false;
                    textBox_Title.Focus();
                    e.Handled = true;
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// 付箋のタイトル部分がダブルクリックされたとき、
        /// タイトルを編集可能にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_Title_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textBox_Title.IsReadOnly = false;
        }

        private void textBox_Title_LostFocus(object sender, RoutedEventArgs e)
        {
            textBox_Title.IsReadOnly = true;
            label_TitleCover.Visibility = Visibility.Visible;
        }

        private void textBox_Title_GotFocus(object sender, RoutedEventArgs e)
        {
            textBox_Title.SelectAll();
        }

        private void textBox_Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void textBox_Title_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBox_Title.IsReadOnly = true;
                label_TitleCover.Visibility = Visibility.Visible;

                this.Title = textBox_Title.Text;

                richTextBox_Body.Focus();
            }
        }


        /// <summary>
        /// 付箋新規作成ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_NewSticky_Click(object sender, RoutedEventArgs e)
        {
            付箋Window newStickyNote = new 付箋Window();

            newStickyNote.G_MainWindow = G_MainWindow;
        }

        /// <summary>
        /// 設定ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Configuration_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingGrid();
        }


        /// <summary>
        /// 閉じるボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Delete_Click(object sender, RoutedEventArgs e)
        {
            ClosingFlag = true;

            //セーブしてから
            SaveStickyNote();

            //捨てる
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(StkyNote.FilePath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

            //内部のリストから消す
            int stkyNoteListIdx = -1;
            stkyNoteListIdx = G_MainWindow.StickyNoteList.FindIndex(x => x.FilePath == StkyNote.FilePath);
            if (stkyNoteListIdx > -1)
            {
                G_MainWindow.StickyNoteList.RemoveAt(stkyNoteListIdx);
            }

            this.Close();
        }
        
        /// <summary>
        /// 保存ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveStickyNote();
            UpdateStickyNoteList();
        }

        /// <summary>
        /// 閉じるボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Close_Click(object sender, RoutedEventArgs e)
        {
            ClosingFlag = true;

            //セーブしてから
            SaveStickyNote();
            UpdateStickyNoteList();

            this.Close();
        }

        /// <summary>
        /// 付箋ウィンドウを閉じる処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //内部の付箋情報リストをXMLに書き出す。
            G_MainWindow.WriteStickyNoteListXML(G_MainWindow.StickyNoteListFilePath);
        }

        /// <summary>
        /// 付箋カラーボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Color_Click(object sender, RoutedEventArgs e)
        {
            Button clickedColorButton = (Button)sender;

            string titleColor = "";
            string noteColor = "";

            //押されたボタンに応じて色を変更する。
            switch (clickedColorButton.Name)
            {
                case "button_Color1":
                    {
                        StkyNote.ColorNumber = 0;
                        break;
                    }
                case "button_Color2":
                    {
                        StkyNote.ColorNumber = 1;

                        break;
                    }
                case "button_Color3":
                    {
                        StkyNote.ColorNumber = 2;

                        break;
                    }
                case "button_Color4":
                    {
                        StkyNote.ColorNumber = 3;

                        break;
                    }
                case "button_Color5":
                    {
                        StkyNote.ColorNumber = 4;

                        break;
                    }
            }

            //付箋カラーのセット
            titleColor = PresetBackGroundColorSet[StkyNote.ColorNumber].TitleColor;
            noteColor = PresetBackGroundColorSet[StkyNote.ColorNumber].NoteColor;

            //付箋カラーの適用
            dockPanel_TitleBar.Background = new SolidColorBrush(GetArbgColor(titleColor, 0));
            richTextBox_Body.Background = new SolidColorBrush(GetArbgColor(noteColor, 0));
        }


        /// <summary>
        /// 付箋ウィンドウが非アクティブになったときの処理
        /// 保存処理を行う。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            HideSettingGrid();

            if (ClosingFlag == false)
            {
                SaveStickyNote();
                UpdateStickyNoteList();
            }
        }

        /// <summary>
        /// 付箋ウィンドウの編集領域が選択されたときの処理。
        /// 設定領域を隠す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox_Body_GotFocus(object sender, RoutedEventArgs e)
        {
            HideSettingGrid();
        }


        /// <summary>
        /// 設定領域を表示する。
        /// </summary>
        private void ShowSettingGrid()
        {
            //設定領域を操作可能にする
            grid_Setting.IsEnabled = true;

            //設定領域をアニメーションさせて表示する。
            var anim = new System.Windows.Media.Animation.DoubleAnimation(70, (Duration)TimeSpan.FromSeconds(0.1));
            grid_Setting.BeginAnimation(ContentControl.HeightProperty, anim);
        }

        /// <summary>
        /// 設定領域を隠す
        /// </summary>
        private void HideSettingGrid()
        {
            //設定領域を操作不可にする。
            grid_Setting.IsEnabled = false;

            //設定領域をアニメーションさせて隠す
            var anim = new System.Windows.Media.Animation.DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.1));
            grid_Setting.BeginAnimation(ContentControl.HeightProperty, anim);
        }

        /// <summary>
        /// 編集領域の選択部分に消し線をつける。
        /// </summary>
        private void StrikeThrough()
        {
            TextRange range_Selected = new TextRange(richTextBox_Body.Selection.Start, richTextBox_Body.Selection.End);
            var currentTextDecoration = range_Selected.GetPropertyValue(Inline.TextDecorationsProperty);

            TextDecorationCollection newTextDecoration;

            if (currentTextDecoration != DependencyProperty.UnsetValue)
            {
                newTextDecoration = ((TextDecorationCollection)currentTextDecoration == TextDecorations.Strikethrough) ? new TextDecorationCollection() : TextDecorations.Strikethrough;
            }
            else
            {
                newTextDecoration = TextDecorations.Strikethrough;
            }

            range_Selected.ApplyPropertyValue(Inline.TextDecorationsProperty, newTextDecoration);
        }

        /// <summary>
        /// 編集領域の文字色を赤⇔黒と切り替える
        /// </summary>
        /// <param name="_key"></param>
        private void ChangeTextForgroundColor(Key _key)
        {
            TextRange range_Selected = new TextRange(richTextBox_Body.Selection.Start, richTextBox_Body.Selection.End);

            switch (_key)
            {
                case Key.Q:
                    {
                        range_Selected.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);

                        break;
                    }
                case Key.W:
                    {
                        range_Selected.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);

                        break;
                    }
            }
        }


        /// <summary>
        /// 編集領域の背景色を黄色⇔なしと切り替える
        /// </summary>
        /// <param name="_key"></param>
        private void ChangeTextBackgroundColor(Key _key)
        {
            TextRange range_Selected = new TextRange(richTextBox_Body.Selection.Start, richTextBox_Body.Selection.End);

            switch (_key)
            {
                case Key.Q:
                    {
                        range_Selected.ApplyPropertyValue(TextElement.BackgroundProperty, null);

                        break;
                    }
                case Key.W:
                    {
                        range_Selected.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);

                        break;
                    }
            }
        }


        /// <summary>
        /// 消し線、文字色、背景色の切り替えショートカット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox_Body_KeyDown(object sender, KeyEventArgs e)
        {
            //取り消し線
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                StrikeThrough();

                e.Handled = true;
                return;
            }

            //文字の色を黒に
            if (e.Key == Key.Q && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ChangeTextForgroundColor(Key.Q);

                e.Handled = true;
                return;
            }
            //文字の色を赤に
            if (e.Key == Key.W && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ChangeTextForgroundColor(Key.W);

                e.Handled = true;
                return;
            }

            //背景色をなしに
            if ((Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) && (e.Key == Key.Q))
            {
                ChangeTextBackgroundColor(Key.Q);

                e.Handled = true;
                return;
            }
            //背景色を黄色に
            if ((Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) && (e.Key == Key.W))
            {
                ChangeTextBackgroundColor(Key.W);

                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// マウスホイールによる編集領域のフォントサイズの変更
        /// 最大２０、最小１２
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox_Body_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool handle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            if (!handle)
            {
                return;
            }

            double fontSize = 0.0;

            TextRange range_Body = new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd);
            double.TryParse(range_Body.GetPropertyValue(TextElement.FontSizeProperty).ToString(), out fontSize);

            fontSize = fontSize + (e.Delta / 100);

            if (fontSize < 12)
            {
                fontSize = 12;
            }
            if (fontSize > 20)
            {
                fontSize = 20;
            }

            range_Body.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize.ToString());

        }

    }
}
