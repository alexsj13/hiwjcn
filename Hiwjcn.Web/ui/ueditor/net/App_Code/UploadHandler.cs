using Hiwjcn.Core.Infrastructure.Common;
using Lib.helper;
using Lib.io;
using Lib.ioc;
using Lib.mvc;
using Lib.mvc.user;
using System.IO;
using System.Linq;
using System.Web;

/// <summary>
/// UploadHandler 的摘要说明
/// </summary>
public class UploadHandler : Handler
{
    public UploadResult Result { get; private set; }

    public UploadHandler(HttpContext context)
        : base(context)
    {
        this.Result = new UploadResult() { State = "位置错误" };
    }

    public override void Process()
    {
        AppContext.Scope(s =>
        {
            var logincontext = s.Resolve_<LoginStatus>();

            var loginuser = logincontext.GetLoginUser(Context);
            if (loginuser == null)
            {
                Result.State = "没有登陆";
                WriteResult();
                return true;
            }

            #region 上传到本地
            string SavePath = ServerHelper.GetMapPath(Context, "~/static/upload/editor/");
            var uploader = new FileUpload();
            uploader.AllowFileType = new string[] { ".gif", ".png", ".jpg", ".jpeg" };
            uploader.MaxSize = Com.MbToB(1);
            var filelist = Context.Request.Files.AllKeys.Select(x => Context.Request.Files[x]).ToList();
            if (!ValidateHelper.IsPlumpList(filelist))
            {
                Result.State = "获取不到文件";
                WriteResult();
                return true;
            }
            var model = uploader.UploadSingleFile(filelist[0], SavePath);
            if (!model.SuccessUpload)
            {
                Result.State = model.Info;
                WriteResult();
                return true;
            }
            if (!File.Exists(model.FilePath))
            {
                Result.State = "本地文件丢失";
                WriteResult();
                return true;
            }
            #endregion
            string file_url = string.Empty;
            string file_name = string.Empty;
            var upfileservice = s.Resolve_<IUpFileService>();
            var res = upfileservice.UploadFileAfterCheckRepeat(new FileInfo(model.FilePath), loginuser.UserID, ref file_url, ref file_name);
            if (ValidateHelper.IsPlumpString(res))
            {
                Result.State = res;
                WriteResult();
                return true;
            }

            Result.Url = file_url;
            Result.OriginFileName = file_name;
            Result.State = "SUCCESS";
            WriteResult();
            return true;
        });
    }

    private void WriteResult()
    {
        this.WriteJson(new
        {
            state = Result.State,
            url = Result.Url,
            title = Result.OriginFileName,
            original = Result.OriginFileName,
            error = Result.ErrorMessage
        });
    }
}

public class UploadResult
{
    public string State { get; set; }
    public string Url { get; set; }
    public string OriginFileName { get; set; }
    public string ErrorMessage { get; set; }
}

