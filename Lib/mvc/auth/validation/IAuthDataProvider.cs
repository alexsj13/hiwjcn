using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lib.mvc.auth;
using Lib.mvc.user;
using System.Configuration;
using Lib.extension;
using Lib.helper;

namespace Lib.mvc.auth.validation
{
    /// <summary>
    /// 获取token和client 信息的渠道
    /// </summary>
    public interface IAuthDataProvider
    {
        string GetToken(HttpContext context);

        string GetClientID(HttpContext context);

        string GetClientSecurity(HttpContext context);
    }
}
