using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.User;

namespace Hiwjcn.Core.Infrastructure.User
{
    public interface ILoginLogService : IServiceBase<LoginErrorLogModel>
    {
        Task<string> AddLoginErrorLog(LoginErrorLogModel model);

        /// <summary>
        /// 获取登录错误，并清除旧数据(时间比较使用秒)
        /// </summary>
        /// <param name="LoginKey"></param>
        /// <param name="ExpireTime"></param>
        /// <returns></returns>
        Task<int> GetRecentLoginErrorTimes(string LoginKey);
    }
}
