using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using org.apache.zookeeper;
using Lib.extension;
using Lib.helper;
using Lib.data;
using Lib.ioc;
using Lib.core;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;
using org.apache.zookeeper.data;
using System.Net;
using System.Net.Http;
using Lib.net;
using System.Text;
using Lib.rpc;

namespace Lib.distributed.zookeeper
{
    public static class ZooKeeperClientExtension
    {
        public static List<string> SplitZookeeperPath(this string path) =>
            path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        public static string AsZookeeperPath(this IEnumerable<string> path) =>
            "/" + "/".Join_(path);

        public static async Task<string> CreateNode_(this ZooKeeper client, string path, CreateMode mode, byte[] data = null) =>
                await client.createAsync(path, data, Ids.OPEN_ACL_UNSAFE, mode);

        public static async Task<bool> WatchNode_(this ZooKeeper client, string path, Watcher watcher) =>
            await client.ExistAsync_(path, watcher ?? throw new ArgumentNullException(nameof(watcher)));

        public static async Task<bool> ExistAsync_(this ZooKeeper client, string path, Watcher watcher = null)
        {
            if (watcher == null)
            {
                return await client.existsAsync(path) != null;
            }
            else
            {
                return await client.existsAsync(path, watcher) != null;
            }
        }

        public static async Task EnsurePath(this ZooKeeper client, string path)
        {
            if (await client.ExistAsync_(path))
            {
                return;
            }

            var sp = path.SplitZookeeperPath();
            var p = string.Empty;
            foreach (var itm in sp)
            {
                p += $"/{itm}";
                if (await client.ExistAsync_(p))
                {
                    continue;
                }

                await client.CreateNode_(p, CreateMode.PERSISTENT);
            }
        }

        public static async Task<string> CreatePersistentPathIfNotExist_(this ZooKeeper client,
            string path, byte[] data = null)
        {
            await client.EnsurePath(path);
            if (ValidateHelper.IsPlumpList(data))
            {
                await client.SetDataAsync_(path, data);
            }
            return path.SplitZookeeperPath().AsZookeeperPath();
        }

        [Obsolete("还有问题")]
        public static async Task<string> CreateSequentialPath_(this ZooKeeper client,
            string path, byte[] data = null, bool persistent = true)
        {
            var mode = persistent ? CreateMode.PERSISTENT_SEQUENTIAL : CreateMode.EPHEMERAL_SEQUENTIAL;
            var p = await client.CreateNode_(path, mode, data);
            return p.Substring(path.Length);
        }

        public static async Task<Stat> SetDataAsync_(this ZooKeeper client, string path, byte[] data) =>
            await client.setDataAsync(path, data);

        public static async Task<byte[]> GetDataOrThrow_(this ZooKeeper client, string path, Watcher watcher = null)
        {
            DataResult res = null;
            if (watcher == null)
            {
                res = await client.getDataAsync(path);
            }
            else
            {
                res = await client.getDataAsync(path, watcher);
            }
            if (res.Stat == null) { throw new Exception(res.ToJson()); }
            return res.Data;
        }

        public static async Task DeleteNodeRecursively_(this ZooKeeper client, string path)
        {
            var handlered_list = new List<string>();

            async Task __DeleteNode(string pre_path, string p)
            {
                var pre_node = pre_path.SplitZookeeperPath();
                var cur_node = p.SplitZookeeperPath();
                if (!ValidateHelper.IsPlumpList(cur_node))
                {
                    throw new Exception($"不能删除：{p}");
                }
                pre_node.AddRange(cur_node);

                var current_node = pre_node.AsZookeeperPath();
                //检查死循环
                handlered_list.AddOnceOrThrow(current_node,
                    $"递归发生错误，已处理节点：{handlered_list.ToJson()}，再次处理：{current_node}");

                if (!await client.ExistAsync_(current_node))
                {
                    return;
                }
                var children = await client.GetChildrenOrThrow_(current_node);
                if (ValidateHelper.IsPlumpList(children))
                {
                    foreach (var child in children.Where(x => ValidateHelper.IsPlumpString(x)))
                    {
                        //递归
                        await __DeleteNode(current_node, child);
                    }
                }
                await client.DeleteSingleNode_(current_node);
            }

            //入口
            await __DeleteNode(string.Empty, path);
        }

        public static async Task DeleteSingleNode_(this ZooKeeper client, string path) =>
            await client.deleteAsync(path);

        public static async Task<List<string>> GetChildrenOrThrow_(this ZooKeeper client,
            string path, Watcher watcher = null)
        {
            ChildrenResult res = null;
            if (watcher == null)
            {
                res = await client.getChildrenAsync(path);
            }
            else
            {
                res = await client.getChildrenAsync(path, watcher);
            }
            if (res.Stat == null) { throw new Exception("读取子节点状态为空"); }

            return res.Children ?? new List<string>() { };
        }

        public static async Task WatchChildren_(this ZooKeeper client, string path, Watcher watcher) =>
            await client.GetChildrenOrThrow_(path, watcher ?? throw new ArgumentNullException(nameof(watcher)));
    }
}
