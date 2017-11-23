using Lib.mvc.user;

namespace Hiwjcn.Web.Models
{
    public abstract class ViewModelBase
    {
        public virtual LoginUserInfo LoginUser { get; set; }

        public virtual long LoginUserID
        {
            get
            {
                if (LoginUser != null) { return LoginUser.IID; }
                return 0;
            }
        }

        public virtual string LoginUserName
        {
            get
            {
                if (LoginUser != null) { return LoginUser.NickName; }
                return "未登陆";
            }
        }

    }
}