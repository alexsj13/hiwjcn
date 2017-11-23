using Bll;
using Lib.data;
using Hiwjcn.Core.Model.Sys;
using Hiwjcn.Dal.Sys;
using Lib.core;
using Lib.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Common;
using HtmlAgilityPack;
using Lib.helper;
using Lib.infrastructure;
using Lib.storage;

namespace Hiwjcn.Bll.Sys
{
    public class UpFileBll : ServiceBase<UpFileModel>, IUpFileService
    {
        public UpFileBll()
        {
            //
        }
        
        /// <summary>
        /// 添加文件记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddFile(UpFileModel model)
        {
            var err = CheckModel(model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            //如果数据库中有就直接返回成功
            var dal = new UpFileDal();
            if (dal.Exist(x => x.FileMD5 == model.FileMD5 && x.UserID == model.UserID)) { return SUCCESS; }

            model.CreateTime = DateTime.Now;
            return dal.Add(model) > 0 ? SUCCESS : "添加文件失败";
        }

        /// <summary>
        /// 查找文件，给ueditor提供数据
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="filelist"></param>
        /// <param name="filecount"></param>
        public void FindFiles(string uid, int start, int size, ref string[] filelist, ref int filecount)
        {
            if (start < 0 || size < 1) { return; }
            int count = 0;
            string[] list = null;
            new UpFileDal().PrepareIQueryable(query =>
            {
                query = query.Where(x => x.UserID == uid);
                count = query.Count();
                list = query.OrderByDescending(x => x.CreateTime).Skip(start).Take(size).Select(x => x.FileUrl).Distinct().ToArray();
                return true;
            });
            filecount = count;
            filelist = list;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string DeleteFile(string fid, string uid)
        {
            var dal = new UpFileDal();
            var model = dal.GetFirst(x => x.UID == fid && x.UserID == uid);
            if (model == null) { return "数据不存在"; }
            var md5 = model.FileMD5;
            if (dal.Delete(model) > 0)
            {
                if (!dal.Exist(x => x.FileMD5 == md5))
                {
                    QiniuHelper.Delete(md5);
                }
            }
            return "删除失败";
        }

        public string UploadFileAfterCheckRepeat(FileInfo file, string uid,
            ref string file_url, ref string file_name, bool DeleteFileAfterUploadToQiniu = true)
        {
            try
            {
                if (!file.Exists)
                {
                    throw new Exception("无法在磁盘上找到文件");
                }

                var dal = new UpFileDal();

                var dbmodel = new UpFileModel();
                dbmodel.UserID = uid;
                dbmodel.FileName = file.Name;
                dbmodel.FileExt = file.Extension;
                dbmodel.FileSize = (int)file.Length;
                dbmodel.FilePath = file.FullName;
                dbmodel.CreateTime = DateTime.Now;
                //获取文件md5值
                dbmodel.FileMD5 = SecureHelper.GetFileMD5(dbmodel.FilePath);
                if (!ValidateHelper.IsPlumpString(dbmodel.FileMD5))
                {
                    throw new Exception("获取文件MD5失败");
                }
                //判断文件是否存在于七牛
                var qiniu_file = QiniuHelper.FindEntry(dbmodel.FileMD5);
                bool FindInQiniu = qiniu_file.HasFile();
                bool uploadToQiniuByMe = false;
                if (FindInQiniu)
                {
                    //直接拿七牛中的文件地址
                    dbmodel.FileUrl = QiniuHelper.GetUrl(dbmodel.FileMD5);
                }
                else
                {
                    var url = QiniuHelper.Upload(file.FullName, dbmodel.FileMD5);
                    dbmodel.FileUrl = url;
                    //标记文件是我上传到七牛的
                    uploadToQiniuByMe = true;
                }
                //运行到这里，七牛已经有文件了
                //判断是否要添加到数据库
                var res = AddFile(dbmodel);
                if (ValidateHelper.IsPlumpString(res))
                {
                    //如果是我上传到七牛的并且保存本地数据库失败就删除
                    if (uploadToQiniuByMe)
                    {
                        QiniuHelper.Delete(dbmodel.FileMD5);
                    }
                    return "保存到数据库失败";
                }

                file_name = dbmodel.FileName;
                file_url = dbmodel.FileUrl;

                return SUCCESS;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (DeleteFileAfterUploadToQiniu) { file.Delete(); }
            }
        }

        /// <summary>
        /// 可能抛异常
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string ReplaceHtmlImageUrl(string html)
        {
            var dom = new HtmlDocument();
            dom.LoadHtml(html);
            var imgs = dom.DocumentNode.Descendants().Where(x => x?.Name?.ToLower() == "a").ToList();
            foreach (var img in imgs)
            {
                //
            }
            return dom.DocumentNode.InnerHtml;
        }
    }
}
