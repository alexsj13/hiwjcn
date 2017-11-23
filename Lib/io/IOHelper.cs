using System;
using System.IO;
using System.Web;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.Linq;
using Lib.core;
using Lib.helper;
using System.Web.Hosting;

namespace Lib.io
{
    /// <summary>
    /// IO帮助类
    /// </summary>
    public sealed class IOHelper
    {
        public static readonly string[] TEXT_EXTENSION = new string[] { ".txt", ".css", ".js", ".log", ".py" };

        /// <summary>
        /// 读取文件属性，比如只读等
        /// </summary>
        /// <param name="file_path"></param>
        /// <returns></returns>
        public static FileAttributes GetFileAttr(string file_path)
        {
            return File.GetAttributes(file_path);
        }

        /// <summary>
        /// 着行读取文件
        /// </summary>
        /// <param name="file_path"></param>
        /// <param name="handler"></param>
        public static void ReadLineFromFile(string file_path, Action<string> handler)
        {
            using (var fs = new FileStream(file_path, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var line = string.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        handler.Invoke(line);
                    }
                }
            }
        }

        /// <summary>
        /// 获取文件的字节数组
        /// </summary>
        /// <param name="postFile"></param>
        /// <returns></returns>
        public static byte[] GetPostFileBytesAndDispose(HttpPostedFile postFile)
        {
            using (postFile.InputStream)
            {
                var b = new byte[postFile.ContentLength];
                if (postFile.InputStream.CanSeek)
                {
                    postFile.InputStream.Seek(0, SeekOrigin.Begin);
                }
                postFile.InputStream.Read(b, 0, b.Length);
                return b;
            }
        }

        public static byte[] GetFileBytes(string file_path)
        {
            //System.IO.File.ReadAllBytes(file_path);
            var f = new FileInfo(file_path);
            using (var fs = f.Open(FileMode.Open, FileAccess.Read))
            {
                var bb = new byte[fs.Length];
                fs.Read(bb, 0, bb.Length);
                return bb;
            }
        }

        public static string ReadFileString(string filePath, Encoding encoding = null)
        {
            var fi = new FileInfo(filePath);
            if (encoding == null) { encoding = Encoding.UTF8; }
            //共享文件锁
            using (var fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs, encoding))
                {
                    var b = new char[1024 * 100];
                    var str = new StringBuilder();
                    while (sr.Read(b, 0, b.Length) > 0)
                    {
                        str.Append(b);
                    }
                    return str.ToString();
                }
            }
        }

        /// <summary>
        /// 如果目录不存在就创建
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void CreatePathIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 获取当前程序根目录（来自nop）
        /// </summary>
        /// <returns></returns>
        public static string GetBinDirectory()
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HttpRuntime.BinDirectory;
            }

            //not hosted. For example, run either in unit tests
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 先试用contenttype获取扩展名，再使用filename获取扩展名
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileExtension(string ContentType, string FileName)
        {
            var ext = string.Empty;
            //使用contentType
            if (ValidateHelper.IsPlumpString(ContentType))
            {
                var sp = ConvertHelper.NotNullList(ContentType.Split('/')).Where(x => ValidateHelper.IsPlumpString(x)).ToArray();
                if (sp.Length == 2)
                {
                    ext = "." + sp[1];
                }
            }
            //使用文件名
            if (ValidateHelper.IsPlumpString(FileName) && FileName.IndexOf('.') >= 0)
            {
                /*
                string[] filenamesplit = filename.Split('.');
                filename = filenamesplit[0];
                string file_extesion = "." + filenamesplit[filenamesplit.Length - 1].Trim().ToLower();*/
                //filename.Substring(filename.LastIndexOf("."));
                //.jpg
                ext = Path.GetExtension(FileName);
            }
            return ConvertHelper.GetString(ext).ToLower();
        }

        /// <summary>
        /// 获取系统 文件目录分隔符[\\/]
        /// </summary>
        /// <returns></returns>
        public static string GetSysPathSeparator()
        {
            //建议使用这种方式类似于join
            //var path = Path.Combine("c:\\", "wj", "files");
            return ConvertHelper.GetString(Path.DirectorySeparatorChar);
        }

        public static class DirectoryHelper
        {
            public static bool Exists(string path)
            {
                return Directory.Exists(path);
            }

            public static string[] ListFiles(string path)
            {
                return Directory.GetFiles(path);
            }

            public static void Delete(string path)
            {
                Directory.Delete(path);
            }
            public static void Create(string path)
            {
                Directory.CreateDirectory(path);
            }
        }

        public static class FileHelper
        {
            public static bool Exists(string path)
            {
                return File.Exists(path);
            }

            public static void Move(string path, string to)
            {
                File.Move(path, to);
            }

            public static void Copy(string path, string to)
            {
                File.Copy(path, to);
            }

            public static void Delete(string path)
            {
                File.Delete(path);
            }
        }


        /// <summary>
        /// 获取文件的编码格式
        /// </summary>
        public class EncodingType
        {
            /// <summary>
            /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型
            /// </summary>
            /// <param name="FILE_NAME">文件路径</param>
            /// <returns>文件的编码类型</returns>
            public static System.Text.Encoding GetType(string FILE_NAME)
            {
                using (var fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read))
                {
                    return GetType(fs);
                }
            }

            /// <summary>
            /// 通过给定的文件流，判断文件的编码类型
            /// </summary>
            /// <param name="fs">文件流</param>
            /// <returns>文件的编码类型</returns>
            public static System.Text.Encoding GetType(FileStream fs)
            {
                byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
                byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
                byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
                var reVal = Encoding.Default;

                using (var r = new BinaryReader(fs, System.Text.Encoding.Default))
                {
                    int i;
                    int.TryParse(fs.Length.ToString(), out i);
                    byte[] ss = r.ReadBytes(i);
                    if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
                    {
                        reVal = Encoding.UTF8;
                    }
                    else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
                    {
                        reVal = Encoding.BigEndianUnicode;
                    }
                    else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
                    {
                        reVal = Encoding.Unicode;
                    }
                    return reVal;
                }
            }
            /// <summary>
            /// 判断是否是不带 BOM 的 UTF8 格式
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            private static bool IsUTF8Bytes(byte[] data)
            {
                int charByteCounter = 1;　 //计算当前正分析的字符应还有的字节数
                byte curByte; //当前分析的字节.
                for (int i = 0; i < data.Length; i++)
                {
                    curByte = data[i];
                    if (charByteCounter == 1)
                    {
                        if (curByte >= 0x80)
                        {
                            //判断当前
                            while (((curByte <<= 1) & 0x80) != 0)
                            {
                                charByteCounter++;
                            }
                            //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                            if (charByteCounter == 1 || charByteCounter > 6)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //若是UTF-8 此时第一位必须为1
                        if ((curByte & 0xC0) != 0x80)
                        {
                            return false;
                        }
                        charByteCounter--;
                    }
                }
                if (charByteCounter > 1)
                {
                    throw new Exception("非预期的byte格式");
                }
                return true;
            }
        }

    }
}
