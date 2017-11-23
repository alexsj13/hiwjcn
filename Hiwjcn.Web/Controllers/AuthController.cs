using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.net;
using Lib.data;
using Lib.cache;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Bll.Auth;
using Hiwjcn.Core.Domain.Auth;
using Lib.events;
using Hiwjcn.Core.Model.Sys;
using Hiwjcn.Framework.Actors;
using Lib.mvc.auth;
using Hiwjcn.Framework;

namespace Hiwjcn.Web.Controllers
{
    [Obsolete("使用wcf的provider，不提供webapi的支持")]
    [RoutePrefix(AuthApiControllerConfig.ControllerName)]
    public class AuthController : BaseController
    {
        private readonly IAuthApi api;
        public AuthController(IAuthApi api)
        {
            this.api = api;
        }

        /// <summary>
        /// 用code换token
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="client_secret"></param>
        /// <param name="code"></param>
        /// <param name="grant_type"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestLog]
        [ActionName(AuthApiControllerConfig.Action_AccessToken)]
        public async Task<ActionResult> AccessToken(
            string client_id, string client_secret, string code, string grant_type)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this.api.GetAccessTokenAsync(client_id, client_secret, code, grant_type);
                return GetJson(data);
            });
        }

        /// <summary>
        /// 检查token 返回用户信息
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestLog]
        [ActionName(AuthApiControllerConfig.Action_CheckToken)]
        public async Task<ActionResult> CheckToken(string client_id, string access_token)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this.api.GetLoginUserInfoByTokenAsync(client_id, access_token);
                return GetJson(data);
            });
        }

        /// <summary>
        /// 用账户密码换取token
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="scope"></param>
        /// <param name="phone"></param>
        /// <param name="sms"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestLog]
        [ActionName(AuthApiControllerConfig.Action_AuthCodeByOneTimeCode)]
        public async Task<ActionResult> AuthCodeByOneTimeCode(string client_id, string scope, string phone, string sms)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this.api.GetAuthCodeByOneTimeCodeAsync(client_id, AuthHelper.ParseScopes(scope), phone, sms);
                return GetJson(data);
            });
        }

        [HttpPost]
        [RequestLog]
        [ActionName(AuthApiControllerConfig.Action_AuthCodeByPassword)]
        public async Task<ActionResult> AuthCodeByPassword(string client_id, string scope, string username, string password)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this.api.GetAuthCodeByPasswordAsync(client_id, AuthHelper.ParseScopes(scope), username, password);
                return GetJson(data);
            });
        }

        [HttpPost]
        [RequestLog]
        [ActionName(AuthApiControllerConfig.Action_RemoveCache)]
        public async Task<ActionResult> RemoveCache(string data)
        {
            return await RunActionAsync(async () =>
            {
                var bundle = data?.JsonToEntity<CacheBundle>();
                var res = await this.api.RemoveCacheAsync(bundle);
                return GetJson(res);
            });
        }
    }
}