using Dal.User;
using Hiwjcn.Core.Infrastructure.Common;
using Hiwjcn.Core.Infrastructure.User;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.io;
using Lib.net;
using Model.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebLogic.Dal.User;
using WebLogic.Model.User;
using Lib.ioc;
using Lib.infrastructure;
using Lib.mvc.user;
using Lib.infrastructure.entity;

namespace Bll.User
{
    /// <summary>
    /// 用户
    /// </summary>
    public class UserBll : ServiceBase<UserModel>, IUserService
    {
        private UserDal _UserDal { get; set; }
        private UserRoleDal _UserRoleDal { get; set; }
        private RoleDal _RoleDal { get; set; }
        private RolePermissionDal _RolePermissionDal { get; set; }

        /// <summary>
        /// 用户逻辑类
        /// </summary>
        public UserBll()
        {
            this._UserDal = new UserDal();
            this._UserRoleDal = new UserRoleDal();
            this._RoleDal = new RoleDal();
            this._RolePermissionDal = new RolePermissionDal();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagerData<UserModel> GetPagerList(
            string name = null, int? sex = null, string email = null, string keywords = null,
            bool LoadRoleAndPrivilege = false,
            int page = 1, int pageSize = 20)
        {

            var data = new PagerData<UserModel>();

            #region 查询
            _UserDal.PrepareIQueryable((query) =>
            {
                if (ValidateHelper.IsPlumpString(name))
                {
                    query = query.Where(x => x.NickName == name);
                    data.UrlParams["name"] = name;
                }

                if (sex != null)
                {
                    query = query.Where(x => x.Sex == sex);
                    data.UrlParams["sex"] = name;
                }

                if (ValidateHelper.IsPlumpString(email))
                {
                    query = query.Where(x => x.Email == email);
                    data.UrlParams["email"] = name;
                }

                if (ValidateHelper.IsPlumpString(keywords))
                {
                    query = query.Where(x =>
                        x.NickName.Contains(keywords)
                        || x.Phone.Contains(keywords)
                        || x.Email.Contains(keywords)
                        || x.Introduction.Contains(keywords)
                        || x.QQ.Contains(keywords));
                    data.UrlParams["q"] = keywords;
                }

                data.ItemCount = query.Count();
                data.DataList = query.OrderByDescending(x => x.IID).QueryPage(page, pageSize).ToList();
                return true;
            });
            #endregion

            if (ValidateHelper.IsPlumpList(data?.DataList) && LoadRoleAndPrivilege)
            {
                data.DataList = GetRolesForUserList(data.DataList);
            }

            return data;
        }

        /// <summary>
        /// 从数据库里读取用户头像
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public byte[] GetUserImage(string userID)
        {
            var b = _UserDal.ReadUserImage(userID);
            //最好在这里压缩一下
            return b;
        }

        /// <summary>
        /// 通过id获取user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public UserModel GetByID(string userID)
        {
            return this._UserDal.GetFirst(x => x.UID == userID);
        }

        /// <summary>
        /// 用户数按照性别分组统计
        /// </summary>
        /// <returns></returns>
        public List<UserCountGroupBySex> GetCountGroupBySex()
        {
            List<UserCountGroupBySex> data = null;
            _UserDal.PrepareIQueryable((query) =>
            {
                data = query.GroupBy(x => x.Sex)
                    .Select(x => new UserCountGroupBySex() { Sex = x.Key, Count = x.Count() })
                    .OrderBy(x => x.Sex).Skip(0).Take(4).ToList();
                return true;
            });
            return data;
        }

        /// <summary>
        /// 更新用户头像
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="file"></param>
        /// <param name="MaxSize"></param>
        /// <returns></returns>
        public string UpdateUserMask(string userID, HttpPostedFile file, string save_path)
        {
            if (file == null || file.InputStream == null)
            {
                return "读取不到文件";
            }
            if (!Directory.Exists(save_path))
            {
                return "存储目录不存在";
            }

            var uploader = new FileUpload();
            uploader.AllowFileType = new string[] { ".gif", ".png", ".jpg", ".jpeg" };
            uploader.MaxSize = Com.MbToB(0.5f);
            var model = uploader.UploadSingleFile(file, save_path);

            if (!model.SuccessUpload || !File.Exists(model.FilePath)) { return model.Info; }

            var b = IOHelper.GetFileBytes(model.FilePath);
            if (!ValidateHelper.IsPlumpList(b))
            {
                return "本地文件丢失";
            }

            var errmsg = new List<string>();

            #region 把文件保存到数据库
            if (_UserDal.UpdateUserMask(userID, b) <= 0) { errmsg.Add("保存字节数组失败"); }
            #endregion

            #region 把文件保存到云存储
            var file_url = string.Empty;
            var file_name = string.Empty;
            return SUCCESS;
            #endregion
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="nick_name"></param>
        /// <param name="sex"></param>
        /// <param name="phone"></param>
        /// <param name="qq"></param>
        /// <param name="introduction"></param>
        /// <returns></returns>
        public string UpdateUserInfo(UserModel updateModel)
        {
            var model = _UserDal.GetByKeys(updateModel.IID);
            if (model == null) { return "用户不存在"; }

            model.NickName = updateModel.NickName;
            model.Sex = updateModel.Sex;
            model.Phone = updateModel.Phone;
            model.QQ = updateModel.QQ;
            model.Introduction = updateModel.Introduction;

            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo))
            {
                return errinfo;
            }

            return _UserDal.Update(model) > 0 ? SUCCESS : "更新失败";
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string UpdateUserPass(string userID, string old_pass, string new_pass, string re_new_pass)
        {
            if (!ValidateHelper.IsAllPlumpString(old_pass, new_pass)) { return "请输入旧密码和新密码"; }
            if (new_pass != re_new_pass) { return "两次输入的新密码不相同"; }

            string md5pass = UserPassWordEncrypt(old_pass);
            var model = _UserDal.GetFirst(x => x.UID == userID && x.PassWord == md5pass);
            if (model == null) { return "用户不存在"; }
            model.PassWord = UserPassWordEncrypt(new_pass);

            if (!this.CheckModel(model, out var msg))
            {
                return msg;
            }

            return _UserDal.Update(model) > 0 ? SUCCESS : "修改密码失败";
        }

        /// <summary>
        /// 加密用户密码
        /// </summary>
        /// <param name="originalPWD"></param>
        /// <returns></returns>
        private string UserPassWordEncrypt(string originalPWD)
        {
            return originalPWD.ToMD5().ToLower();
        }

        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sms"></param>
        /// <returns></returns>
        public string CreateToken(UserModel model)
        {
            return Com.GetPassKey($"{model.IID}/{model.UID}/{model.PassWord}");
        }

        private UserModel FindValidLoginUser(string email, ref string msg)
        {
            var loginemail = ConvertHelper.GetString(email).Trim().ToLower();
            var model = _UserDal.GetFirst(x => x.Email == loginemail);
            if (model == null)
            {
                msg = "此用户不存在";
                return null;
            }

            if (model.Flag < 0)
            {
                msg = "您的账户被冻结，请联系管理员";
                return null;
            }

            //获取用户权限
            model = LoadAllPermissionForUser(model);
            //生成token
            model.UserToken = CreateToken(model);
            return model;
        }

        /// <summary>
        /// 登录账户并记录登录状态
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pass"></param>
        /// <param name="autoLogin"></param>
        /// <returns></returns>
        public UserModel LoginByPassWord(string email, string pass, ref string msg)
        {
            if (!ValidateHelper.IsLenInRange(email, 5, 50) || !ValidateHelper.IsLenInRange(pass, 5, 50))
            {
                msg = "请输入正确的账户密码";
                return null;
            }
            var loginuser = FindValidLoginUser(email, ref msg);
            if (loginuser != null)
            {
                if (loginuser.PassWord != UserPassWordEncrypt(pass))
                {
                    msg = "密码错误";
                    return null;
                }
            }
            return loginuser;
        }

        /// <summary>
        /// 通过token找到用户
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public UserModel LoginByToken(string email, string token)
        {
            var msg = string.Empty;
            var loginuser = FindValidLoginUser(email, ref msg);
            if (loginuser != null)
            {
                if (loginuser.UserToken != token)
                {
                    msg = "token错误";
                    return null;
                }
            }
            return loginuser;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="model"></param>
        /// <param name="web_name"></param>
        /// <returns></returns>
        public string Register(UserModel model, string web_name = null)
        {
            model.UID = Com.GetUUID();
            model.Email = ConvertHelper.GetString(model.Email).Trim().ToLower();
            model.PassWord = ConvertHelper.GetString(model.PassWord).Trim();

            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo)) { return errinfo; }

            //检查用户是否存在
            if (_UserDal.Exist(x => x.Email == model.Email))
            {
                return "您输入的邮箱已经存在";
            }
            //添加用户
            if (_UserDal.Add(model) <= 0)
            {
                return "注册失败";
            }
            //保险起见，读取一下刚刚写入的用户信息
            model = _UserDal.GetFirst(x => x.Email == model.Email);
            if (model == null)
            {
                return "注册验证失败";
            }
            //发送邮件
            errinfo = SendEmail(model, web_name);
            if (ValidateHelper.IsPlumpString(errinfo))
            {
                return errinfo;
            }
            return SUCCESS;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="web_name"></param>
        /// <returns></returns>
        private string SendEmail(UserModel model, string web_name)
        {
            try
            {
                var mail = new EmailModel();
                mail.ToList = new List<string>() { model.Email };
                mail.Subject = string.Format("恭喜你注册成功，初始登陆帐户：{0}", model.Email);
                if (ValidateHelper.IsPlumpString(web_name))
                {
                    mail.Subject = string.Format("{0}提示：", web_name) + mail.Subject;
                }
                mail.MailBody = mail.Subject;
                if (!EmailSender.SendMail(mail))
                {
                    return "账号注册成功，但是未能发送到您的邮箱";
                }
                return SUCCESS;
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
                return "账号注册成功，但是邮件发送异常";
            }
        }

        /// <summary>
        /// 重设用户密码为随机密码，并将新密码发送到用户邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public string ResetUserPassWord(string email)
        {
            if (!ValidateHelper.IsEmail(email))
            {
                return "请输入注册邮箱";
            }
            var model = _UserDal.GetFirst(x => x.Email == email);
            if (model == null) { return "用户不存在"; }
            string user_pass = Com.GetRandomNumString();
            model.PassWord = UserPassWordEncrypt(user_pass);

            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo)) { return errinfo; }

