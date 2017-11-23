using Lib.mvc.user;
using Lib.ioc;
using System;
using System.Web;
using Lib.mvc.auth;
using System.Threading.Tasks;

namespace Lib.mvc
{
    /// <summary>
    /// PC前台工作上下文类
    /// </summary>
    public class WebWorkContext : IDisposable
    {
        public HttpContext context { get; private set; }

        public bool IsPost { get; private set; }

        public bool IsAjax { get; private set; }

        public bool IsPostAjax
        {
            get => this.IsPost && this.IsAjax;
        }

        public string IP { get; private set; }

        public string BaseUrl { get; private set; }

        public string Url { get; private set; }

        #region 登录信息
        private LoginUserInfo _auth_user;
        public LoginUserInfo AuthUser
        {
            get
            {
                if (this._auth_user == null)
                {
                    this._auth_user = this.context.GetAuthUser();
                }
                return this._auth_user;
            }
        }

        public LoginUserInfo User { get; private set; }

        public LoginUserInfo SSOUser { get; private set; }

        public LoginUserInfo LoginUser { get; private set; }

        public LoginUserInfo LoginTrader { get; private set; }

        public LoginUserInfo LoginSeller { get; private set; }

        public LoginUserInfo LoginAdmin { get; private set; }
        #endregion

        public WebWorkContext() : this(System.Web.HttpContext.Current)
        { }

        public WebWorkContext(System.Web.HttpContext context)
        {
            this.context = context ?? throw new Exception("上下文不能为空");

            this.IsPost = context.Request.IsPost();
            this.IsAjax = context.Request.IsAjax();
            this.IP = context.Ip();
            this.BaseUrl = context.Request.GetBaseUrl();
            this.Url = context.Request.GetCurrentUrl();
            //login user
            LoadLoginUser();
        }

        public void LoadLoginUser()
        {
            this._auth_user = null;
            this.User = AppContext.Scope(s => s.Resolve_<LoginStatus>().GetLoginUser(this.context));
            this.SSOUser = AccountHelper.SSO.GetLoginUser(this.context);
            this.LoginUser = AccountHelper.User.GetLoginUser(this.context);
            this.LoginTrader = AccountHelper.Trader.GetLoginUser(this.context);
            this.LoginSeller = AccountHelper.Seller.GetLoginUser(this.context);
            this.LoginAdmin = AccountHelper.Admin.GetLoginUser(this.context);
        }

        public void Dispose()
        {
            //
        }
    }
}
