using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace 付箋アプリ
{
    interface IShutickySetting
    {
        string FilePath
        { get; set; }
        string Title
        { get; set; }

        //string ColorCode
        //{ get; set; }
        int ColorNumber
        { get; set; }
        double Position_X
        { get; set; }
        double Position_Y
        { get; set; }
        double Size_Width
        { get; set; }
        double Size_Height
        { get; set; }
        DisplayStatus DisplayStatus
        { get; set; }
    }

    public enum DisplayStatus
    {
        Visible,
        Minimize,
        Hidden,
    }

    public class ShutickySetting:IShutickySetting
    {
        public string FilePath
        { get; set; }
        public string Title
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
        public DisplayStatus DisplayStatus
        { get; set; }

        public ShutickySetting(string filePath)
        {
            FilePath = filePath;//RTFファイルのパス
            Title =　Path.GetFileNameWithoutExtension(filePath);

            ColorNumber = 0;

            Position_X = 20;
            Position_Y = 20;

            Size_Width = 280;
            Size_Height = 250;

            DisplayStatus = DisplayStatus.Visible;
        }


        //XMLデシリアライズ、シリアライズには引数なしのコンストラクタが必要。
        public ShutickySetting()
        {
        }
    }



    public class BackGroundColorSet
    {
        public string TitleColor
        { get; set; }
        public string BodyColor
        { get; set; }

        public BackGroundColorSet()
        {
            TitleColor = "";
            BodyColor = "";
        }
    }
}
