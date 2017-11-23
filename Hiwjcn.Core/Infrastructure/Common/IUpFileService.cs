using Hiwjcn.Core.Model.Sys;
using Lib.infrastructure;
using Lib.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Infrastructure.Common
{
    public interface IUpFileService : IServiceBase<UpFileModel>
    {
        /// <summary>
        /// 添加文件记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string AddFile(UpFileModel model);

        /// <summary>
        /// 查找文件，给ueditor提供数据
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="filelist"></param>
        /// <param name="filecount"></param>
        void FindFiles(string uid, int start, int size, ref string[] filelist, ref int filecount);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        string DeleteFile(string fid, string uid);

        string UploadFileAfterCheckRepeat(FileInfo file, string uid,
           ref string file_url, ref string file_name, bool DeleteFileAfterUploadToQiniu = true);

        /// <summary>
        /// 可能抛异常
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        string ReplaceHtmlImageUrl(string html);
    }
}
