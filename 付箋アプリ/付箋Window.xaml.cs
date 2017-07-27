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
    /// 付箋Window.xaml の相互作用ロジック
    /// </summary>
    public partial class 付箋Window : Window
    {
        private StickyNote StkyNote = new StickyNote();

        public MainWindow G_MainWindow;

        public List<BackGroundColorSet> PresetBackGroundColorSet = new List<BackGroundColorSet>()
        {
            new BackGroundColorSet{TitleColor="#FFFFDF67",NoteColor="#FFFDF5D6"},
            new BackGroundColorSet{TitleColor="#FFFCA8A8",NoteColor="#FFFDD6D6"},
            new BackGroundColorSet{TitleColor="#FFAFE780",NoteColor="#FFCFFBAA"},
            new BackGroundColorSet{TitleColor="#FF9BD8FA",NoteColor="#FFD6EFFD"},
            new BackGroundColorSet{TitleColor="#FFC1BAF7",NoteColor="#FFDAD6FD"}
        };

        private bool ClosingFlag = false;

        public 付箋Window()
        {
            InitializeComponent();

            textBox_Title.Text = StkyNote.Title;
            this.Visibility = Visibility.Visible;
        }
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
            //if (string.IsNullOrEmpty(StkyNote.ColorCode) == false)
            //{
                //richTextBox_Body.Background = new SolidColorBrush(GetArbgColor(StkyNote.ColorCode, 0));
                dockPanel_TitleBar.Background = new SolidColorBrush(GetArbgColor(PresetBackGroundColorSet[StkyNote.ColorNumber].TitleColor, 0));
                richTextBox_Body.Background = new SolidColorBrush(GetArbgColor(PresetBackGroundColorSet[StkyNote.ColorNumber].NoteColor, 0));
            //}

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

        public void LoadStickyNote()
        {

            if (System.IO.File.Exists(StkyNote.FilePath) == true)
            {
                textBox_Title.Text = StkyNote.Title;


                TextRange range_Body;
                range_Body = new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd);

                using (System.IO.FileStream fStream = new System.IO.FileStream(StkyNote.FilePath, System.IO.FileMode.OpenOrCreate))
                {
                    range_Body.Load(fStream, DataFormats.Rtf);
                }

                //range_Body.ApplyPropertyValue(TextElement.FontSizeProperty, "14");
            }
        }

        public void SaveStickyNote()
        {
            if (string.IsNullOrEmpty(StkyNote.FilePath) == true)
            {
                StkyNote.FilePath = G_MainWindow.StickyNoteApplicationFolderPath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + textBox_Title.Text + ".rtf";
            }

            StkyNote.Title = textBox_Title.Text;

            TextRange range_Body = new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd);
            StkyNote.Body = range_Body.Text;

            using (System.IO.FileStream fStream = new System.IO.FileStream(StkyNote.FilePath, System.IO.FileMode.Create))
            {
                range_Body.Save(fStream, DataFormats.Rtf);
            }

        }


        private void UpdateStickyNoteList()
        {
            StkyNote.Title = textBox_Title.Text;
            StkyNote.Position_X = this.Left;
            StkyNote.Position_Y = this.Top;
            StkyNote.Size_Height = this.Height;
            StkyNote.Size_Width = this.Width;
            StkyNote.ColorCode = richTextBox_Body.Background.ToString();


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




        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
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



        private void button_NewSticky_Click(object sender, RoutedEventArgs e)
        {
            付箋Window newStickyNote = new 付箋Window();

            newStickyNote.G_MainWindow = G_MainWindow;
        }

        private void button_Configuration_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingGrid();
        }



        private void button_Delete_Click(object sender, RoutedEventArgs e)
        {
            ClosingFlag = true;

            //セーブしてから
            SaveStickyNote();

            //捨てる
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(StkyNote.FilePath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

            //リストから消す
            int stkyNoteListIdx = -1;
            stkyNoteListIdx = G_MainWindow.StickyNoteList.FindIndex(x => x.FilePath == StkyNote.FilePath);
            if (stkyNoteListIdx > -1)
            {
                G_MainWindow.StickyNoteList.RemoveAt(stkyNoteListIdx);
            }

            this.Close();
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveStickyNote();
            UpdateStickyNoteList();
        }

        private void button_Close_Click(object sender, RoutedEventArgs e)
        {
            ClosingFlag = true;

            //セーブしてから
            SaveStickyNote();
            UpdateStickyNoteList();

            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            G_MainWindow.WriteStickyNoteListXML(G_MainWindow.StickyNoteListFilePath);


            //int windowCount = Application.Current.Windows.Count;
            ////見えないMainWindowも含めて。
            //if (windowCount < 3)
            //{
            //    G_MainWindow.WriteStickyNoteListXML(G_MainWindow.StickyNoteListFilePath);
            //    Application.Current.Shutdown();
            //}
        }

        private void Button_Color_Click(object sender, RoutedEventArgs e)
        {
            Button clickedColorButton = (Button)sender;

            string titleColor = "";
            string noteColor = "";

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


            titleColor = PresetBackGroundColorSet[StkyNote.ColorNumber].TitleColor;
            noteColor = PresetBackGroundColorSet[StkyNote.ColorNumber].NoteColor;

            dockPanel_TitleBar.Background = new SolidColorBrush(GetArbgColor(titleColor, 0));
            richTextBox_Body.Background = new SolidColorBrush(GetArbgColor(noteColor, 0));
        }



        private void Window_Deactivated(object sender, EventArgs e)
        {
            HideSettingGrid();

            if (ClosingFlag == false)
            {
                SaveStickyNote();
                UpdateStickyNoteList();
            }
        }

        private void richTextBox_Body_GotFocus(object sender, RoutedEventArgs e)
        {
            HideSettingGrid();
        }


        private void ShowSettingGrid()
        {
            var anim = new System.Windows.Media.Animation.DoubleAnimation(70, (Duration)TimeSpan.FromSeconds(0.1));
            grid_Setting.BeginAnimation(ContentControl.HeightProperty, anim);
        }
        private void HideSettingGrid()
        {
            var anim = new System.Windows.Media.Animation.DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.1));
            grid_Setting.BeginAnimation(ContentControl.HeightProperty, anim);
        }


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

            if ((Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) && (e.Key == Key.Q))
            {
                ChangeTextBackgroundColor(Key.Q);

                e.Handled = true;
                return;
            }

            if ((Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) && (e.Key == Key.W))
            {
                ChangeTextBackgroundColor(Key.W);

                e.Handled = true;
                return;
            }
        }

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
