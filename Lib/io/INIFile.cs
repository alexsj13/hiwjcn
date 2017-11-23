using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Lib.io
{
    /// <summary>
    /// INI文件读写类。
    /// </summary>
	public class INIFile
    {
        private readonly string path;

        public INIFile(string INIPath)
        {
            this.path = INIPath;
            if (!File.Exists(this.path ?? throw new Exception("文件地址不能为空")))
            {
                throw new Exception($"文件{this.path}不存在");
            }
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);


        /// <summary>
        /// 写INI文件
        /// </summary>
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        public string IniReadValue(string Section, string Key)
        {
            var temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }

        public byte[] IniReadValues(string section, string key)
        {
            var temp = new byte[255];
            int i = GetPrivateProfileString(section, key, "", temp, 255, this.path);
            return temp;

        }


        /// <summary>
        /// 删除ini文件下所有段落
        /// </summary>
        public void ClearAllSection()
        {
            IniWriteValue(null, null, null);
        }

        /// <summary>
        /// 删除ini文件下personal段落下的所有键
        /// </summary>
        public void ClearSection(string Section)
        {
            IniWriteValue(Section, null, null);
        }

    }


}
