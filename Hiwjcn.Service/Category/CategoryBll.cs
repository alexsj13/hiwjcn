using Dal.Category;
using Lib.helper;
using Lib.infrastructure;
using Model.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using Lib.extension;

namespace Bll.Category
{
    /// <summary>
    /// 分类、导航、层级关系
    /// </summary>
    public class CategoryBll : ServiceBase<CategoryModel>
    {
        /// <summary>
        /// 第一层级
        /// </summary>
        public static readonly int FIRST_LEVEL = 0;
        /// <summary>
        /// 第一层的父级
        /// </summary>
        public static readonly string FIRST_PARENT = string.Empty;

        private CategoryDal _CategoryDal { get; set; }

        public CategoryBll()
        {
            this._CategoryDal = new CategoryDal();
        }

        public override void CustomCheckModel(ref CategoryModel model, ref List<string> errors)
        {
            if (model.CategoryLevel < FIRST_LEVEL)
            {
                errors.Add("层级参数为空");
            }
        }

        /// <summary>
        /// 获取type
        /// </summary>
        /// <returns></returns>
        public List<string> GetTop(int count = 1000)
        {
            List<string> list = null;

            _CategoryDal.PrepareIQueryable((query) =>
            {
                list = query.Where(x => x.CategoryType != null && x.CategoryType != string.Empty)
                    .Select(x => x.CategoryType).Distinct().Take(count).ToList();
                return true;
            });

            return list;
        }

        /// <summary>
        /// 根据ID获取节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CategoryModel GetCategoryByID(string id)
        {
            return this._CategoryDal.GetFirst(x => x.UID == id);
        }

        /// <summary>
        /// 根据类型获取数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public List<CategoryModel> GetCategoryByType(string type, int maxCount = 500)
        {
            if (!ValidateHelper.IsPlumpString(type))
            {
                return null;
            }
            return GetList(categoryType: type, count: maxCount);
        }

        /// <summary>
        /// 根据条件获取分类列表
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        public List<CategoryModel> GetList(List<string> ids = null,
            string parent = null, int level = -99,
            string categoryType = null, int count = 500)
        {
            List<CategoryModel> list = null;
            _CategoryDal.PrepareIQueryable((query) =>
            {
                if (ValidateHelper.IsPlumpList(ids))
                {
                    query = query.Where(x => ids.Contains(x.UID));
                }
                if (ValidateHelper.IsPlumpString(parent))
                {
                    query = query.Where(x => x.CategoryParent == parent);
                }
                if (level >= CategoryBll.FIRST_LEVEL)
                {
                    query = query.Where(x => x.CategoryLevel == level);
                }
                if (ValidateHelper.IsPlumpString(categoryType))
                {
                    query = query.Where(x => x.CategoryType == categoryType);
                }

                //读取最大限制
                query = query.OrderByDescending(x => x.OrderNum).Skip(0).Take(count);
                list = query.ToList();
                return true;
            });
            return list;
        }

        /// <summary>
        /// 从一个节点向上找直到根节点
        /// </summary>
        /// <param name="list"></param>
        /// <param name="node"></param>
        /// <param name="SuccessIDSList"></param>
        /// <returns></returns>
        private bool NodeCanFindRoot(List<CategoryModel> list, CategoryModel node, ref List<string> SuccessIDSList)
        {
            //临时存放id
            var CurrentNodeID = node.UID;
            int CurrentLevel = node.CategoryLevel;
            var CurrentParent = node.CategoryParent;
            //历遍过的ID
            var handleredIDS = new List<string>();
            //从这个节点一直向上到，直到找到第一级
            while ((node = list.Where(x => x.UID == CurrentNodeID).FirstOrDefault()) != null)
            {
                //标记已经处理过的节点
                handleredIDS.Add(node.UID);

                {
                    //获取层级
                    CurrentLevel = node.CategoryLevel;
                    //获取父级
                    CurrentParent = node.CategoryParent;
                }

                //如果这个节点已经处理过说明死循环了，立即退出
                if (handleredIDS.Contains(node.CategoryParent))
                {
                    LogHelper.Error(this.GetType(), "树存在死循环:" + node.CategoryType);
                    break;
                }

                //设置上一层级的id
                CurrentNodeID = node.CategoryParent;
            }
            //如果最后一个节点是第一层并且父级是默认就表示节点无错误
            bool success = CurrentLevel == CategoryBll.FIRST_LEVEL && CurrentParent == CategoryBll.FIRST_PARENT;
            //如果是正确的树结构就添加到成功列表
            if (success)
            {
                SuccessIDSList.AddRange(handleredIDS);
                SuccessIDSList = SuccessIDSList.Distinct().ToList();
            }
            return success;
        }

