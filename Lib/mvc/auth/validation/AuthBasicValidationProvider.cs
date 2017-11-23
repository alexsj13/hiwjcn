using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lib.ioc;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.mvc.user;
using System.Net;
using System.Net.Http;
using Lib.net;

namespace Lib.mvc.auth.validation
{
    /// <summary>
    /// 使用了auth api来验证
    /// </summary>
    public class AuthBasicValidationProvider : TokenValidationProviderBase
    {
        private readonly IAuthDataProvider _dataProvider;
        private readonly IAuthApi api;

        public AuthBasicValidationProvider(
            IAuthDataProvider _dataProvider,
            IAuthApi api)
        {
            this._dataProvider = _dataProvider;
            this.api = api;
        }

        public override string HttpContextItemKey() => "context.items.auth.user.entity";

        public override LoginUserInfo FindUser(HttpContext context)
        {
            return AsyncHelper.RunSync(() => FindUserAsync(context));
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            try
            {
                var access_token = this._dataProvider.GetToken(context);
                var client_id = this._dataProvider.GetClientID(context);

                var loginuser = await this.api.GetLoginUserInfoByTokenAsync(client_id: client_id, access_token: access_token);

                if (!loginuser.success)
                {
                    loginuser.msg?.AddBusinessInfoLog();
                    return null;
                }

                return loginuser.data;
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return null;
            }
        }
    }
}
