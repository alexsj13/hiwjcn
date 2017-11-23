using Bll.Sys;
using Hiwjcn.Core.Infrastructure.User;
using Hiwjcn.Web.Models.User;
using Lib.helper;
using Lib.core;
using Lib.io;
using Lib.mvc;
using Lib.mvc.user;
using Model.Sys;
using Model.User;
using System;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    public class UserController : WebCore.MvcLib.Controller.UserBaseController
    {
        private IUserService _IUserService { get; set; }
        private readonly LoginStatus _LoginStatus;
        public UserController(IUserService user, LoginStatus _LoginStatus)
        {
            this._IUserService = user;
            this._LoginStatus = _LoginStatus;
        }


        public ActionResult NoTest()
        {
            this._LoginStatus.SetUserLogin(loginuser: new LoginUserInfo()
            {
                IID = 99,
                UserID = Com.GetUUID(),
                UserUID = Com.GetUUID(),
                LoginToken = Com.GetUUID(),
                Email = "hiwjcn@live.com",
                IsActive = 1
            });
            return Content("");
        }

        [PageAuth(Permission = "不存在的权限")]
        public ActionResult No()
        {
            return Content("");
        }

        [ApiAuth(Permission = "测试用的权限")]
        public ActionResult NoApi()
        {
            return GetJson(new _() { });
        }

        public ActionResult AllPers()
        {
            var data = this.ScanAllAssignedPermissionOnThisAssembly();
            return GetJson(new { permissions = data.permissions, scopes = data.scopes });
        }

        /// <summary>
        /// 用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult UserInfo()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = _IUserService.GetByID(loginuser.UserID);
                if (model == null) { return Http404(); }
                ViewData["user"] = model;
                return View();
            });
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult InfoAction(string user_name, int? user_sex, string user_phone,
            string user_qq, string user_introduction)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new UserModel();
                model.IID = loginuser.IID;
                model.NickName = user_name;
                model.Sex = user_sex ?? (int)SexEnum.未知;
                model.Phone = user_phone;
                model.QQ = user_qq;
                model.Introduction = user_introduction;

                var res = _IUserService.UpdateUserInfo(model);
                if (!ValidateHelper.IsPlumpString(res))
                {
                    //
                }
                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 个人信息展示页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Me(string id)
        {
            return RunAction(() =>
            {
                id = id ?? this.X.LoginUser?.UserID;

                var user = _IUserService.GetByID(id);
                if (user == null) { return GoHome(); }

                var model = new MeViewModel();
                model.User = user;
                model.IsMe = model.User.IID == (this.X.LoginUser?.IID ?? 0);

                ViewData["model"] = model;

                return View();
            });
        }

        /// <summary>
        /// 更新用户头像
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateUserMask()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = _IUserService.GetByID(loginuser.UserID);
                ViewData["model"] = model;
                return View();
            });
        }

        [HttpPost]
        public ActionResult UpdateUserMaskAction()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var file = this.X.context.Request.Files["user_mask"];
                var save_path = this.X.context.Server.MapPath("~/static/upload/usermask/");
                var res = _IUserService.UpdateUserMask(loginuser.UserID, file, save_path);
                if (!ValidateHelper.IsPlumpString(res))
                {
                    return Redirect("/user/updateusermask/");
                }
                return Content("请求未能完成：" + res);
            });
        }

        /// <summary>
        /// 用户头像
        /// </summary>
        /// <returns></returns>
        [Route("User/UserMask/{id}/")]
        public ActionResult UserMask(string id)
        {
            return RunAction(() =>
            {
                var b = _IUserService.GetUserImage(id);
                if (!ValidateHelper.IsPlumpList(b))
                {
                    string deftImage = Server.MapPath("~/static/image/moren.png");
                    b = IOHelper.GetFileBytes(deftImage);
                }
                if (!ValidateHelper.IsPlumpList(b))
                {
                    return Content("空数据");
                }

                this.X.context.Response.SetResponseNoCache();

                return File(b, "Image/Png");
            });
        }

        /// <summary>
        /// 站内信
        /// </summary>
        /// <returns></returns>
        public ActionResult Msg()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var bll = new MessageBll();

                ViewData["list"] = bll.GetTopMessage(loginuser.UserID, 100);

                return View();
            });
        }

        public ActionResult MsgCount()
        {
            return Content("");
        }

        public ActionResult MsgShow()
        {
            return Content("");
        }

        /// <summary>
        /// 发送站内信处理请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendMessageAction(string to, string msg)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var bll = new MessageBll();

                var start = DateTime.Now.Date;
                var end = start.AddDays(1);
                int count = bll.GetSenderMessageCount(loginuser.UserID, start, end);
                if (count >= 30)
                {
                    return GetJsonRes("您今天发送条数达到上限");
                }

                var model = new MessageModel();
                model.MsgContent = msg;
                model.ReceiverUserID = to;
                model.SenderUserID = loginuser.UserID;
                model.UpdateTime = DateTime.Now;
                model.IsNew = (int)YesOrNoEnum.是;

                return GetJsonRes(bll.SendMessage(model));
            });
        }

        /// <summary>
        /// 发送站内信页面
        /// </summary>
        /// <returns></returns>
        public ActionResult SendMessage(string to)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                if (to == loginuser.UserID) { return Content("参数错误"); }
                var model = _IUserService.GetByID(to);
                if (model == null) { return Content("用户不存在"); }

                ViewData["model"] = model;

                return View();
            });
        }

    }
}
