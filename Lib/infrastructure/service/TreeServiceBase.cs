using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Data.Entity;
using Lib.infrastructure.entity;
using Lib.infrastructure.extension;

namespace Lib.infrastructure.service
{
    /// <summary>
    /// 树结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeServiceBase<T> : IServiceBase<T>
        where T : TreeEntityBase
    {
        Task<List<T>> FindNodeChildrenRecursively_(IQueryable<T> data_source, T first_node,
               string tree_error = null);

        Task<(bool success, List<T> node_path)> CheckNodeIfCanFindRoot(IQueryable<T> data_source, T first_node);

        Task<List<T>> FindTreeBadNodes(IQueryable<T> data_source);
    }

    /// <summary>
    /// 树结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TreeServiceBase<T> : ServiceBase<T>, ITreeServiceBase<T>
        where T : TreeEntityBase
    {
        public virtual async Task<List<T>> FindNodeChildrenRecursively_(IQueryable<T> data_source, T first_node,
            string tree_error = "树存在无限递归") =>
            await data_source.FindNodeChildrenRecursively_(first_node, tree_error);

        public virtual async Task<(bool success, List<T> node_path)> CheckNodeIfCanFindRoot(IQueryable<T> data_source, T first_node) =>
            await data_source.CheckNodeIfCanFindRoot(first_node);

        public virtual async Task<List<T>> FindTreeBadNodes(IQueryable<T> data_source) =>
            await data_source.FindTreeBadNodes();
    }
}
