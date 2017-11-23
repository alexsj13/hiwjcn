using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.extension;
using Lib.core;
using Lib.helper;
using Lib.ioc;
using Lib.data;
using org.apache.zookeeper;
using System.Threading.Tasks;
using Lib.mvc;
using static org.apache.zookeeper.ZooDefs;
using Lib.distributed;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest8
    {
        [TestMethod]
        public void TestMethod1()
        {
            var json = new _().ToJson();
        }
    }
}
