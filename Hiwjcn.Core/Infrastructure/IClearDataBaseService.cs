using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ioc;

namespace Hiwjcn.Core.Infrastructure
{
    [Obsolete("清理数据是危险操作，请不要随意开启！！！")]
    public interface IClearDataBaseService : IAutoRegistered
    {
        void ClearScope();

        void ClearToken();

        void ClearClient();

        void ClearUser();

        void ClearPage();

        void ClearRequestLog();

        void ClearCacheHitLog();

        void ClearTag();

        void ClearRole();

        void ClearPermission();

        void ClearLoginLog();
    }
}
