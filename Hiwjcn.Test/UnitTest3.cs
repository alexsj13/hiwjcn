using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bll.User;
using Lib.extension;
using Lib.infrastructure;
using Hiwjcn.Core.Infrastructure.User;
using Model.User;
using System.Linq;
using Lib.helper;
using Model;
using Lib.infrastructure.service;
using Lib.infrastructure.entity;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest3
    {
        class tm : TreeEntityBase
        { }

        class trr : TreeServiceBase<tm>
        { }

        [TestMethod]
        public void lkhkjhafdsafasdkgf()
        {
            var interfaces = typeof(trr).GetInterfaces();
        }

        [TestMethod]
        public void strhide()
        {
            var list = new int[10].Select(x => Com.GetUUID().HideForSecurity()).ToList();
        }

        [TestMethod]
        public void TestMethod1()
        {
            var userType = typeof(UserBll);
            //false
            var a = userType.IsAssignableTo_(typeof(IServiceBase<>));
            //false
            var b = userType.IsGenericType_(typeof(ServiceBase<>));
            //true
            var c = userType.BaseType.IsGenericType_(typeof(ServiceBase<>));

            //{Name = "ServiceBase`1" FullName = "Lib.infrastructure.ServiceBase`1"}
            var d = userType.BaseType.GetGenericTypeDefinition();
            //{Name = "IServiceBase`1" FullName = "Lib.infrastructure.IServiceBase`1"}
            var e = typeof(IServiceBase<>);
            //{Name = "ServiceBase`1" FullName = "Lib.infrastructure.ServiceBase`1"}
            var f = typeof(ServiceBase<>);
            //{Name = "ServiceBase`1" FullName = "Lib.infrastructure.ServiceBase`1[[Model.User.UserModel, Hiwjcn.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"}
            var m = typeof(ServiceBase<UserModel>);

            //true
            var g = userType.IsAssignableTo_<IUserService>();
            //true
            var h = userType.IsAssignableTo_(typeof(IUserService));
        }
    }
}
