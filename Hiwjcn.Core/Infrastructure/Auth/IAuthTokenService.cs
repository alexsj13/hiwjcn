using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.helper;
using Lib.mvc;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthTokenService : IServiceBase<AuthToken>
    {
        Task<_<AuthCode>> CreateCodeAsync(string client_uid, List<string> scopes, string user_uid);

        Task<_<AuthToken>> CreateTokenAsync(string client_id, string client_secret, string code_uid);

        Task<AuthToken> FindTokenAsync(string client_uid, string token_uid);

        Task<string> DeleteUserTokensAsync(string client_uid, string user_uid);

        Task<string> DeleteUserTokensAsync(string user_uid);

        Task<PagerData<AuthClient>> GetMyAuthorizedClientsAsync(string user_id, string q, int page, int pagesize);
    }
}
