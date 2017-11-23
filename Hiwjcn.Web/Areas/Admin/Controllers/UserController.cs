using Hiwjcn.Core.Infrastructure.User;
using Lib.helper;
using System.Web.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [ValidateInput(false)]
    public class UserController : WebCore.MvcLib.Controller.UserBaseController
    {
        private IUserService _IUserService { get; set; }
        public UserController(IUserService user)
        {
            this._IUserService = user;
        }
        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult UserList(int? page, string q)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                page = Com.CheckPage(page);
                int pageSize = 16;
                ViewData["q"] = q;
                var data = _IUserService.GetPagerList(keywords: q, page: page.Value, pageSize: pageSize,
                    LoadRoleAndPrivilege: true);
                if (data != null)
                {
                    ViewData["list"] = data.DataList;

                    ViewData["pager"] = data.GetPagerHtml(this, "page", page.Value, pageSize);
                }
                return View();
            });
        }
    }
}
