using Lib.core;
using Lib.helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lib.mvc
{
    public static class ResultHelper
    {
        public static ActionResult BadRequest(string msg, object data = null)
        {
            return new CustomJsonResult()
            {
                Data = new _() { success = false, msg = msg, data = data },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }

    /// <summary>
    /// 通用返回错误代码
    /// </summary>
    public enum ErrorCodeEnum : int
    {
        未登录 = -1,
        没有权限 = -2,
        跳转SSO = -3,
        账户未验证 = -4,
        账户被冻结 = -5
    }

    /// <summary>
    /// 接口公共返回值的缩写
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class _ : ResJson { }

    /// <summary>
    /// 接口公共返回值的缩写
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class _<T> : ResJson<T> { }

    /// <summary>
    /// 通用返回
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ResJson : ResJson<object> { }

    /// <summary>
    /// json格式
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ResJson<T> : ResultMsg<T>
    {
        [DataMember]
        [JsonIgnore]
        public virtual bool error
        {
            get => !this.success;
            set => this.success = !value;
        }

        [DataMember]
        [JsonIgnore]
        public virtual bool success
        {
            get => this.Success;
            set => this.Success = value;
        }

        [DataMember]
        [JsonIgnore]
        public virtual string msg
        {
            get { return this.ErrorMsg; }
            set { this.ErrorMsg = value; }
        }

        [DataMember]
        [JsonIgnore]
        public virtual T data
        {
            get { return this.Data; }
            set { this.Data = value; }
        }

        [DataMember]
        [JsonIgnore]
        public virtual string code
        {
            get { return this.ErrorCode; }
            set { this.ErrorCode = value; }
        }
    }

    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ResultMsg : ResultMsg<object> { }

    /// <summary>
    /// 汽配龙的model
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ResultMsg<T>
    {
        public void ThrowIfNotSuccess()
        {
            if (!this.Success)
            {
                throw new Exception(this.ErrorMsg ?? $"{nameof(ResultMsg)}默认错误信息");
            }
        }

        public void SetSuccessData(T data)
        {
            this.Data = data;
            this.Success = true;
        }

        public void SetErrorMsg(string msg)
        {
            if (!ValidateHelper.IsPlumpString(msg)) { throw new Exception("错误信息不能为空"); }
            this.ErrorMsg = msg;
            this.Success = false;
        }

        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserToken { get; set; }

        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SafeCode { get; set; }

        [DataMember]
        public bool Success { get; set; } = false;

        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember]
        public string ErrorMsg { get; set; }

        [DataMember]
        public T Data { get; set; }

        [DataMember]
        public int Total { get; set; }

        [Obsolete("使用没有参数的构造函数")]
        public ResultMsg(T data, bool success, string errorCode, string errorMsg, string userToken)
        {
            this.Data = data;
            this.Success = success;
            this.ErrorMsg = errorMsg;
            this.ErrorCode = errorCode;
            this.UserToken = userToken;
        }

        [Obsolete("使用没有参数的构造函数")]
        public ResultMsg(bool success, string errorCode, string errorMsg, string userToken)
        {
            this.Success = success;
            this.ErrorMsg = errorMsg;
            this.ErrorCode = errorCode;
            this.UserToken = userToken;
        }

        [Obsolete("使用没有参数的构造函数")]
        public ResultMsg(bool success, string errorCode, string errorMsg)
        {
            this.Success = success;
            this.ErrorMsg = errorMsg;
            this.ErrorCode = errorCode;
        }

        public ResultMsg() { }
    }

    /// <summary>
    /// 解决mvc中json丢时区的问题
    /// </summary>
    public class CustomJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var response = context.HttpContext.Response;

            if (ValidateHelper.IsPlumpString(this.ContentType))
            {
                response.ContentType = this.ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }

            if (this.ContentEncoding != null)
            {
                response.ContentEncoding = this.ContentEncoding;
            }

            if (this.Data != null)
            {
                var json = JsonHelper.ObjectToJson(Data);
                response.Write(json);
            }
        }
    }

    /// <summary>
    /// 登录
    /// </summary>
    public class LoginResult : ActionResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            string url = ConvertHelper.GetString(context.HttpContext.Request.Url);
            url = EncodingHelper.UrlEncode(url);
            context.HttpContext.Response.Redirect("/account/login/?next=" + url);
            context.HttpContext.ApplicationInstance.CompleteRequest();
        }
    }

    /// <summary>
    /// 返回首页
    /// </summary>
    public class GoHomeResult : ActionResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Redirect("/");
            context.HttpContext.ApplicationInstance.CompleteRequest();
        }
    }

    /// <summary>
    /// 301永久跳转
    /// </summary>
    public class Http301 : ActionResult
    {
        public string Url { get; set; }

        public Http301(string url)
        {
            this.Url = url;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.BufferOutput = true;
            context.HttpContext.Response.Status = "301 Moved Permanently";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.MovedPermanently;
            context.HttpContext.Response.RedirectLocation = this.Url;
            //context.HttpContext.Response.End();
            context.HttpContext.ApplicationInstance.CompleteRequest();
        }
    }

    /// <summary>
    /// 无权限
    /// </summary>
    public class Http403 : ActionResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.BufferOutput = true;
            context.HttpContext.Response.StatusDescription = "Forbidden";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.HttpContext.Server.Transfer("~/ErrorPage/403.html");
            //context.HttpContext.Response.End();
            context.HttpContext.ApplicationInstance.CompleteRequest();
        }
    }

    /// <summary>
    /// 页面找不到
    /// </summary>
    public class Http404 : ActionResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.BufferOutput = true;
            context.HttpContext.Response.StatusDescription = "404 Not Found";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.HttpContext.Server.Transfer("~/ErrorPage/404.html");
            //context.HttpContext.Response.End();
            context.HttpContext.ApplicationInstance.CompleteRequest();
        }
    }

    /// <summary>
    /// 服务器内部错误
    /// </summary>
    public class Http500 : ActionResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.BufferOutput = true;
            context.HttpContext.Response.StatusDescription = "Internal Server Error";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.HttpContext.Server.Transfer("~/ErrorPage/500.html");
            //context.HttpContext.Response.End();
            context.HttpContext.ApplicationInstance.CompleteRequest();
        }
    }
}
