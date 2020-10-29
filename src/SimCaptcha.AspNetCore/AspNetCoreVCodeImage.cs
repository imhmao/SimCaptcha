using SimCaptcha.Interface;
using SimCaptcha.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
// Project: SimCaptcha
// https://github.com/yiyungent/SimCaptcha
// Author: yiyun <yiyungent@gmail.com>

namespace SimCaptcha.AspNetCore
{
    public class AspNetCoreVCodeImage : IVCodeImage
    {
        public VCodeImgModel Create(string code, int width, int height, ISimCaptchaOptions options)
        {
            VCodeImgModel rtnResult = new VCodeImgModel { VCodePos = new List<PointPosModel>() };

            // TODO: 变化点: 答案: 4个字
            int rightCodeLength = options.CodeLength;

            Bitmap Img = null;
            Graphics g = null;
            MemoryStream ms = null;

            Color[] color_Array = { Color.Black, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };

            if (options.Colors != null && options.Colors.Any())
            {
                List<Color> clrLst = new List<Color>();
                foreach (var c in options.Colors)
                {
                    if (c.StartsWith("#"))
                    {
                        try
                        {
                            clrLst.Add(Color.FromArgb(Convert.ToInt32($"FF{ c[1..]}", 16)));
                        }
                        catch { }
                    }
                    else
                        clrLst.Add(Color.FromName(c));
                }

                color_Array = clrLst.ToArray();
            }

            string[] fonts = options.Fonts;

            string _base = options.BackgroundPath;

            if (_base.StartsWith("~/"))
                _base = Path.Combine(Environment.CurrentDirectory, options.BackgroundPath[2..]);

            var _file_List = System.IO.Directory.GetFiles(_base);
            int imageCount = _file_List.Length;
            if (imageCount == 0)
                throw new Exception("image not Null");

            long tick = DateTime.Now.Ticks;
            Random random = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            int imageRandom = random.Next(1, (imageCount + 1));
            string _random_file_image = _file_List[imageRandom - 1];
            var imageStream = Image.FromFile(_random_file_image);

            Img = new Bitmap(imageStream, width, height);
            imageStream.Dispose();
            g = Graphics.FromImage(Img);


            int code_length = code.Length;
            List<string> words = new List<string>();
            for (int i = 0; i < code_length; i++)
            {
                int cindex = random.Next(color_Array.Length);
                int findex = random.Next(fonts.Length);
                using (Font f = new Font(fonts[findex], 15, FontStyle.Bold))
                {
                    using (Brush b = new SolidBrush(color_Array[cindex]))
                    {
                        string word = code.Substring(i, 1);
                        var m = g.MeasureString(word, f);

                        int _y = random.Next(height);
                        if (_y > (height - m.Height))
                            _y = _y - (int)m.Height * 2;


                        //int _x = width / (i + 1);

                        int _x = random.Next(width);
                        if ((width - _x) < m.Width)
                        {
                            _x = width - (int)m.Width - 10;
                        }

                        if (_y < 0)
                            _y = 0;

                        if (_x < 0)
                            _x = 0;

                        if (rtnResult.VCodePos.Count < rightCodeLength)
                        {
                            (int, int) percentPos = ToPercentPos((width, height), (_x, _y));
                            // 添加正确答案 位置数据
                            rtnResult.VCodePos.Add(new PointPosModel()
                            {
                                X = percentPos.Item1,
                                Y = percentPos.Item2,
                            });
                            words.Add(word);
                        }
                        g.DrawString(word, f, b, _x, _y);
                    }
                }
            }

            rtnResult.Words = words;
            rtnResult.VCodeTip = "请依次点击: " + string.Join(",", words);

            ms = new MemoryStream();
            Img.Save(ms, ImageFormat.Png);
            g.Dispose();
            Img.Dispose();
            ms.Dispose();
            rtnResult.ImgBase64 = "data:image/jpg;base64," + Convert.ToBase64String(ms.GetBuffer());

            return rtnResult;
        }


        /// <summary>
        /// 转换为相对于图片的百分比单位
        /// </summary>
        /// <param name="widthAndHeight">图片宽高</param>
        /// <param name="xAndy">相对于图片的绝对尺寸</param>
        /// <returns>(int:xPercent, int:yPercent)</returns>
        private (int, int) ToPercentPos((int, int) widthAndHeight, (int, int) xAndy)
        {
            (int, int) rtnResult = (0, 0);
            // 注意: int / int = int (小数部分会被截断)
            rtnResult.Item1 = (int)(((double)xAndy.Item1) / ((double)widthAndHeight.Item1) * 100);
            rtnResult.Item2 = (int)(((double)xAndy.Item2) / ((double)widthAndHeight.Item2) * 100);

            return rtnResult;
        }
    }
}
