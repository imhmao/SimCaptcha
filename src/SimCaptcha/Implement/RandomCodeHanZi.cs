using SimCaptcha.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
// Project: SimCaptcha
// https://github.com/yiyungent/SimCaptcha
// Author: yiyun <yiyungent@gmail.com>

namespace SimCaptcha.Implement
{
    public class RandomCodeHanZi : IRandomCode
    {
        public string Create(ISimCaptchaOptions options)
        {
            var str = options.HZ;

            if (string.IsNullOrEmpty(str))
                throw new Exception("没有设置随机汉字字符串");

            char[] str_char_arrary = str.ToArray();
            long tick = DateTime.Now.Ticks;
            Random rand = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            HashSet<string> hs = new HashSet<string>();
            bool randomBool = true;
            while (randomBool)
            {
                if (hs.Count == options.WashCodeLength)
                    break;

                int rand_number = rand.Next(str_char_arrary.Length);
                hs.Add(str_char_arrary[rand_number].ToString());
            }
            string code = string.Join("", hs);
            return code;
        }
    }
}
