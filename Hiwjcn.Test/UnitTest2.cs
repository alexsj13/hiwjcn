using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Castle.DynamicProxy;
using Lib.helper;
using System.Dynamic;
using System.Reflection.Emit;
using System.Reflection;
using Newtonsoft.Json;
using Lib.extra.api;
using Polly;
using System.Threading;
using Polly.Timeout;
using Polly.Retry;
using Polly.CircuitBreaker;
using Lib.mq;
using Lib.io;
using Lib.extension;
using System.IO;
using System.Linq;
using Hiwjcn.Dal;
using Autofac;
using Lib.ioc;
using Hiwjcn.Framework;
using System.Data.Entity;

namespace Hiwjcn.Test
{
    /// <summary>
    /// UnitTest2 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTest2
    {
        public UnitTest2()
        {
            AppContext.AddExtraRegistrar(new CommonDependencyRegister());
        }
        
        [TestMethod]
        public void fasdfkjasldfajsdkfhasldfkj()
        {
            var codeHelper = new DrawVerifyCode();
            var path = "d:\\data";
            new DirectoryInfo(path).CreateIfNotExist();
            for (var i = 0; i < 100; ++i)
            {
                var p = Path.Combine(path, $"data_{i}");
                new DirectoryInfo(p).CreateIfNotExist();
                for (var j = 0; j < 1000; ++j)
                {
                    var (bs, with, height) = codeHelper.GetImageBytesAndSize();
                    var f = Path.Combine(p, $"{codeHelper.Code}_{Com.GetUUID()}.png");
                    using (var fs = new FileStream(f, FileMode.Create))
                    {
                        fs.Write(bs, 0, bs.Length);
                    }
                }
            }
        }

        [TestMethod]
        public void fasdfasdfadsf()
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var tps = a.GetTypes();
                }
                catch (Exception e)
                {

                }
            }
        }

        [TestMethod]
        public void mq()
        {
            //
        }

        [TestMethod]
        public void rongduan()
        {
            CircuitBreakerPolicy p = Policy.Handle<Exception>().CircuitBreaker(3, TimeSpan.FromMinutes(1));
            try
            {
                p.Execute(() =>
                {
                    throw new Exception("fasd");
                });
            }
            catch (Exception e)
            { }
            try
            {
                p.Execute(() =>
                {
                    throw new Exception("fasd");
                });
            }
            catch (Exception e)
            { }
            try
            {
                p.Execute(() =>
                {
                    throw new Exception("fasd");
                });
            }
            catch (Exception e)
            { }
            //错误3次后熔断，action不执行，直接抛出异常
            try
            {
                p.Execute(() =>
                {
                    throw new Exception("fasd");
                });
            }
            catch (Exception e)
            { }
        }

        [TestMethod]
        public void PollyTest()
        {
            try
            {
                int i = 0;
                var count = Policy.Handle<Exception>().Retry(3).Execute(() =>
                 {
                     if ((++i) <= 1) { throw new Exception(""); }
                     return 0;
                 });

                string data = null;
                Policy.Timeout(3, TimeoutStrategy.Pessimistic).Execute(() =>
                {
                    Thread.Sleep(5000);
                    data = "拿到数据";
                });
            }
            catch (Exception e)
            { }
        }

        [TestMethod]
        public void fsadfasd()
        {
            var json = JsonConvert.SerializeObject(new { name = "wj", age = 25 });

            var obj = JsonConvert.DeserializeObject(json);

        }

        [TestMethod]
        public void fsadfajsdflasdfas()
        {
            try
            {
                var payload = new PayLoadAndroid();
                payload.body = new PayLoadAndroidBody()
                {
                    ticker = "123",
                    title = "123",
                    text = "123"
                };
                payload.extra = new
                {
                    ContentType = "7",
                    PositionMackTitle = "网页",
                    Url = "http://www.qq.com/"
                };
                var t = UmengPushHelper.PushAndroid(new UmengPushKeyConfig(), payload, new List<string>() { "6d28395cac6c427bb77fba889b9e54a5", "b11dfe19ef4d4f60860dda673dfa7863" });
                var data = Lib.helper.AsyncHelper.RunSync(() => t);
            }
            catch (Exception e)
            {
                //
            }
        }

        [TestMethod]
        public void fasdfasd()
        {
            try
            {
                var payload = new
                {
                    aps = new { alert = "提示内容" },
                    ContentType = "7",
                    PositionMackTitle = "网页",
                    Url = "http://www.qq.com/"
                };
                var t = UmengPushHelper.PushIOS(new UmengPushKeyConfig(), payload, new List<string>() { "6d28395cac6c427bb77fba889b9e54a5", "b11dfe19ef4d4f60860dda673dfa7863" });
                var data = Lib.helper.AsyncHelper.RunSync(() => t);
            }
            catch (Exception e)
            { }
        }

        [TestMethod]
        public void TestMethod1()
        {
            // specify a new assembly name
            var assemblyName = new AssemblyName("Kitty");

            // create assembly builder
            var assemblyBuilder = AppDomain.CurrentDomain
              .DefineDynamicAssembly(assemblyName,
                AssemblyBuilderAccess.RunAndSave);

            // create module builder
            var moduleBuilder =
              assemblyBuilder.DefineDynamicModule(
                "KittyModule", "Kitty.exe");

            // create type builder for a class
            var typeBuilder =
              moduleBuilder.DefineType(
                "HelloKittyClass", TypeAttributes.Public);

            // create method builder
            var methodBuilder = typeBuilder.DefineMethod(
              "SayHelloMethod",
              MethodAttributes.Public | MethodAttributes.Static,
              null,
              null);

            // then get the method il generator
            var il = methodBuilder.GetILGenerator();

            // then create the method function
            il.Emit(OpCodes.Ldstr, "Hello, Kitty!");
            il.Emit(OpCodes.Call,
              typeof(Console).GetMethod(
              "WriteLine", new Type[] { typeof(string) }));
            il.Emit(OpCodes.Call,
              typeof(Console).GetMethod("ReadLine"));
            il.Emit(OpCodes.Pop); // we just read something here, throw it.
            il.Emit(OpCodes.Ret);

            // then create the whole class type
            var helloKittyClassType = typeBuilder.CreateType();

            // set entry point for this assembly
            assemblyBuilder.SetEntryPoint(
              helloKittyClassType.GetMethod("SayHelloMethod"));

            // save assembly
            assemblyBuilder.Save("Kitty.exe");

            Console.WriteLine(
              "Hi, Dennis, a Kitty assembly has been generated for you.");
            Console.ReadLine();
        }
    }
}
