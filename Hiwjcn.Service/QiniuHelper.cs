using Lib.core;
using Lib.extension;
using Lib.helper;
using Qiniu.Http;
using Qiniu.IO;
using Qiniu.IO.Model;
using Qiniu.RS;
using Qiniu.RS.Model;
using Qiniu.Util;
using System;

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
                ValidateHelper.IsPlumpString(status.Result.Hash);
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
                throw new Exception($"七牛服务器错误，返回数据：{res.ToJson()}");
            }
        }
    }

    /// <summary>
    /// http://developer.qiniu.com/resource/official.html#sdk
    /// </summary>
    public static class QiniuHelper
    {
        private static readonly string AK = ConfigHelper.Instance.QiniuAccessKey;
        private static readonly string SK = ConfigHelper.Instance.QiniuSecretKey;
        private static readonly string bucket = ConfigHelper.Instance.QiniuBucketName;
        private static readonly string BaseUrl = ConfigHelper.Instance.QiniuBaseUrl;

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
}
