using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Linq;

namespace Lib.helper
{
    /// <summary>
    /// 安装帮助类
    /// </summary>
    public static class SecureHelper
    {
        private static Encoding _encoding { get => Encoding.UTF8; }

        private static string BsToStr(byte[] bs) => string.Join(string.Empty, bs.Select(x => x.ToString("x2"))).Replace("-", string.Empty);

        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMD5(string str)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var bs = _encoding.GetBytes(str);
                bs = md5.ComputeHash(bs);
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// 获取md5
        /// </summary>
        /// <param name="_bs"></param>
        /// <returns></returns>
        public static string GetMD5(byte[] _bs)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var bs = md5.ComputeHash(_bs);
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// 读取文件MD5
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileMD5(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open))
            {
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    var bs = md5.ComputeHash(file);
                    return BsToStr(bs);
                }
            }
        }

        /// <summary>
        /// 获取sha1
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetSHA1(string str)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var bs = _encoding.GetBytes(str);
                bs = sha1.ComputeHash(bs);
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果</returns>
        public static string GetSHA256(string str)
        {
            using (var Sha256 = new SHA256CryptoServiceProvider())
            {
                var bs = _encoding.GetBytes(str);
                bs = Sha256.ComputeHash(bs);
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// 获取hmac md5
        /// </summary>
        /// <param name="str"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetHMACMD5(string str, string password)
        {
            using (var hmac_md5 = new HMACMD5())
            {
                hmac_md5.Key = _encoding.GetBytes(password);
                var bs = hmac_md5.ComputeHash(_encoding.GetBytes(str));
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// 获取hmac sha1
        /// </summary>
        /// <param name="str"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetHMACSHA1(string str, string password)
        {
            using (var hmac_sha1 = new HMACSHA1())
            {
                hmac_sha1.Key = _encoding.GetBytes(password);
                var bs = hmac_sha1.ComputeHash(_encoding.GetBytes(str));
                return BsToStr(bs);
            }
        }

        #region DES加密解密
        private static readonly string txtKey = "PatrickpanP=";
        private static readonly string txtIV = "LiuJineagel=";

        public static string DESEncrypt(string Text, string key) => DESEncrypt(Text, key, key);
        public static string DESDecrypt(string Text, string key) => DESDecrypt(Text, key, key);

        /// <summary>
        /// 加密数据
        /// </summary>
        public static string DESEncrypt(string Text, string txtKey, string txtIV)
        {
            using (var des = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray;
                inputByteArray = _encoding.GetBytes(Text);
                //des.Key = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
                //des.IV = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
                des.Key = Convert.FromBase64String(txtKey);
                des.IV = Convert.FromBase64String(txtIV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        StringBuilder ret = new StringBuilder();
                        foreach (byte b in ms.ToArray())
                        {
                            ret.AppendFormat("{0:X2}", b);
                        }
                        return ret.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        public static string DESDecrypt(string Text, string txtKey, string txtIV)
        {
            using (var des = new DESCryptoServiceProvider())
            {
                int len;
                len = Text.Length / 2;
                byte[] inputByteArray = new byte[len];
                int x, i;
                for (x = 0; x < len; x++)
                {
                    i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                    inputByteArray[x] = (byte)i;
                }
                //des.Key = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
                //des.IV = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
                des.Key = Convert.FromBase64String(txtKey);
                des.IV = Convert.FromBase64String(txtIV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        return _encoding.GetString(ms.ToArray());
                    }
                }
            }
        }

        #endregion

        //AES密钥向量
        private static readonly byte[] _aeskeys = new byte[] {
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD,
            0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF
        };

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="encryptStr">加密字符串</param>
        /// <param name="encryptKey">密钥</param>
        /// <returns></returns>
        [Obsolete("未测试")]
        public static string AESEncrypt(string encryptStr, string encryptKey)
        {
            if (string.IsNullOrWhiteSpace(encryptStr))
                return string.Empty;

            encryptKey = StringHelper.SubString(encryptKey, 32);
            encryptKey = encryptKey.PadRight(32, ' ');

            //分组加密算法
            using (SymmetricAlgorithm des = Rijndael.Create())
            {
                byte[] inputByteArray = _encoding.GetBytes(encryptStr);//得到需要加密的字节数组 
                                                                       //设置密钥及密钥向量
                des.Key = _encoding.GetBytes(encryptKey);
                des.IV = _aeskeys;
                byte[] cipherBytes = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cipherBytes = ms.ToArray();//得到加密后的字节数组
                    }
                }
                return Convert.ToBase64String(cipherBytes);
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="decryptStr">解密字符串</param>
        /// <param name="decryptKey">密钥</param>
        /// <returns></returns>
        [Obsolete("未测试")]
        public static string AESDecrypt(string decryptStr, string decryptKey)
        {
            if (string.IsNullOrWhiteSpace(decryptStr))
                return string.Empty;

            decryptKey = StringHelper.SubString(decryptKey, 32);
            decryptKey = decryptKey.PadRight(32, ' ');

            byte[] cipherText = Convert.FromBase64String(decryptStr);

            using (SymmetricAlgorithm des = Rijndael.Create())
            {
                des.Key = _encoding.GetBytes(decryptKey);
                des.IV = _aeskeys;
                byte[] decryptBytes = new byte[cipherText.Length];
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        cs.Read(decryptBytes, 0, decryptBytes.Length);
                    }
                }
                return _encoding.GetString(decryptBytes).Replace("\0", "");//将字符串后尾的'\0'去掉}
            }

        }
    }
}
