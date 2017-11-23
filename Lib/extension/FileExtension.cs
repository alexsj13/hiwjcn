using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using Lib.helper;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Lib.extension
{
    public static class FileExtension
    {
        /// <summary>
        /// bitmap转byte数组
        /// </summary>
        public static byte[] ToBytes(this Bitmap bm, ImageFormat formart) =>
            ConvertHelper.BitmapToBytes(bm, formart);

        /// <summary>
        /// 获取文件MD5
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetMD5(this FileInfo file)
        {
            if (!file.Exists) { throw new Exception($"无法读取MD5，文件{file.FullName}不存在"); }
            return SecureHelper.GetFileMD5(file.FullName);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file"></param>
        public static void DeleteIfExist(this FileInfo file)
        {
            if (!file.Exists) { return; }
            file.Delete();
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dir"></param>
        public static void CreateIfNotExist(this DirectoryInfo dir)
        {
            if (dir.Exists) { return; }
            dir.Create();
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        public static void DeleteIfExist(this DirectoryInfo dir, bool recursive = true)
        {
            if (!dir.Exists) { return; }
            dir.Delete(recursive: recursive);
        }

    }
}
