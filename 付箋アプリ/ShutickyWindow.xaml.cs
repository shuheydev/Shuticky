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
using System.IO;


namespace 付箋アプリ
{
    /// <summary>
    /// 付箋Window.xaml の相互作用ロジック
    /// </summary>
    public partial class ShutickyWindow : Window
    {
        private ShutickySetting _shutickySetting;

        private bool _deleted = false;

        public static readonly List<BackGroundColorSet> PresetBackGroundColorSet = new List<BackGroundColorSet>()
        {
            new BackGroundColorSet{TitleColor="#FFFFDF67",BodyColor="#FFFDF5D6"},
            new BackGroundColorSet{TitleColor="#FFFCA8A8",BodyColor="#FFFDD6D6"},
            new BackGroundColorSet{TitleColor="#FFAFE780",BodyColor="#FFCFFBAA"},
            new BackGroundColorSet{TitleColor="#FF9BD8FA",BodyColor="#FFD6EFFD"},
            new BackGroundColorSet{TitleColor="#FFC1BAF7",BodyColor="#FFDAD6FD"},
        };

        private double _minimumFontSize = 12;
        private double _maximumFontSize = 20;

        public ShutickyWindow(ShutickySetting shutickySetting)
        {
            InitializeComponent();

            _shutickySetting = shutickySetting;

            LoadRTF(_shutickySetting.FilePath);

            this.textBox_Title.Text = _shutickySetting.Title;
            this.Title = this.textBox_Title.Text;//こちらはWindowコントロールのタイトル。
            this.Height = _shutickySetting.Size_Height;
            this.Width = _shutickySetting.Size_Width;
            this.Top = _shutickySetting.Position_Y;
            this.Left = _shutickySetting.Position_X;

            SetShutickyColor(_shutickySetting.ColorNumber);

            SetDisplayStatus(_shutickySetting.DisplayStatus);

        }

        /// <summary>
        /// 付箋データの読み込み（rtf）
        /// </summary>
        private void LoadRTF(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            textBox_Title.Text = Path.GetFileNameWithoutExtension(filePath);

            TextRange range_Body;
            range_Body = new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd);

            using (FileStream fStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                range_Body.Load(fStream, DataFormats.Rtf);
            }
        }
        /// <summary>
        /// 付箋データの書き込み(rtf)
        /// </summary>
        public void SaveRTF()
        {
            //Deleteボタンが押されてShutickyウィンドウが閉じられた時に、
            //RTFが保存されないようにする。
            if (this._deleted == true)
            {
                return;
            }

            TextRange range_Body = new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd);

            using (FileStream fStream = new FileStream(_shutickySetting.FilePath, FileMode.Create))
            {
                range_Body.Save(fStream, DataFormats.Rtf);
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Label_TitleCover_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            label_TitleCover.Visibility = Visibility.Collapsed;
            if (textBox_Title.IsFocused == false)
            {
                textBox_Title.IsReadOnly = false;
                textBox_Title.Focus();
                e.Handled = true;
            }
        }

