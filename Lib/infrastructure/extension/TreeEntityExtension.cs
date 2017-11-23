using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure.entity;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.data.ef;

namespace Lib.infrastructure.extension
{
    public static class TreeEntityExtension
    {
        #region 递归相关逻辑

        public static async Task<List<T>> FindNodeChildrenRecursively_<T>(this IQueryable<T> data_source, T first_node,
            string tree_error = "树存在无限递归")
            where T : TreeEntityBase
        {
            var repeat_check = new List<string>();
            var list = new List<T>();

            async Task FindRecursively(T node)
            {
                if (node == null) { return; }

                repeat_check.AddOnceOrThrow(node.UID, error_msg: tree_error);

                var child_parent_uid = node.UID;
                var child_level = node.Level + 1;
                var children = await data_source.Where(x => x.ParentUID == child_parent_uid && x.Level == child_level).ToListAsync();
                if (ValidateHelper.IsPlumpList(children))
                {
                    foreach (var child in children)
                    {
                        //递归
                        await FindRecursively(child);
                    }
                }

                list.Add(node);
            }

            await FindRecursively(first_node);

            return list;
        }

        public static async Task<(bool success, List<T> node_path)> CheckNodeIfCanFindRoot<T>(this IQueryable<T> data_source, T first_node)
            where T : TreeEntityBase
        {
            var repeat_check = new List<string>();
            var current_uid = first_node.UID;

            var top_level = default(int?);
            var top_parent = default(string);

            var node = default(T);

            var node_path = new List<T>();

            while (true)
            {
                node = await data_source.Where(x => x.UID == current_uid).FirstOrDefaultAsync();
                if (node == null) { break; }

                repeat_check.AddOnceOrThrow(node.UID, error_msg: "存在无限循环");

                {
                    //设置检查值
                    top_level = node.Level;
                    top_parent = node.ParentUID;
                }

                current_uid = node.ParentUID;
                node_path.Add(node);
            }
            var success = top_level == TreeEntityBase.FIRST_LEVEL && top_parent == TreeEntityBase.FIRST_PARENT_UID;
            return (success, node_path);
        }

        public static async Task<List<T>> FindTreeBadNodes<T>(this IQueryable<T> data_source)
            where T : TreeEntityBase
        {
            var list = await data_source.ToListAsync();
            var error_list = new List<string>();

            foreach (var node in list.OrderByDescending(x => x.Level))
            {
                if (error_list.Contains(node.UID))
                {
                    //防止中间节点被重复计算
                    //node1->node2->node3->node4->node5
                    //计算了node1到node5为错误节点之后将跳过1,2,3,4的检查
                    continue;
                }
                var check = await data_source.CheckNodeIfCanFindRoot(node);
                if (!check.success)
                {
                    error_list.AddRange(check.node_path.Select(x => x.UID));
                }
            }

            return list.Where(x => error_list.Distinct().Contains(x.UID)).ToList();
        }

        #endregion

        public static async Task<_<string>> AddTreeNode<T>(this IRepository<T> repo,
            T model, string model_flag)
            where T : TreeEntityBase
        {
            var data = new _<string>();
            if (model.ParentUID == TreeEntityBase.FIRST_PARENT_UID)
            {
                model.Level = TreeEntityBase.FIRST_LEVEL;
            }
            else
            {
                var parent = await repo.GetFirstAsync(x => x.UID == model.ParentUID);
                Com.AssertNotNull(parent, "父节点为空");
                model.Level = parent.Level + 1;
            }
            model.Init(model_flag);
            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await repo.AddAsync(model) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("添加菜单失败");
        }

        public static async Task<List<T>> QueryNodeList<T>(this ILinqRepository<T> repo,
            string parent = null, int max = 5000)
            where T : TreeEntityBase
        {
            return await repo.PrepareIQueryableAsync_(async query =>
            {
                if (ValidateHelper.IsPlumpString(parent))
                {
                    query = query.Where(x => x.ParentUID == parent);
                }
                return await query.OrderByDescending(x => x.UpdateTime).Take(max).ToListAsync();
            });
        }

        public static async Task<_<string>> DeleteTreeNodeRecursively<T>(this IRepository<T> repo, string node_uid)
            where T : TreeEntityBase
        {
            var data = new _<string>();
            var list = await repo.GetListEnsureMaxCountAsync(null, 5000, "树节点数量达到上线");
            var permission = list.Where(x => x.UID == node_uid).FirstOrThrow($"节点不存在{node_uid}");

            var dead_nodes = await list.AsQueryable().FindNodeChildrenRecursively_(permission);

            if (await repo.DeleteAsync(dead_nodes.ToArray()) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("删除节点错误");
        }
    }
}
