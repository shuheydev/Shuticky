using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace 付箋アプリ
{
    static class ColorConverter
    {
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
    }
}
