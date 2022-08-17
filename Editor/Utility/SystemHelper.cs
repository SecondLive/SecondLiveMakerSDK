using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace SecondLive.Maker.Editor
{
    public static class SystemHelper
    {
        readonly static DateTime m_Basic = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        
        public static long NewGUID()
        {
            var gb = System.Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(gb, 0);
        }

        public static string Hash(string text)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(text);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }
        
        public static long NowTimeStamp()
        {
            var ts = DateTime.UtcNow.Subtract(m_Basic);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}
