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

        public static readonly List<BackGroundColorSet> PresetBackGroundColorSet = new List<BackGroundColorSet>()
        {
            new BackGroundColorSet{TitleColor="#FFFFDF67",BodyColor="#FFFDF5D6"},
            new BackGroundColorSet{TitleColor="#FFFCA8A8",BodyColor="#FFFDD6D6"},
            new BackGroundColorSet{TitleColor="#FFAFE780",BodyColor="#FFCFFBAA"},
            new BackGroundColorSet{TitleColor="#FF9BD8FA",BodyColor="#FFD6EFFD"},
            new BackGroundColorSet{TitleColor="#FFC1BAF7",BodyColor="#FFDAD6FD"},
        };


        public ShutickyWindow(ShutickySetting shutickySetting)
        {
            InitializeComponent();

            _shutickySetting = shutickySetting;

            Load(_shutickySetting.FilePath);

            this.textBox_Title.Text = _shutickySetting.Title;
            this.Height = _shutickySetting.Size_Height;
            this.Width = _shutickySetting.Size_Width;
            this.Top = _shutickySetting.Position_Y;
            this.Left = _shutickySetting.Position_X;

            SetShutickyColor(_shutickySetting.ColorNumber);

            this.Visibility = Visibility.Visible;
        }



        /// <summary>
        /// 付箋データの読み込み（rtf）
        /// </summary>
        private void Load(string filePath)
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

        private void label_TitleCover_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            label_TitleCover.Visibility = Visibility.Collapsed;
            if (textBox_Title.IsFocused == false)
            {
                textBox_Title.IsReadOnly = false;
                textBox_Title.Focus();
                e.Handled = true;
            }
        }



        private void textBox_Title_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textBox_Title.IsReadOnly = false;
        }

        public event EventHandler TitleLostFocus;
        private void textBox_Title_LostFocus(object sender, RoutedEventArgs e)
        {
            textBox_Title.IsReadOnly = true;
            label_TitleCover.Visibility = Visibility.Visible;

            if (TitleLostFocus != null)
            {
                TitleLostFocus(this, EventArgs.Empty);
            }
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


        public event EventHandler NewShutickyButtonClicked;
        private void button_NewShuticky_Click(object sender, RoutedEventArgs e)
        {
            if (NewShutickyButtonClicked != null)
            {
                NewShutickyButtonClicked(this, EventArgs.Empty);
            }
        }

        private void button_Configuration_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingGrid();
        }


        public event EventHandler DeleteButtonClicked;
        private void button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteButtonClicked != null)
            {
                DeleteButtonClicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler SaveButtonClicked;
        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (SaveButtonClicked != null)
            {
                SaveButtonClicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler CloseButtonClicked;
        private void button_Close_Click(object sender, RoutedEventArgs e)
        {
            if (CloseButtonClicked != null)
            {
                CloseButtonClicked(this, EventArgs.Empty);
            }
        }


        /// <summary>
        /// 内部付箋情報リストを更新
        /// </summary>
        public ShutickySetting GetShutickySetting()
        {
            _shutickySetting.Title = textBox_Title.Text;
            _shutickySetting.Position_X = this.Left;
            _shutickySetting.Position_Y = this.Top;
            _shutickySetting.Size_Height = this.Height;
            _shutickySetting.Size_Width = this.Width;

            return _shutickySetting;
        }

        /// <summary>
        /// 付箋ウィンドウが閉じるときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.Close();
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
            richTextBox_Body.Background = new SolidColorBrush(ColorConverter.GetArbgColor(bodyColorCode, 0));
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
        private void richTextBox_Body_GotFocus(object sender, RoutedEventArgs e)
        {
            HideSettingGrid();
        }

        /// <summary>
        /// 
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
            //文字の背景色を変更
            if ((Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) && (e.Key == Key.Q))
            {
                ChangeTextBackgroundColor(Key.Q);

                e.Handled = true;
                return;
            }
            //文字の背景色を変更
            if ((Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) && (e.Key == Key.W))
            {
                ChangeTextBackgroundColor(Key.W);

                e.Handled = true;
                return;
            }
        }


        /// <summary>
        /// Ctrl+マウスホイールによる拡大、縮小。
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

        /// <summary>
        /// ウィンドウ最小化ボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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
        /// 文字色を変更
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
        /// 文字の背景色を変更。
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


    }
}