        private void TextBox_Title_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textBox_Title.IsReadOnly = false;
        }

        public event EventHandler TitleLostFocus;
        private void TextBox_Title_LostFocus(object sender, RoutedEventArgs e)
        {
            textBox_Title.IsReadOnly = true;
            label_TitleCover.Visibility = Visibility.Visible;

            TitleLostFocus?.Invoke(this, EventArgs.Empty);
        }

        private void TextBox_Title_GotFocus(object sender, RoutedEventArgs e)
        {
            textBox_Title.SelectAll();
        }

        private void TextBox_Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void TextBox_Title_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBox_Title.IsReadOnly = true;
                label_TitleCover.Visibility = Visibility.Visible;

                this.Title = textBox_Title.Text;

                richTextBox_Body.Focus();
            }
        }


        public event EventHandler NewShutickyButtonClicked;
        private void Button_NewShuticky_Click(object sender, RoutedEventArgs e)
        {
            NewShutickyButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void Button_Configuration_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingGrid();
        }


        public event EventHandler DeleteButtonClicked;
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            //削除フラグをOnにする。
            //削除ボタンを押して閉じられる時に、
            //Deactivatedイベントで保存が行われないように。
            this._deleted = true;

            DeleteButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SaveButtonClicked;
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CloseButtonClicked;
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            CloseButtonClicked?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// 内部付箋情報リストを更新
        /// </summary>
        public ShutickySetting GetShutickySetting()
        {
            //状態を更新してから
            _shutickySetting.Position_X = this.Left;
            _shutickySetting.Position_Y = this.Top;
            //ただし幅と高さは、最小化されている場合は更新しない
            if (_shutickySetting.DisplayStatus != DisplayStatus.Minimize)
            {
                _shutickySetting.Size_Height = this.Height;
                _shutickySetting.Size_Width = this.Width;
            }

            return _shutickySetting;
        }
        public void SetShutickySetting(ShutickySetting shutickySetting)
        {
            if (shutickySetting == null)
            {
                return;
            }

            _shutickySetting = shutickySetting;

            this.Left = _shutickySetting.Position_X;
            this.Top = _shutickySetting.Position_Y;
            this.Width = _shutickySetting.Size_Width;
            this.Height = _shutickySetting.Size_Height;
        }

        /// <summary>
        /// 付箋ウィンドウが閉じるときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e != null)
            {
                e.Cancel = true;
            }

            SetDisplayStatus(DisplayStatus.Hidden);
        }

        /// <summary>
        /// 付箋の背景色が選択されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Color_Click(object sender, RoutedEventArgs e)
        {
            Button clickedColorButton = (Button)sender;

            int colorNumber = 0;
            switch (clickedColorButton.Name)
            {
                case "button_Color1":
                    {
                        colorNumber = 0;
                        break;
                    }
                case "button_Color2":
                    {
                        colorNumber = 1;

                        break;
                    }
                case "button_Color3":
                    {
                        colorNumber = 2;

                        break;
                    }
                case "button_Color4":
                    {
                        colorNumber = 3;

                        break;
                    }
                case "button_Color5":
                    {
                        colorNumber = 4;

                        break;
                    }
            }

            SetShutickyColor(colorNumber);
        }


        public void SetShutickyColor(int colorNumber)
        {
            _shutickySetting.ColorNumber = colorNumber;

            string titleColorCode = PresetBackGroundColorSet[_shutickySetting.ColorNumber].TitleColor;
            string bodyColorCode = PresetBackGroundColorSet[_shutickySetting.ColorNumber].BodyColor;
            dockPanel_TitleBar.Background = new SolidColorBrush(ColorConverter.GetArbgColor(titleColorCode, 0));
            dockPanel_Body.Background = new SolidColorBrush(ColorConverter.GetArbgColor(bodyColorCode, 0));
        }


        /// <summary>
        /// 付箋ウィンドウが非アクティブになったとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            HideSettingGrid();
        }

        /// <summary>
        /// 付箋のリッチテキストコントロールがフォーカスを得たとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBox_Body_GotFocus(object sender, RoutedEventArgs e)
        {
            HideSettingGrid();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBox_Body_KeyDown(object sender, KeyEventArgs e)
        {
            //取り消し線
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                StrikeThrough();

                e.Handled = true;
                return;
            }

            //文字の色を赤に
            if (e.Key == Key.W && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ChangeTextForegroundColor();

                e.Handled = true;
                return;
            }

            //文字の背景色を変更
            if (e.Key == Key.Q && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ChangeTextBackgroundColor();

                e.Handled = true;
                return;
            }
        }


        /// <summary>
        /// Ctrl+マウスホイールによる拡大、縮小。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBox_Body_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool handle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            if (!handle)
            {
                return;
            }

            //var fontSize = 0.0;

            TextRange range_Body = new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd);
            double.TryParse(range_Body.GetPropertyValue(TextElement.FontSizeProperty).ToString(), out double fontSize);

            fontSize = fontSize + (e.Delta / 100);

            if (fontSize < _minimumFontSize)
            {
                fontSize = _minimumFontSize;
            }
            if (fontSize > _maximumFontSize)
            {
                fontSize = _maximumFontSize;
            }

            range_Body.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize.ToString());

        }

        public event EventHandler MinimizeButtonClicked;
        /// <summary>
        /// ウィンドウ最小化ボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            MinimizeButtonClicked?.Invoke(this, EventArgs.Empty);
        }




        /// <summary>
        /// セッティング領域を表示させる。
        /// </summary>
        private void ShowSettingGrid()
        {
            var anim = new System.Windows.Media.Animation.DoubleAnimation(70, (Duration)TimeSpan.FromSeconds(0.1));
            grid_Setting.BeginAnimation(ContentControl.HeightProperty, anim);

            grid_Setting.IsEnabled = true;
        }
        /// <summary>
        /// セッティング領域を隠す。
        /// </summary>
        private void HideSettingGrid()
        {
            var anim = new System.Windows.Media.Animation.DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.1));
            grid_Setting.BeginAnimation(ContentControl.HeightProperty, anim);

            grid_Setting.IsEnabled = false;
        }

        /// <summary>
        /// 文字に消し線をつける
        /// </summary>
        private void StrikeThrough()
        {
            var range_Selected = new TextRange(richTextBox_Body.Selection.Start, richTextBox_Body.Selection.End);
            var currentTextDecoration = range_Selected.GetPropertyValue(Inline.TextDecorationsProperty);
            
            if (currentTextDecoration == DependencyProperty.UnsetValue)
            {
                range_Selected.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                return;
            }

            if (currentTextDecoration.Equals(TextDecorations.Strikethrough))
            {
                range_Selected.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            }
            else
            {
                range_Selected.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            }
        }

        /// <summary>
        /// 文字色を変更
        /// </summary>
        /// <param name="_key"></param>
        private void ChangeTextForegroundColor()
        {
            TextRange range_Selected = new TextRange(richTextBox_Body.Selection.Start, richTextBox_Body.Selection.End);

            var foregroundProperty = range_Selected.GetPropertyValue(TextElement.ForegroundProperty);
            if (foregroundProperty.Equals(DependencyProperty.UnsetValue))//選択範囲に複数の色が存在するとき
            {
                range_Selected.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);//黒にする
            }
            else if (foregroundProperty.ToString() == "#FF000000")//黒の時
            {
                range_Selected.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);//赤にする
            }
            else if (foregroundProperty.ToString() == "#FFFF0000")//赤の時
            {
                range_Selected.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);//黒にする
            }
        }

        /// <summary>
        /// 文字の背景色を変更。
        /// </summary>
        /// <param name="_key"></param>
        private void ChangeTextBackgroundColor()
        {
            TextRange range_Selected = new TextRange(richTextBox_Body.Selection.Start, richTextBox_Body.Selection.End);

            var backgroundProperty = range_Selected.GetPropertyValue(TextElement.BackgroundProperty);

            if (backgroundProperty == null)//無色の時は
            {
                range_Selected.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);//黄色に
                return;
            }

            if (backgroundProperty.Equals(DependencyProperty.UnsetValue))//無色と黄色の両方が存在するとき
            {
                range_Selected.ApplyPropertyValue(TextElement.BackgroundProperty, null);//無色に
            }
            else if (backgroundProperty.ToString() == "#FFFFFF00")//黄色の時は
            {
                range_Selected.ApplyPropertyValue(TextElement.BackgroundProperty, null);//無色に
            }
        }


        public void SetDisplayStatus(DisplayStatus displayStatus)
        {
            _shutickySetting.DisplayStatus = displayStatus;

            //DisplayStatusに応じて
            switch (_shutickySetting.DisplayStatus)
            {
                case DisplayStatus.Visible:
                    {
                        this.WindowState = WindowState.Normal;
                        this.Visibility = Visibility.Visible;
                        this.Activate();
                        break;
                    }
                case DisplayStatus.Minimize:
                    {
                        //this.Visibility = Visibility.Visible;
                        this.WindowState = WindowState.Minimized;
                        break;
                    }
                case DisplayStatus.Hidden:
                    {
                        this.Visibility = Visibility.Hidden;
                        break;
                    }
            }
        }

        public void SetDisplayPosition(double x,double y)
        {
            this.Top = y;
            this.Left = x;
        }

        private void MenuItem_StrikeThrough_Click(object sender, RoutedEventArgs e)
        {
            StrikeThrough();
        }

        private void MenuItem_ChangeTextForegroundColor_Click(object sender, RoutedEventArgs e)
        {
            ChangeTextForegroundColor();
        }

        private void MenuItem_ChangeTextBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            ChangeTextBackgroundColor();
        }

        private void MenuItem_WindowTopMost_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
