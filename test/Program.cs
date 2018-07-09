using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            var captcha = "unknow";
            for (var i = 0; i < 10; i++)
            {
                var iconPath = AppDomain.CurrentDomain.BaseDirectory + $"ems-captcha-{i}.png";

                using (FileStream fileStream = new FileStream(iconPath, FileMode.Open, FileAccess.Read))
                {
                    System.Drawing.Bitmap image = new System.Drawing.Bitmap(fileStream);
                    Me.Dan.CaptchaRecog.Ems EmsRecognizer = new Me.Dan.CaptchaRecog.Ems(image);
                    captcha = EmsRecognizer.Analyse();
                };
                Console.WriteLine(captcha);

            }

            Console.ReadKey();
        }
    }
}
