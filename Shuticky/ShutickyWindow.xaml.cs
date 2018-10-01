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
using System.Text.RegularExpressions;
using System.Printing;

namespace Shuticky
{
    /// <summary>
    /// 付箋Window.xaml の相互作用ロジック
    /// </summary>
    public partial class ShutickyWindow : Window
    {
        private ShutickySetting _shutickySetting;
        //private List<ReminderData> _reminders;
        public List<ReminderData> Reminders
        { get; private set; }

        private static readonly List<BackGroundColorSet> _presetBackGroundColorSet = new List<BackGroundColorSet>()
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

            SetPinStatus(_shutickySetting.IsPinned);
            SetShutickyColor(_shutickySetting.ColorNumber);

            SetDisplayStatus(_shutickySetting.DisplayStatus);

            this.Reminders = ExtractRemindDatas();
        }

        private List<ReminderData> ExtractRemindDatas()
        {
            var result = new List<ReminderData>();

            var text_Body_Sentences = GetText_Body().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var sentence in text_Body_Sentences)
            {
                var reminders_sentence = ReminderData.ReminderPattern.Select(pattern =>
                {
                    //
                    var match = Regex.Match(sentence, pattern);
                    if (match.Success)
                    {
                        var reminder = new ReminderData()
                        {
                            Title = _shutickySetting.Title,
                            Content = sentence.Replace(ReminderData._reminderTag, ""),
                            DateAndTime = DateTime.MinValue,
                            Year = match.Groups["Year"].Value,
                            Month = match.Groups["Month"].Value,
                            Day = match.Groups["Day"].Value,
                            Hour = match.Groups["Hour"].Value,
                            Minute = match.Groups["Minute"].Value,
                        };
                        string dateTimeString = "";
                        if (!string.IsNullOrWhiteSpace(reminder.Year))
                        {
                            dateTimeString += $"{reminder.Year}年";

                        }
                        else
                        {
                            reminder.IntervalType = RegularIntervalType.EveryYear;
                            reminder.RemindBefore = new TimeSpan(1, 0, 0, 0);
                        }
                        if (!string.IsNullOrWhiteSpace(reminder.Month))
                        {
                            dateTimeString += $"{reminder.Month}月";
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(dateTimeString))
                            {
                                reminder.IntervalType = RegularIntervalType.EveryMonth;
                                reminder.RemindBefore = new TimeSpan(1, 0, 0, 0);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(reminder.Day))
                        {
                            dateTimeString += $"{reminder.Day}日";
                            reminder.RemindBefore = new TimeSpan(1, 0, 0, 0);
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(dateTimeString))
                            {
                                reminder.IntervalType = RegularIntervalType.EveryDay;
                                reminder.RemindBefore = new TimeSpan(1, 0, 0, 0);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(reminder.Hour))
                        {
                            dateTimeString += $"{reminder.Hour}時";
                            reminder.RemindBefore = new TimeSpan(0, 0, 30, 0);
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(dateTimeString))
                            {
                                reminder.IntervalType = RegularIntervalType.EveryHour;
                                reminder.RemindBefore = new TimeSpan(0, 1, 0, 0);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(reminder.Minute))
                        {
                            dateTimeString += $"{reminder.Minute}分";
                            reminder.RemindBefore = new TimeSpan(0, 0, 30, 0);
                        }
                        else
                        {

                        }

                        DateTime.TryParse(dateTimeString, out DateTime dateAndTime);
                        reminder.DateAndTime = dateAndTime;

                        return reminder;
                    }

                    return null;

                }).Where(remind => remind != null);

                result.AddRange(reminders_sentence);
            }