            if (_UserDal.Update(model) <= 0)
            {
                return "密码修改错误";
            }

            try
            {
                var mail = new EmailModel();
                mail.ToList = new List<string>() { email };
                mail.Subject = string.Format("您申请了重设密码，目前登陆帐户：{0}，密码：{1}", email, user_pass);
                mail.MailBody = mail.Subject;
                if (!EmailSender.SendMail(mail))
                {
                    return "发送邮件错误";
                }
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
            }

            return SUCCESS;
        }

        /// <summary>
        /// 读取用户角色和用户特权（读取model）
        /// </summary>
        /// <param name="list"></param>
        /// <param name="LoadEntity"></param>
        /// <returns></returns>
        private List<UserModel> GetRolesForUserList(List<UserModel> list)
        {
            var useridlist = list.Select(x => x.UID).ToArray();

            //读取角色
            var userrolemaplist = _UserRoleDal.GetList(x => useridlist.Contains(x.UserID));
            if (ValidateHelper.IsPlumpList(userrolemaplist))
            {
                var roleidlist = userrolemaplist.Select(x => x.RoleID).Distinct().ToArray();
                var rolelist = _RoleDal.GetList(x => roleidlist.Contains(x.UID));
                if (ValidateHelper.IsPlumpList(rolelist))
                {
                    foreach (var model in list)
                    {
                        var myroleidlist = userrolemaplist.Where(x => x.UserID == model.UID).Select(x => x.RoleID).Distinct().ToArray();
                        if (!ValidateHelper.IsPlumpList(myroleidlist)) { continue; }
                        model.RoleModelList = rolelist.Where(x => myroleidlist.Contains(x.UID)).Select(x => (RoleEntityBase)x).ToList();
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 获取用户的所有权限（只读取ID）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private UserModel LoadAllPermissionForUser(UserModel model)
        {
            model.RoleList = new List<string>();
            model.PermissionList = new List<string>();

            _UserDal.PrepareSession(db =>
            {
                var maprole = db.Set<UserRoleModel>().Where(x => x.UserID == model.UID).Select(x => x.RoleID);
                var deftrole = db.Set<RoleModel>().Where(x => x.AutoAssignRole > 0).Select(x => x.UID);

                var rolepermissionlist = db.Set<RolePermissionModel>()
                .Where(x => maprole.Contains(x.RoleID) || deftrole.Contains(x.RoleID))
                .Select(x => new { role = x.RoleID, permission = x.PermissionID }).ToList();

                if (ValidateHelper.IsPlumpList(rolepermissionlist))
                {
                    model.RoleList.AddRange(rolepermissionlist.Select(x => x.role).Distinct());
                    model.PermissionList.AddRange(rolepermissionlist.Select(x => x.permission).Distinct());
                }
                return true;
            });

            return model;
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="loginuser"></param>
        /// <returns></returns>
        public List<string> FetchPermission(LoginUserInfo loginuser)
        {
            var RoleList = new List<string>();
            var PermissionList = new List<string>();
            _UserDal.PrepareSession(db =>
            {
                var maprole = db.Set<UserRoleModel>().Where(x => x.UserID == loginuser.UserID).Select(x => x.RoleID);
                var deftrole = db.Set<RoleModel>().Where(x => x.AutoAssignRole > 0).Select(x => x.UID);

                var rolepermissionlist = db.Set<RolePermissionModel>()
                .Where(x => maprole.Contains(x.RoleID) || deftrole.Contains(x.RoleID))
                .Select(x => new { role = x.RoleID, permission = x.PermissionID }).ToList();

                if (ValidateHelper.IsPlumpList(rolepermissionlist))
                {
                    RoleList.AddRange(rolepermissionlist.Select(x => x.role).Distinct());
                    PermissionList.AddRange(rolepermissionlist.Select(x => x.permission).Distinct());
                }
                return true;
            });
            return PermissionList;
        }
    }
}
