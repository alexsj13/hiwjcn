using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using System.Threading;
using System.Diagnostics;
using Lib.helper;
using Polly;
using Polly.Retry;
using org.apache.zookeeper;

namespace Lib.distributed.zookeeper
{
    public class ZooKeeperDistributedLock : ZooKeeperClient, IDistributedLock
    {
        private readonly string _path;
        private string _no;

        public ZooKeeperDistributedLock(string key, string configName) :
            this(key, ZooKeeperConfigSection.FromSection(configName))
        {
            //
        }

        public ZooKeeperDistributedLock(string key, ZooKeeperConfigSection config) :
            this(config.Server, config.DistributedLockPath, key)
        {
            //
        }

        public ZooKeeperDistributedLock(string host, string lock_path, string key) : base(host)
        {
            this._path = lock_path + "/" + key.ToMD5();
        }

        public async Task LockOrThrow()
        {
            await this.Client.CreatePersistentPathIfNotExist_(this._path);
            var pt = await this.Client.CreateNode_(this._path + "/", CreateMode.EPHEMERAL_SEQUENTIAL);
            this._no = pt.SplitZookeeperPath().Reverse_().Take(1).FirstOrDefault() ?? throw new Exception("创建序列节点失败");

            //最小值拿到锁
            var children = await this.Client.GetChildrenOrThrow_(this._path);
            var preNo = children.Where(x => string.CompareOrdinal(x, this._no) < 0).OrderByDescending(x => x).FirstOrDefault();
            if (preNo != null)
            {
                throw new Exception("获取锁失败");
            }
        }

        public async Task ReleaseLock()
        {
            try
            {
                await this.Client.DeleteSingleNode_(this._path + "/" + this._no);
                this._no = null;
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }

        public override void Dispose()
        {
            if (ValidateHelper.IsPlumpString(this._no))
            {
                AsyncHelper_.RunSync(() => this.ReleaseLock());
            }

            base.Dispose();
        }
    }
}
