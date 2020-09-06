# EmsCAPTCHA-Recognition
EMS验证码识别
采用简单的相似度算法，并对结果比较作了一些优化，以确保可得匹配的数字

调用方式参考如下：

    class Program
    {
        static void Main(string[] args)
        {
            var captcha = "unknow";
            for (var i = 0; i < 10; i++)
            {
                var iconPath =$"{AppDomain.CurrentDomain.BaseDirectory}ems-captcha-{i}.png";
                using (Stream fileStream = GoGetStream())
                {
                    System.Drawing.Bitmap image = new System.Drawing.Bitmap(fileStream);
                    image.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
                    Me.Dan.CaptchaRecog.Ems EmsRecognizer = new Me.Dan.CaptchaRecog.Ems(image);
                    captcha = EmsRecognizer.Analyse();
                    Console.WriteLine($"captcha-file:{iconPath} , recognized as :{captcha}");
                    System.Threading.Thread.Sleep(1000);
                }
            }

            Console.ReadKey();
        }

        private static Stream Gzip(HttpWebResponse HWResp)
        {
            Stream stream1 = HWResp.GetResponseStream();
            if (HWResp.ContentEncoding.ToLower().Contains("gzip"))
            {
                stream1 = new System.IO.Compression.GZipStream(stream1, System.IO.Compression.CompressionMode.Decompress);
            }
            else if (HWResp.ContentEncoding.ToLower().Contains("deflate"))
            {
                stream1 = new System.IO.Compression.DeflateStream(stream1, System.IO.Compression.CompressionMode.Decompress);
            }

            return stream1;
        }


        public static Stream GoGetStream()
        {
            HttpWebRequest re = (HttpWebRequest)HttpWebRequest.Create("http://www.ems.com.cn/ems/rand");
            HttpWebResponse rep = null;
            Stream oresponseStream = null;
            try
            {
                rep = (HttpWebResponse)re.GetResponse();
                //oresponseStream = rep.GetResponseStream();
                oresponseStream = Gzip(rep);
                return oresponseStream;
            }
            catch (Exception err)
            {
                return null;
            }
        }

    }
