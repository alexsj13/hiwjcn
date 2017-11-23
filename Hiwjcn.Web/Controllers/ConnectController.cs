using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.data;
using Lib.extension;
using Lib.net;
using Lib.cache;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Bll.Auth;
using Hiwjcn.Core.Domain.Auth;
using Lib.mvc.auth;
using Hiwjcn.Framework;
using System.Collections.ObjectModel;
using Lib.mvc.auth.validation;
using Lib.infrastructure.service;

namespace Hiwjcn.Web.Controllers
{
    [RoutePrefix("connect")]
    public class ConnectController : BaseController
    {
        private readonly IAuthDataProvider _IValidationDataProvider;
        private readonly IAuthLoginService _IAuthLoginService;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly IAuthClientService _IAuthClientService;
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;

        public ConnectController(
            IAuthDataProvider _IValidationDataProvider,
            IAuthLoginService _IAuthLoginService,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IAuthClientService _IAuthClientService,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthClient> _AuthClientRepository)
        {
            this._IValidationDataProvider = _IValidationDataProvider;
            this._IAuthLoginService = _IAuthLoginService;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;
            this._IAuthClientService = _IAuthClientService;

            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthClientRepository = _AuthClientRepository;
        }

        [RequestLog]
        public async Task<ActionResult> Authorize(string client_id, string redirect_uri, string scope,
            string response_type, string state, string login_type, string return_type)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsPlumpString(redirect_uri))
                {
                    return Content($"{nameof(redirect_uri)}为空");
                }

                ViewData[nameof(redirect_uri)] = redirect_uri;
                ViewData[nameof(login_type)] = login_type;
                ViewData[nameof(return_type)] = return_type;

                var scopes = ConvertHelper.GetString(scope).Trim().Split(',').Where(x => ValidateHelper.IsPlumpString(x)).ToList();
                var scopelist = await this._IAuthScopeService.GetScopesOrDefaultAsync(scopes.ToArray());
                ViewData["scopes"] = scopelist.OrderByDescending(x => x.Important).ToList();

                var client = await this._IAuthClientService.GetByID(client_id);
                if (client == null)
                {
                    return Content("client_id无效");
                }
                ViewData["client"] = client;

                //使用异步加载
                var loginuser = await this.X.context.GetAuthUserAsync();

                return View();
            });
        }

        [HttpPost]
        [ApiAuth]
        [RequestLog]
        public async Task<ActionResult> CreateAuthorizeCode(string client_id, string scope)
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();

                var data = await this._IAuthTokenService.CreateCodeAsync(client_id, scope?.JsonToEntity<List<string>>(), loginuser.UserID);
                if (ValidateHelper.IsPlumpString(data.msg))
                {
                    return GetJsonRes(data.msg);
                }
                return GetJson(new _() { success = true, data = data.data?.UID });
            });
        }

        [PageAuth]
        [RequestLog]
        public async Task<ActionResult> MyClients(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = CheckPage(page);
                var pagesize = 100;

                var loginuser = await this.X.context.GetAuthUserAsync();

                var data = await this._IAuthTokenService.GetMyAuthorizedClientsAsync(loginuser?.UserID, q, page.Value, pagesize);
                ViewData["pager"] = data.GetPagerHtml(this, "page", page.Value, pagesize);
                ViewData["list"] = data.DataList;
                return View();
            });
        }

    }
}