        private bool NodeCanFindRoot(List<CategoryModel> list, ref List<string> SuccessIDSList)
        {
            foreach (var node in list)
            {
                //临时存放id
                var CurrentNodeID = node.UID;
                int CurrentLevel = node.CategoryLevel;
                var CurrentParent = node.CategoryParent;
                var CurrentNode = default(CategoryModel);
                //历遍过的ID
                var handleredIDS = new List<string>();
                //从这个节点一直向上到，直到找到第一级
                while ((CurrentNode = list.Where(x => x.UID == CurrentNodeID).FirstOrDefault()) != null)
                {
                    //标记已经处理过的节点
                    if (handleredIDS.Contains(CurrentNode.UID))
                    {
                        $"树存在死循环,type:{CurrentNode.CategoryType}".AddErrorLog("树存在死循环");
                        return false;
                    }

                    //获取层级
                    CurrentLevel = CurrentNode.CategoryLevel;
                    //获取父级
                    CurrentParent = CurrentNode.CategoryParent;

                    //如果这个节点已经处理过说明死循环了，立即退出
                    handleredIDS.Add(CurrentNode.UID);

                    //下一个循环
                    CurrentNodeID = CurrentNode.CategoryParent;
                }
                //如果最后一个节点是第一层并且父级是默认就表示节点无错误
                bool success = CurrentLevel == CategoryBll.FIRST_LEVEL && CurrentParent == CategoryBll.FIRST_PARENT;
                //如果是正确的树结构就添加到成功列表
                if (success)
                {
                    SuccessIDSList.AddRange(handleredIDS);
                }
                else
                {
                    $"树不完整,type:{CurrentNode.CategoryType}".AddErrorLog("树不完整");
                    return false;
                }
            }
            SuccessIDSList = SuccessIDSList.Distinct().ToList();
            return true;
        }

        /// <summary>
        /// 分析树的结构，返回错误的节点
        /// </summary>
        public List<CategoryModel> AnalysisTreeStructureAndGetErrorNodesList(List<CategoryModel> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return null; }
            var successIDS = new List<string>();
            list.OrderByDescending(x => x.CreateTime).ToList().ForEach(node =>
            {
                //如果已经成功的就跳过
                if (successIDS.Contains(node.UID)) { return; }
                NodeCanFindRoot(list, node, ref successIDS);
            });
            //错误节点列表
            return list.Where(x => !successIDS.Contains(x.UID)).ToList();
        }

        /// <summary>
        /// 删除所有的节点
        /// </summary>
        /// <returns></returns>
        public string DeleteTree(string type)
        {
            if (!ValidateHelper.IsPlumpString(type))
            {
                return "未指定类型";
            }
            int count = 0;
            var list = _CategoryDal.GetList(x => x.CategoryType == type);
            if (ValidateHelper.IsPlumpList(list))
            {
                count = _CategoryDal.Delete(list.ToArray());
            }
            return count > 0 ? SUCCESS : "删除失败";
        }

        /// <summary>
        /// 先查出一棵树的所有节点再递归
        /// </summary>
        /// <param name="allNodesStore"></param>
        /// <param name="startNodes"></param>
        /// <param name="finder"></param>
        /// <param name="findedIDS"></param>
        private void FindAllNodesCore(
            List<CategoryModel> allNodesStore,
            List<CategoryModel> startNodes,
            Func<CategoryModel, bool> finder,
            ref List<string> findedIDS)
        {
            //条件不成熟就返回
            if (!ValidateHelper.IsPlumpList(allNodesStore) || finder == null || !ValidateHelper.IsPlumpList(startNodes)) { return; }
            //去掉null
            startNodes = startNodes.Where(x => x != null).ToList();
            foreach (var node in startNodes)
            {
                //如果节点已经被处理过了就跳过，防止死循环
                if (findedIDS.Contains(node.UID)) { continue; }

                //如果方法返回false就中断
                if (!finder.Invoke(node)) { return; }

                //处理过的节点添加到已处理列表
                findedIDS.Add(node.UID);

                //找到当前节点下的子节点
                var children = allNodesStore.Where(x => x.CategoryParent == node.UID).ToList();
                //递归子节点
                FindAllNodesCore(allNodesStore, children, finder, ref findedIDS);
            }
        }

