using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.extension;

namespace Lib.io
{
    public static class GZipHelper
    {
        private static Encoding _encoding { get => Encoding.UTF8; }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="text">文本</param>
        public static string Compress(string text)
        {
            if (!ValidateHelper.IsPlumpString(text))
            {
                return text;
            }
            var buffer = _encoding.GetBytes(text);
            return Convert.ToBase64String(Compress(buffer));
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="text">文本</param>
        public static string Decompress(string text)
        {
            if (!ValidateHelper.IsPlumpString(text))
            {
                return text;
            }
            var buffer = Convert.FromBase64String(text);
            using (var ms = new MemoryStream(buffer))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(zip))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="buffer">字节流</param>
        public static byte[] Compress(byte[] buffer)
        {
            if (buffer == null)
                return null;
            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(buffer, 0, buffer.Length);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="buffer">字节流</param>
        public static byte[] Decompress(byte[] buffer)
        {
            if (buffer == null)
                return null;
            using (var ms = new MemoryStream(buffer))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(zip))
                    {
                        return _encoding.GetBytes(reader.ReadToEnd());
                    }
                }
            }
        }

    }
}
