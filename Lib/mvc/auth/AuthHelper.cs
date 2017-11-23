using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;

namespace Lib.mvc.auth
{
    public static class AuthHelper
    {
        public static List<string> ParseScopes(List<string> scope)
        {
            var scopeslist = ConvertHelper.NotNullList(scope);
            scopeslist = scopeslist.Select(x => x?.Trim()).Where(x => ValidateHelper.IsPlumpString(x)).ToList();
            return scopeslist;
        }

        public static List<string> ParseScopes(string scope)
        {
            try
            {
                var scopeslist = (scope ?? throw new Exception("scope为空")).JsonToEntity<List<string>>();

                return AuthHelper.ParseScopes(scopeslist);
            }
            catch (Exception e)
            {
                throw new Exception("无法解析Scopes", e);
            }
        }
    }
}