        /// <summary>
        /// 先查出一棵树的所有节点再递归
        /// </summary>
        /// <param name="allNodesStore"></param>
        /// <param name="startNodes"></param>
        /// <param name="finder"></param>
        private void FindAllNodes(
            List<CategoryModel> allNodesStore,
            List<CategoryModel> startNodes,
            Func<CategoryModel, bool> finder)
        {
            //已经找过的节点
            var findedIDS = new List<string>();
            FindAllNodesCore(allNodesStore, startNodes, finder, ref findedIDS);
        }

        /// <summary>
        /// 删除节点和子节点
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public string DeleteNodesAndChildren(string nodeID)
        {
            var root = _CategoryDal.GetFirst(x => x.UID == nodeID);
            Com.Assert(root, x => x != null, "您要删除的节点不存在");

            //获取这棵树下的所有节点
            var allNodesStore = _CategoryDal.GetList(x => x.CategoryType == root.CategoryType);
            if (!ValidateHelper.IsPlumpList(allNodesStore)) { return "读取数据出现错误，请联系管理员"; }

            //需要删除的id
            var DeletedIDS = new List<string>();
            //通过递归获取所有id
            FindAllNodes(allNodesStore, new List<CategoryModel>() { root }, (node) =>
            {
                //如果已经遍历过这个节点就终止递归，防止死循环
                if (DeletedIDS.Contains(node.UID)) { return false; }

                DeletedIDS.Add(node.UID);
                return true;
            });
            DeletedIDS = DeletedIDS.Where(x => ValidateHelper.IsPlumpString(x)).ToList();
            if (!ValidateHelper.IsPlumpList(DeletedIDS))
            {
                return "没有获取到要删除的ID";
            }
            return DeleteSingleNodeByIDS(DeletedIDS.ToArray());
        }

        /// <summary>
        /// 删除单个点，不递归删除子节点
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public string DeleteSingleNodeByIDS(params string[] ids)
        {
            if (!ValidateHelper.IsPlumpList(ids)) { return "未找到有效参数"; }
            int count = 0;
            var list = _CategoryDal.GetList(x => ids.Contains(x.UID));
            if (ValidateHelper.IsPlumpList(list))
            {
                count = _CategoryDal.Delete(list.ToArray());
            }
            return count > 0 ? SUCCESS : "删除了0个节点";
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="updateModel"></param>
        /// <returns></returns>
        public string UpdateNode(CategoryModel updateModel)
        {
            var model = _CategoryDal.GetFirst(x => x.UID == updateModel.UID);
            if (model == null) { return "您要修改的数据不存在"; }

            //名字
            model.CategoryName = updateModel.CategoryName;
            //描述
            model.CategoryDescription = updateModel.CategoryDescription;
            //URL
            model.LinkURL = updateModel.LinkURL;
            //图片
            model.CategoryImage = updateModel.CategoryImage;
            //背景图
            model.BackgroundImage = updateModel.BackgroundImage;
            //图标
            model.IconClass = updateModel.IconClass;
            //target
            model.OpenInNewWindow = updateModel.OpenInNewWindow;
            //排序
            model.OrderNum = updateModel.OrderNum;

            string check = CheckModel(model);
            if (ValidateHelper.IsPlumpString(check)) { return check; }
            return _CategoryDal.Update(model) > 0 ? SUCCESS : "修改失败";
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddNode(CategoryModel model)
        {
            string check = CheckModel(model);
            if (ValidateHelper.IsPlumpString(check)) { return check; }
            return _CategoryDal.Add(model) > 0 ? SUCCESS : "添加节点失败";
        }
    }
}