            return result;
        }

        private string GetText_Body()
        {
            return new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd).Text;
        }


        #region イベント、イベントハンドラ
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
            DeleteButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void Button_Pin_Click(object sender, RoutedEventArgs e)
        {
            if (this.Topmost == true)
            {//ピン留めされていた場合
                SetPinStatus(false);
            }
            else
            {//ピン留めされていない場合
                SetPinStatus(true);
            }
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

        /// <summary>
        /// 付箋ウィンドウが非アクティブになったとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            HideSettingGrid();

            ExtractRemindDatas();
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

            if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
            {
                PrintRichTextContent();

                e.Handled = true;
                return;
            }
        }


        /// <summary>
        /// https://blogs.msdn.microsoft.com/prajakta/2007/01/02/printing-contents-of-wpf-richtextbox/
        /// を参考にした。感謝したい。
        /// もう少し余白を空けたい。
        /// </summary>
        private void PrintRichTextContent()
        {
            var sourceDocument = new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd);

            var memstrm = new MemoryStream();

            //flowDocumentを複製するために、一度MemoryStreamに書き込んでいる。
            sourceDocument.Save(memstrm, DataFormats.XamlPackage);


            var flowDocumentCopy = new FlowDocument();

            var copyDocumentRange = new TextRange(flowDocumentCopy.ContentStart, flowDocumentCopy.ContentEnd);


            //MemoryStreamから読み込み
            copyDocumentRange.Load(memstrm, DataFormats.XamlPackage);


            PrintDocumentImageableArea ia = null;


            var docWriter = PrintQueue.CreateXpsDocumentWriter(ref ia);


            if (docWriter != null && ia != null)
            {
                var paginator = ((IDocumentPaginatorSource)flowDocumentCopy).DocumentPaginator;

                paginator.PageSize = new Size(ia.MediaSizeWidth, ia.MediaSizeHeight);

                //var pagePadding = flowDocumentCopy.PagePadding;

                ////pagePaddingの中身がNaNになっている場合があるので、その場合はNaNを0とする。
                //if (double.IsNaN(pagePadding.Left))
                //    pagePadding.Left = 0;
                //if (double.IsNaN(pagePadding.Top))
                //    pagePadding.Top = 0;
                //if (double.IsNaN(pagePadding.Right))
                //    pagePadding.Right = 0;
                //if (double.IsNaN(pagePadding.Bottom))
                //    pagePadding.Bottom = 0;

                //flowDocumentCopy.PagePadding = new Thickness(
                //    Math.Max(ia.OriginWidth, pagePadding.Left),
                //    Math.Max(ia.OriginHeight, pagePadding.Top),
                //    Math.Max(ia.MediaSizeWidth - (ia.OriginWidth + ia.ExtentWidth), pagePadding.Right),
                //    Math.Max(ia.MediaSizeHeight - (ia.OriginHeight + ia.ExtentHeight), pagePadding.Bottom)
                //    );

                flowDocumentCopy.PagePadding = new Thickness(50, 50, 50, 50);

                flowDocumentCopy.ColumnWidth = double.PositiveInfinity;

                docWriter.Write(paginator);
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

        #endregion


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

            _shutickySetting.IsPinned = this.Topmost;

            return _shutickySetting;
        }

        /// <summary>
        /// 付箋カラーをセット
        /// </summary>
        /// <param name="colorNumber"></param>
        public void SetShutickyColor(int colorNumber)
        {
            _shutickySetting.ColorNumber = colorNumber;

            string titleColorCode = _presetBackGroundColorSet[_shutickySetting.ColorNumber].TitleColor;
            string bodyColorCode = _presetBackGroundColorSet[_shutickySetting.ColorNumber].BodyColor;
            dockPanel_TitleBar.Background = new SolidColorBrush(ColorConverter.GetArbgColor(titleColorCode, 0));
            dockPanel_Body.Background = new SolidColorBrush(ColorConverter.GetArbgColor(bodyColorCode, 0));
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

        public void SetDisplayPosition(double x, double y)
        {
            this.Top = y;
            this.Left = x;
        }

        private void SetPinStatus(bool pinned)
        {
            this.Topmost = pinned;

            if (pinned)
            {
                image_Pin.Source = new BitmapImage(new Uri("Resources/Icon_UnPin.png", UriKind.Relative));//アイコンをピン留め解除にする
                button_Pin.ToolTip = "ピン留めを外す";
            }
            else
            {
                image_Pin.Source = new BitmapImage(new Uri("Resources/Icon_Pinned.png", UriKind.Relative));//アイコンをピン留めにする
                button_Pin.ToolTip = "ピン留め";
            }
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
            TextRange range_Body = new TextRange(richTextBox_Body.Document.ContentStart, richTextBox_Body.Document.ContentEnd);

            using (FileStream fStream = new FileStream(_shutickySetting.FilePath, FileMode.Create))
            {
                range_Body.Save(fStream, DataFormats.Rtf);
            }
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

        private void MenuItem_Print_Click(object sender, RoutedEventArgs e)
        {
            PrintRichTextContent();
        }
    }

    public class ReminderData
    {
        public static readonly string _reminderTag = "tt::";

        public static readonly List<string> ReminderPattern = new List<string>()
        {
            _reminderTag+ @"(?<Month>\d+?)月(?<Day>\d+?)日[^\d]",//毎年定期月日
            _reminderTag+@"(?<Month>\d\d)(?<Day>\d\d)",//毎年定期月日
            _reminderTag+@"(?<Hour>\d+?):(?<Minute>\d+?)",//毎日定時分
            _reminderTag+@"(?<Hour>\d+?)時(?<Minute>\d+?)分",//毎日定時分
            _reminderTag+@"(?<Hour>\d+?)時[^\d]",//毎日定時
            _reminderTag+@"(?<Month>\d+?)月(?<Day>\d+?)日(?<Hour>\d+?):(?<Minute>\d+?)",//毎年月日時
            _reminderTag+ @"(?<Month>\d+?)月(?<Day>\d+?)日(?<Hour>\d+?)時(?<Minute>\d+?)分",//毎年月日時
            _reminderTag+ @"(?<Month>\d+?)月(?<Day>\d+?)日(?<Hour>\d+?)時[^\d]",//毎年月日時
            _reminderTag+@"(?<Day>\d+?)日(?<Hour>\d+?):(?<Minute>\d+?)",//毎月定期日時分
            _reminderTag+@"(?<Day>\d+?)日(?<Hour>\d+?)時(?<Minute>\d+?)分",//毎月定期日時分
            _reminderTag+@"(?<Day>\d+?)日(?<Hour>\d+?)時[^\d]",//毎月定期日時
            _reminderTag+@"(?<Day>\d+?)日[^\d]",//毎月定期日
            _reminderTag+@"[^時](?<Minute>\d+?)分",//毎時定期分
        };

        public string Title
        { get; set; } = "";
        public string Content
        { get; set; } = "";
        public DateTime DateAndTime
        { get; set; }
        public string Year
        { get; set; }
        public string Month
        { get; set; }
        public string Day
        { get; set; }
        public string Hour
        { get; set; }
        public string Minute
        { get; set; }
        public TimeSpan RemindBefore
        { get; set; }
        public RegularIntervalType IntervalType
        { get; set; } = RegularIntervalType.None;

        public ReminderData()
        {
        }
    }

    public enum RegularIntervalType
    {
        EveryYear,
        EveryMonth,
        EveryDay,
        EveryHour,
        None,
    }
}
