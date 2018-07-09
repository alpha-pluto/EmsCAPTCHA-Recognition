using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Me.Dan.CaptchaRecog
{
    public class Ems : ImageBase
    {
        #region para

        private int picNum = 6;

        private int valvedGray = 125;

        /// <summary>
        /// 基本的图片点阵
        /// </summary>
        private string[] CodeArray =
            new string[] {
                "000111000011111110011000110110000011110000011110000011110000011110000011110000011110000011011000110011111110000111000",//0
                "00111000111110001111100000011000000110000001100000011000000110000001100000011000000110001111111111111111",//1
                "01111100111111101000001100000011000000110000011000001100000110000011000001100000110000001111111111111111",//2
                "01111100111111111000001100000011000001100111110001111110000001110000001100000011100001111111111001111100",//3
                "000001100000011100000011100000111100001101100001101100011001100011001100111111111111111111000001100000001100000001100",//4
                "11111111111111111100000011000000110000001111100011111110000001110000001100000011100001111111111001111100",//5
                "000111100001111110011000010011000000110000000110111100111111110111000111110000011110000011011000111011111110000111100",//6
                "11111111111111110000001100000010000001100000110000001000000110000001000000110000001100000110000001100000",//7
                "001111100011111110011000110011000110011100100001111100001111100011001110110000011110000011111000111011111110001111100",//8
                "001111000011111110111000110110000011110000011111000111011111111001111011000000011000000110010000110011111100001111000"//9

            };
        #endregion

        #region constructor

        public Ems(Bitmap bmp)
            : base(bmp)
        {

        }

        #endregion

        #region methodes

        /// <summary>
        /// 去除杂点
        /// </summary>
        /// <param name="bmp"></param>
        private void EraseNoiseDotAndGray(Bitmap bmp)
        {
            for (int w = 0; w < 85; w++)
            {
                for (int h = 0; h < 20; h++)
                {
                    Color __tmpColor = bmp.GetPixel(w, h);
                    if (__tmpColor.R < valvedGray || __tmpColor.G < valvedGray || __tmpColor.B < valvedGray) //为杂点，变为白色
                        bmp.SetPixel(w, h, Color.White);
                    else
                        bmp.SetPixel(w, h, Color.Black);
                }
            }
        }

        public string Analyse()
        {
            string ret = string.Empty;
            GrayByPixels(); //灰度处理
            GetPicValidByValue(valvedGray, picNum); //得到有效空间
            Bitmap[] pics = GetSplitPics(picNum, 1);     //分割

            if (pics.Length == picNum)
            {
                // 重新调整大小
                for (int ii = 0; ii < pics.Length; ii++)
                {
                    pics[ii] = GetPicValidByValue(pics[ii], valvedGray);
                }
            }
            char singleChar = ' ';
            double _currentSimilarity = 0.00;
            for (int i = 0; i < picNum; i++)
            {
                List<MatchedChar> _posi_char = new List<MatchedChar>();
                singleChar = ' ';
                _currentSimilarity = 0.00;
                string code = GetSingleBmpCode(pics[i], valvedGray);   //得到代码串
                for (int arrayIndex = 0; arrayIndex < CodeArray.Length; arrayIndex++)
                {
                    MatchedChar _c_char = new MatchedChar();
                    _c_char._similarity = GetStrSimilarity(CodeArray[arrayIndex], code);
                    _c_char._char = (char)(48 + arrayIndex);
                    _posi_char.Add(_c_char);
                }
                foreach (MatchedChar c in _posi_char)
                {
                    if (_currentSimilarity < c._similarity)
                    {
                        singleChar = c._char;
                        _currentSimilarity = c._similarity;
                    }
                }
                ret += singleChar;
            }
            return ret;
        }

        #endregion
    }
}
