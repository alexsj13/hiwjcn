using Qiniu.Http;
using Qiniu.IO;
using Qiniu.IO.Model;
using Qiniu.RS;
using Qiniu.RS.Model;
using Qiniu.Util;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Data.Common;
using System.Web;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Data;

namespace Lib.storage
{
    public static class QiniuExtension
    {
        /// <summary>
        /// 文件存在
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool HasFile(this StatResult status)
        {
            return status.IsOk() &&
                status.Result != null &&
                status.Result.Fsize > 0 &&
                status.Result.Hash?.Length > 0;
        }

        /// <summary>
        /// 是否是200
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static bool IsOk(this HttpResult res) => res.Code == 200;

        /// <summary>
        /// 有异常就抛出
        /// </summary>
        /// <param name="res"></param>
        public static void ThrowIfException(this HttpResult res)
        {
            if (!res.IsOk())
            {
                throw new Exception($"七牛服务器错误，返回数据：{JsonConvert.SerializeObject(res)}");
            }
        }
    }

    /// <summary>
    /// http://developer.qiniu.com/resource/official.html#sdk
    /// </summary>
    public static class QiniuHelper
    {
        public static readonly string AK = ConfigurationManager.AppSettings["QiniuAccessKey"];
        public static readonly string SK = ConfigurationManager.AppSettings["QiniuSecretKey"];
        public static readonly string bucket = ConfigurationManager.AppSettings["QiniuBucketName"];
        public static readonly string BaseUrl = ConfigurationManager.AppSettings["QiniuBaseUrl"];

        /// <summary>
        /// 获取七牛的文件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static StatResult FindEntry(string key)
        {
            var mac = new Mac(AK, SK);
            var bm = new BucketManager(mac);
            // 返回结果存储在result中
            var res = bm.Stat(bucket, key);
            res.ThrowIfException();
            return res;
        }

        /// <summary>
        /// 删除七牛的文件
        /// </summary>
        /// <param name="key"></param>
        public static void Delete(string key)
        {
            var mac = new Mac(AK, SK);
            var bm = new BucketManager(mac);
            // 返回结果存储在result中
            var res = bm.Delete(bucket, key);
            res.ThrowIfException();
        }


        /// <summary>
        /// 创建上传token
        /// </summary>
        /// <returns></returns>
        private static string CreateUploadToken()
        {
            // 上传策略
            var putPolicy = new PutPolicy();
            // 设置要上传的目标空间
            putPolicy.Scope = bucket;
            putPolicy.SetExpires(3600);
            var mac = new Mac(AK, SK);
            // 生成上传凭证
            var uploadToken = new Qiniu.Util.Auth(mac).CreateUploadToken(putPolicy.ToJsonString());
            return uploadToken;
        }

        /// <summary>
        /// 上传文件到qiniu，返回访问链接
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string Upload(string localFile, string saveKey)
        {
            // 开始上传文件
            var um = new UploadManager();
            var res = um.UploadFile(localFile, saveKey, CreateUploadToken());
            res.ThrowIfException();
            return GetUrl(saveKey);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="saveKey"></param>
        /// <returns></returns>
        public static string Upload(byte[] bs, string saveKey)
        {
            // 开始上传文件
            var um = new UploadManager();
            var res = um.UploadData(bs, saveKey, CreateUploadToken());
            res.ThrowIfException();
            return GetUrl(saveKey);
        }

        /// <summary>
        /// 获取文件地址http://www.domain.com/file-qiniu-key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetUrl(string key)
        {
            return BaseUrl + key;
        }

    }

    public class UpFile
    {
        public int UpID { get; set; }
        public string UserID { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string FileExt { get; set; }
        public string FilePath { get; set; }
        public string FileUrl { get; set; }
        public string FileMD5 { get; set; }
        public System.DateTime UpTime { get; set; }
    }

    public enum UploadType : int
    {
        Product,
        QRCode,
        Promotion,
        ProductDetail,
        User
    }

    public static class HttpPostedFileExtension
    {
        public static string UploadTypeStr(UploadType tp)
        {
            switch (tp)
            {
                case UploadType.Product: return nameof(UploadType.Product);
                case UploadType.ProductDetail: return nameof(UploadType.ProductDetail);
                case UploadType.Promotion: return nameof(UploadType.Promotion);
                case UploadType.QRCode: return nameof(UploadType.QRCode);
                case UploadType.User: return nameof(UploadType.User);
                default: return "default";
            }
        }

