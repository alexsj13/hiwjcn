using Hiwjcn.Core.Infrastructure.User;
using Lib.helper;
using Lib.infrastructure;
using System;
using System.Threading.Tasks;
using WebLogic.Dal.User;
using WebLogic.Model.User;

namespace WebLogic.Bll.User
{
    public class LoginErrorLogBll : ServiceBase<LoginErrorLogModel>, ILoginLogService
    {
        private LoginErrorLogDal _LoginErrorLogDal { get; set; }

        public LoginErrorLogBll()
        {
            this._LoginErrorLogDal = new LoginErrorLogDal();
        }

        /// <summary>
        /// 添加登录错误
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> AddLoginErrorLog(LoginErrorLogModel model)
        {
            model?.Init();
            if (!this.CheckModel(model, out var msg))
            {
                return msg;
            }

            if (await _LoginErrorLogDal.AddAsync(model) > 0)
            {
                return SUCCESS;
            }
            throw new Exception("添加登录错误记录失败");
        }

        /// <summary>
        /// 获取登录错误，并清除旧数据(时间比较使用秒)
        /// </summary>
        /// <param name="LoginKey"></param>
        /// <param name="ExpireTime"></param>
        /// <returns></returns>
        public async Task<int> GetRecentLoginErrorTimes(string LoginKey)
        {
            if (!ValidateHelper.IsPlumpString(LoginKey)) { throw new Exception("loginkey为空"); }
            var start = DateTime.Now.AddMinutes(-10);

            int count = await this._LoginErrorLogDal.GetCountAsync(x => x.LoginKey == LoginKey && x.CreateTime >= start);

            //清理更早的数据
            await this._LoginErrorLogDal.DeleteWhereAsync(x => x.LoginKey == LoginKey && x.CreateTime < start);

            return count;
        }

    }
}
