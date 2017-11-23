using Hiwjcn.Core.Infrastructure.User;
using Hiwjcn.Web.Models.Permission;
using Lib.mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebLogic.Bll.User;
using WebLogic.Model.User;
using Lib.helper;
using Lib.extension;

namespace WebApp.Areas.Admin.Controllers
{
    /// <summary>
    /// 权限角色相关
    /// </summary>
    public class PermissionController : WebCore.MvcLib.Controller.UserBaseController
    {
        private IUserService _IUserService { get; set; }
        private IRoleService _IRoleService { get; set; }
        public PermissionController(IUserService user, IRoleService role)
        {
            this._IUserService = user;
            this._IRoleService = role;
        }

        /// <summary>
        /// 角色列表
        /// </summary>
        /// <returns></returns>
        public ActionResult RoleList()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                ViewData["list"] = _IRoleService.GetAllRoles();
                return View();
            });
        }

        /// <summary>
        /// 角色编辑页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RoleEdit(string id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                if (ValidateHelper.IsPlumpString(id))
                {
                    var role = _IRoleService.GetByID(id);
                    ViewData["role"] = role;
                }
                return View();
            });
        }

        /// <summary>
        /// 保存或者更新角色
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <param name="auto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveRoleAction(int? id, string name, string desc, string auto)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new RoleModel();
                model.IID = id ?? 0;
                model.RoleName = name;
                model.RoleDescription = desc;
                model.AutoAssignRole = ConvertHelper.GetString(auto).ToBoolInt();

                var res = string.Empty;

                if (model.IID > 0)
                {
                    res = _IRoleService.UpdateRole(model);
                }
                else
                {
                    res = _IRoleService.AddRole(model);
                }

                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteRoleAction(string id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                string res = string.Empty;

                res = _IRoleService.DeleteRole(id);

                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 角色和权限mapping管理
        /// </summary>
        /// <returns></returns>
        public ActionResult RolePermission(string role_id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var role = _IRoleService.GetByID(role_id);
                if (role == null) { return new GoHomeResult(); }

                var model = new RolePermissionViewModel();
                model.Role = role;

                var rolepermissions = _IRoleService.GetRolePermissionsList(role.UID);
                if (rolepermissions == null) { rolepermissions = new List<string>(); }
                var allpermissions = PermissionRecord.GetAllPermission();
                if (allpermissions == null) { allpermissions = new List<PermissionRecord>(); }

                model.AssignedList = allpermissions.Where(x => rolepermissions.Contains(x.PermissionID)).ToList();
                model.UnAssignedList = allpermissions.Where(x => !rolepermissions.Contains(x.PermissionID)).ToList();

                ViewData["model"] = model;

                return View();
            });
        }

        /// <summary>
        /// 角色和权限mapping管理处理函数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RolePermissionAction(string action, string role_id, string id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new RolePermissionModel();
                model.RoleID = role_id;
                model.PermissionID = id;

                var bll = new RolePermissionBll();
                if (action == "add")
                {
                    return GetJsonRes(bll.AddRolePermission(model));
                }
                if (action == "del")
                {
                    return GetJsonRes(bll.DeleteRolePermission(model.RoleID, model.PermissionID));
                }
                return GetJsonRes("未知请求");
            });
        }

        /// <summary>
        /// 用户权限mapping管理
        /// </summary>
        /// <returns></returns>
        public ActionResult UserRole(string user_id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var user = _IUserService.GetByID(user_id);
                if (user == null) { return GoHome(); }

                var model = new UserRoleViewModel();

                model.User = user;

                UserRoleBll bll = new UserRoleBll() { };

                var userrolesid = bll.GetRolesByUserID(user.UID);
                if (userrolesid == null) { userrolesid = new List<string>(); }

                var allrolelist = _IRoleService.GetAllRoles();
                if (allrolelist == null) { allrolelist = new List<RoleModel>(); }

                model.AssignedList = allrolelist.Where(x => userrolesid.Contains(x.UID)).ToList();
                model.UnAssignedList = allrolelist.Where(x => !userrolesid.Contains(x.UID)).ToList();

                ViewData["model"] = model;

                return View();
            });
        }

        /// <summary>
        /// 用户权限mapping管理处理函数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserRoleAction(string action, string role_id, string user_id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new UserRoleModel();
                model.RoleID = role_id;
                model.UserID = user_id;

                var bll = new UserRoleBll();
                if (action == "add")
                {
                    return GetJsonRes(bll.AddUserRole(model));
                }
                if (action == "del")
                {
                    return GetJsonRes(bll.DeleteUserRole(model.UserID, model.RoleID));
                }
                return GetJsonRes("未知请求");
            });
        }

    }
}