        public static byte[] GetPostFileBytesAndDispose(HttpPostedFile postFile)
        {
            if (postFile.ContentLength <= 0) { throw new Exception("上传文件为空"); }
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

        public static (int width, int height) GetShape(byte[] bs)
        {
            using (var ms = new MemoryStream(bs))
            {
                using (var img = Image.FromStream(ms))
                {
                    return (img.Width, img.Height);
                }
            }
        }

        public static bool IsImage(string extension) =>
            new string[] { ".png", ".jpg", ".gif", ".bmp", ".jpeg" }.Select(x => x.ToLower()).Contains(extension.ToLower());

        private static string BsToStr(byte[] bs) => string.Join(string.Empty, bs.Select(x => x.ToString("x2"))).Replace("-", string.Empty);

        public static string GetMD5(byte[] _bs)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var bs = md5.ComputeHash(_bs);
                return BsToStr(bs);
            }
        }

        private static readonly string con_str =
            ConfigurationManager.AppSettings["db"] ??
            ConfigurationManager.ConnectionStrings["db"]?.ConnectionString ??
            ConfigurationManager.AppSettings["MsSqlConnectionString"] ??
            ConfigurationManager.ConnectionStrings["MsSqlConnectionString"]?.ConnectionString ??
            throw new Exception("七牛上传需要配置数据库连接字符串");

        private static IDbConnection GetCon()
        {
            var con = new SqlConnection(con_str);
            con.Open();
            return con;
        }

        public static string Wrapper(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }
            bool has_prefix(string url)
            {
                var lower = url.ToLower();
                return lower.StartsWith("http://") || lower.StartsWith("https://");
            }
            if (!has_prefix(data))
            {
                data = QiniuHelper.BaseUrl + data;
            }
            return data;
        }

        public static (List<UpFile> filelist, int total) FindFileList(string user_id, int start, int size)
        {
            using (var con = GetCon())
            {
                var p = new { uid = user_id, skip = start, take = size };
                var sql = "select {0} from parties.dbo.UpFile where UserID=@uid";
                var total = con.ExecuteScalar<int?>(string.Format(sql, "count(1)"), p) ?? 0;
                var list_sql = string.Format(sql, "*") + " order by UpTime desc OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
                var list = con.Query<UpFile>(list_sql, p).Where(x => !string.IsNullOrEmpty(x.FileUrl)).ToList();

                foreach (var m in list)
                {
                    m.FileUrl = Wrapper(m.FileUrl);
                }
                return (list, total);
            }
        }

        private static UpFile FindFileInDB(string md5)
        {
            using (var con = GetCon())
            {
                var sql = "select * from parties.dbo.UpFile where FileMD5=@uid";
                var file = con.Query<UpFile>(sql, new { uid = md5 }).FirstOrDefault();
                return file;
            }
        }

        private static void DeleteFile(string md5)
        {
            using (var con = GetCon())
            {
                var sql = "delete from parties.dbo.UpFile where FileMD5=@uid";
                con.Execute(sql, new { uid = md5 });
            }
        }

        private static void SaveFile(UpFile file)
        {
            using (var con = GetCon())
            {
                var sql = "insert into parties.dbo.UpFile (UserID,FileName,FileSize,FileExt,FilePath,FileUrl,FileMD5,UpTime) values (@UserID,@FileName,@FileSize,@FileExt,@FilePath,@FileUrl,@FileMD5,@UpTime)";
                con.Execute(sql, file);
            }
        }

        public static (string url, string msg) UploadToQiniuAndSaveInDB(this HttpPostedFile file,
            string user_id, UploadType upload_type,
            int? max_size = null, string[] allowed_extension = null)
        {
            if (string.IsNullOrEmpty(user_id)) { throw new Exception("上传用户不能为空"); }
            var file_name = file.FileName;
            var extension = Path.GetExtension(file_name);
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }
            if (allowed_extension?.Length > 0 && (!allowed_extension.Any(x => x.ToLower() == extension.ToLower())))
            {
                return (null, $"{extension}不允许上传");
            }

            var bs = GetPostFileBytesAndDispose(file);
            var md5 = GetMD5(bs).ToLower();

            var guid_name = md5;
            if (IsImage(extension))
            {
                var size = GetShape(bs);
                guid_name = $"{guid_name}_w{size.width}_h{size.height}";
            }
            guid_name = $"{UploadTypeStr(upload_type)}/{DateTime.Now.ToString("yyyyMMdd")}/{guid_name}{extension}";
            guid_name = guid_name.ToLower();

            var db_file = FindFileInDB(md5);

            if (db_file != null)
            {
                if (QiniuHelper.FindEntry(db_file.FileUrl).IsOk())
                {
                    return (Wrapper(db_file.FileUrl), null);
                }
                else
                {
                    DeleteFile(db_file.FileMD5);
                }
            }
            //upload new file and save to db
            var url = QiniuHelper.Upload(bs, guid_name);
            //save to db
            var dbfile = new UpFile()
            {
                UserID = user_id,
                FileName = file_name,
                FileSize = file.ContentLength,
                FileExt = extension,
                FilePath = "/nopath",
                FileUrl = guid_name,
                FileMD5 = md5,
                UpTime = DateTime.Now
            };

            SaveFile(dbfile);
            return (url, null);
        }
    }
}
