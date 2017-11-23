using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Model.User;
using System.Runtime.Serialization;
using Model;
using System.Configuration;
using Lib.extension;
using Lib.infrastructure.entity;

namespace Hiwjcn.Core.Domain.Auth
{
    /// <summary>
    /// auth客户端
    /// </summary>
    [Serializable]
    [Table("auth_client")]
    public class AuthClient : AuthClientBase { }

    [Serializable]
    [Table("auth_client_checklog")]
    public class AuthClientCheckLog : AuthClientCheckLogBase { }

    [Serializable]
    [Table("auth_client_useage")]
    public class AuthClientUseage : AuthClientCheckLogBase { }

    [Serializable]
    [Table("auth_scope")]
    public class AuthScope : AuthScopeBase { }

    [Serializable]
    [Table("auth_token")]
    public class AuthToken : AuthTokenBase { }

    [Serializable]
    [Table("auth_token_scope")]
    public class AuthTokenScope : AuthTokenScopeBase { }

    [Serializable]
    [Table("auth_code")]
    public class AuthCode : AuthCodeBase { }
}
