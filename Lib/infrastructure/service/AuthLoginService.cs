using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;
using Lib.mvc;
using Lib.helper;
using Autofac;

namespace Lib.infrastructure.service
{
    public interface IAuthLoginService
    {
        Task<PagerData<LoginUserInfo>> SearchUser(string q = null, int page = 1, int pagesize = 10);

        Task<LoginUserInfo> LoadPermissions(LoginUserInfo model);

        Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password);

        Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code);

        Task<string> SendOneTimeCode(string phoneOrEmail);

        Task<LoginUserInfo> GetUserInfoByUID(string uid);
    }

    public static class LoginAccountSystemExtension
    {
        public static void UseAccountSystem<T>(this ContainerBuilder builder) where T : IAuthLoginService
        {
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().As<IAuthLoginService>().SingleInstance();
        }
    }
}
