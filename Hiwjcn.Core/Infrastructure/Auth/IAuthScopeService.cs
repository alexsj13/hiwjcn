using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.helper;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthScopeService : IServiceBase<AuthScope>
    {
        Task<List<AuthScope>> GetScopesOrDefaultAsync(params string[] names);

        Task<string> AddScopeAsync(AuthScope scope);

        Task<string> DeleteScopeAsync(string scope_uid);

        Task<string> UpdateScopeAsync(AuthScope updatemodel);

        Task<PagerData<AuthScope>> QueryPagerAsync(string q = null, int page = 1, int pagesize = 10);
    }
}
