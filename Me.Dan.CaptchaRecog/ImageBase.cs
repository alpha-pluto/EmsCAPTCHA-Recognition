using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Me.Dan.CaptchaRecog
{
    public class ImageBase
    {
        #region 原生态

        protected Bitmap bmpobj;

        public ImageBase(Bitmap pic)
        {
            bmpobj = pic;
        }

        /// <summary>
        /// 根据RGB，计算灰度值
        /// </summary>
        /// <param name="posClr">Color值</param>
        /// <returns>灰度值</returns>
        private int GetGrayNumColor(System.Drawing.Color posClr)
        {
            return (posClr.R * 19595 + posClr.G * 38469 + posClr.B * 7472) >> 16;
        }

        /// <summary>
        /// 灰度转换,逐点方式
        /// </summary>
        public void GrayByPixels()
        {
            for (int i = 0; i < bmpobj.Height; i++)
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int tmpValue = GetGrayNumColor(bmpobj.GetPixel(j, i));
                    bmpobj.SetPixel(j, i, Color.FromArgb(tmpValue, tmpValue, tmpValue));
                }
            }
        }

        /// <summary>
        /// 去图形边框
        /// </summary>
        /// <param name="borderWidth"></param>
        public void ClearPicBorder(int borderWidth)
        {
            for (int i = 0; i < bmpobj.Height; i++)
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    if (i < borderWidth || j < borderWidth || j > bmpobj.Width - 1 - borderWidth || i > bmpobj.Height - 1 - borderWidth)
                        bmpobj.SetPixel(j, i, Color.FromArgb(255, 255, 255));
                }
            }
        }

        /// <summary>
        /// 灰度转换,逐行方式
        /// </summary>
        public void GrayByLine()
        {
            Rectangle rec = new Rectangle(0, 0, bmpobj.Width, bmpobj.Height);
            BitmapData bmpData = bmpobj.LockBits(rec, ImageLockMode.ReadWrite, bmpobj.PixelFormat);// PixelFormat.Format32bppPArgb);
            //    bmpData.PixelFormat = PixelFormat.Format24bppRgb;
            IntPtr scan0 = bmpData.Scan0;
            int len = bmpobj.Width * bmpobj.Height;
            int[] pixels = new int[len];
            Marshal.Copy(scan0, pixels, 0, len);

            //对图片进行处理
            int GrayValue = 0;
            for (int i = 0; i < len; i++)
            {
                GrayValue = GetGrayNumColor(Color.FromArgb(pixels[i]));
                pixels[i] = (byte)(Color.FromArgb(GrayValue, GrayValue, GrayValue)).ToArgb();      //Color转byte
            }

            bmpobj.UnlockBits(bmpData);

            ////输出
            //GCHandle gch = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            //bmpOutput = new Bitmap(bmpobj.Width, bmpobj.Height, bmpData.Stride, bmpData.PixelFormat, gch.AddrOfPinnedObject());
            //gch.Free();
        }

        /// <summary>
        /// 得到有效图形并调整为可平均分割的大小
        /// </summary>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        /// <param name="CharsCount">有效字符数</param>
        public void GetPicValidByValue(int dgGrayValue, int CharsCount)
        {
            int posx1 = bmpobj.Width; int posy1 = bmpobj.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < bmpobj.Height; i++)      //找有效区
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int pixelValue = bmpobj.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)     //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            // 确保能整除
            int Span = CharsCount - (posx2 - posx1 + 1) % CharsCount;   //可整除的差额数
            if (Span < CharsCount)
            {
                int leftSpan = Span / 2;    //分配到左边的空列 ，如span为单数,则右边比左边大1
                if (posx1 > leftSpan)
                    posx1 = posx1 - leftSpan;
                if (posx2 + Span - leftSpan < bmpobj.Width)
                    posx2 = posx2 + Span - leftSpan;
            }
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            bmpobj = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);
        }

        /// <summary>
        /// 得到有效图形,图形为类变量
        /// </summary>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        public void GetPicValidByValue(int dgGrayValue)
        {
            int posx1 = bmpobj.Width;
            int posy1 = bmpobj.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < bmpobj.Height; i++)      //找有效区
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int pixelValue = bmpobj.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)     //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            bmpobj = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);
        }

        /// <summary>
        /// 得到有效图形,图形由外面传入
        /// </summary>
        /// <param name="singlepic"></param>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        /// <returns></returns>
        public Bitmap GetPicValidByValue(Bitmap singlepic, int dgGrayValue)
        {
            int posx1 = singlepic.Width; int posy1 = singlepic.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < singlepic.Height; i++)      //找有效区
            {
                for (int j = 0; j < singlepic.Width; j++)
                {
                    int pixelValue = singlepic.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)     //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            return singlepic.Clone(cloneRect, singlepic.PixelFormat);
        }

        /// <summary>
        /// 平均分割图片
        /// </summary>
        /// <param name="RowNum">水平上分割数</param>
        /// <param name="ColNum">垂直上分割数</param>
        /// <returns>分割好的图片数组</returns>
        public Bitmap[] GetSplitPics(int RowNum, int ColNum)
        {
            if (RowNum == 0 || ColNum == 0)
                return null;
            int singW = bmpobj.Width / RowNum;
            int singH = bmpobj.Height / ColNum;
            Bitmap[] PicArray = new Bitmap[RowNum * ColNum];

            Rectangle cloneRect;
            for (int i = 0; i < ColNum; i++)      //找有效区
            {
                for (int j = 0; j < RowNum; j++)
                {
                    cloneRect = new Rectangle(j * singW, i * singH, singW, singH);
                    PicArray[i * RowNum + j] = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);//复制小块图
                }
            }
            return PicArray;
        }

        /// <summary>
        /// 返回灰度图片的点阵描述字串，1表示灰点，0表示背景
        /// </summary>
        /// <param name="singlepic">灰度图</param>
        /// <param name="dgGrayValue">背前景灰色界限</param>
        /// <returns></returns>
        public string GetSingleBmpCode(Bitmap singlepic, int dgGrayValue)
        {
            Color piexl;
            string code = "";
            for (int posy = 0; posy < singlepic.Height; posy++)
                for (int posx = 0; posx < singlepic.Width; posx++)
                {
                    piexl = singlepic.GetPixel(posx, posy);
                    if (piexl.R < dgGrayValue)    // Color.Black )
                        code = code + "1";
                    else
                        code = code + "0";
                }
            return code;
        }

        #region similarity calculating

        public int minValue(int a, int b, int c)
        {
            if (a < b && a < c) return a;
            else if (b < a && b < c) return b;
            else return c;
        }

        public int calculateStringDistance(string strA, string strB)
        {
            int lenA = strA.Length;
            int lenB = strB.Length;
            int[,] c = new int[lenA + 1, lenB + 1];
            // Record the distance of all begin points of each string
            //初始化方式与背包问题有点不同
            for (int i = 0; i < lenA; i++) c[i, lenB] = lenA - i;
            for (int j = 0; j < lenB; j++) c[lenA, j] = lenB - j;
            c[lenA, lenB] = 0;
            for (int i = lenA - 1; i >= 0; i--)
                for (int j = lenB - 1; j >= 0; j--)
                {
                    if (strB[j] == strA[i])
                        c[i, j] = c[i + 1, j + 1];
                    else
                        c[i, j] = minValue(c[i, j + 1], c[i + 1, j], c[i + 1, j + 1]) + 1;
                }

            return c[0, 0];
        }

        public double GetStrSimilarity(string sourceStr, string destStr)
        {
            int distance = calculateStringDistance(sourceStr, destStr);
            return 100.00 / (distance + 1);
        }

        #endregion

        #endregion

        #region 缩略图
        private int _level = 100; //缩略图的质量 1-100的范围
        private int _icnWidth;
        private int _icnHeight;
        private string _pathPhysic;
        private string _imgFile;
        private string _neoFile;
        private Bitmap _bitMap;

        public ImageBase(
            int inLevel,
            int inWidth,
            int inHeight,
            string inPath,
            string inFile,
            string inNeoFile
        )
        {
            this._level = (inLevel >= 60 && inLevel <= 100) ? inLevel : 80;
            this._icnWidth = (inWidth >= 60 && inWidth <= 800) ? inWidth : 80;
            this._icnHeight = (inHeight >= 60 && inHeight <= 800) ? inHeight : 80;
            this._pathPhysic = inPath;
            this._imgFile = inFile;
            this._neoFile = inNeoFile;
        }

        public bool GetThumbnail(out string retCode)
        {
            bool retFlag = false;
            retCode = string.Empty;
            System.Drawing.Image oImg = null;
            try
            {

                oImg = System.Drawing.Image.FromFile(_pathPhysic + _imgFile);

                _bitMap = new Bitmap(oImg);

                if (_bitMap.Width > 100)
                {
                    // ＝＝＝处理JPG质量的函数＝＝＝
                    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo ici = null;
                    foreach (ImageCodecInfo codec in codecs)
                    {
                        if (codec.MimeType == "image/jpeg")
                        {
                            ici = codec;
                        }
                    }
                    EncoderParameters ep = new EncoderParameters();
                    ep.Param[0] =
                        new EncoderParameter(
                            System.Drawing.Imaging.Encoder.Quality,
                            (long)this._level
                        );

                    this._icnWidth = this._icnWidth < this._bitMap.Width ? this._icnWidth : this._bitMap.Width;
                    this._icnHeight = (int)((decimal)this._bitMap.Height * ((decimal)this._icnWidth / (decimal)this._bitMap.Width));

                    this._bitMap
                        .GetThumbnailImage(this._icnWidth, this._icnHeight, null, new System.IntPtr(0))
                        .Save(this._pathPhysic + this._neoFile, ici, ep);

                    retCode = "COMPLETE";
                }
                else
                {
                    retCode = "SMALL_SIZE";
                }
                oImg.Dispose();
                _bitMap.Dispose();
                retFlag = true;
            }
            catch (Exception err)
            {
                oImg.Dispose();
                _bitMap.Dispose();
                retFlag = false;
                retCode = err.ToString();
            }
            finally
            {
                oImg.Dispose();
                _bitMap.Dispose();
            }
            return retFlag;
        }

        #endregion
    }

    [Serializable]
    public class MatchedChar
    {
        public char _char { get; set; }
        public double _similarity { get; set; }
    }

}
