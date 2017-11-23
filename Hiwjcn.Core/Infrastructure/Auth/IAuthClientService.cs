using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Lib.helper;
using Hiwjcn.Core.Domain.Auth;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthClientService : IServiceBase<AuthClient>
    {
        Task<AuthClient> GetByID(string uid);

        Task<string> AddClientAsync(AuthClient client);

        Task<string> DeleteClientAsync(string client_uid, string user_uid);

        Task<string> EnableOrDisableClientAsync(string client_uid, string user_uid);

        Task<string> UpdateClientAsync(AuthClient updatemodel);

        Task<PagerData<AuthClient>> QueryListAsync(
            string user_uid = null, string q = null, bool? is_active = null, bool? is_remove = null,
            int page = 1, int pagesize = 10);
    }
}
