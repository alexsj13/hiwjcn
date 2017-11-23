using Bll.Category;
using Hiwjcn.Web.Models.Category;
using Lib.helper;
using Lib.mvc;
using Model.Category;
using System;
using System.Linq;
using System.Web.Mvc;
using Lib.extension;

namespace WebApp.Areas.Admin.Controllers
{
    public class CategoryController : WebCore.MvcLib.Controller.UserBaseController
    {
        /// <summary>
        /// 分类管理
        /// </summary>
        /// <returns></returns>
        public ActionResult CategoryManage(string type)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new CategoryManageViewModel();
                //读取树的数据
                var bll = new CategoryBll();

                {
                    //加载所有类型
                    model.TypesList = bll.GetTop();
                    var cookie_key = "deft_category";
                    if (!ValidateHelper.IsPlumpString(type))
                    {
                        type = this.X.context.GetCookie(cookie_key);
                    }
                    if (!ValidateHelper.IsPlumpString(type) && ValidateHelper.IsPlumpList(model.TypesList))
                    {
                        type = model.TypesList[0];
                    }
                    if (!ValidateHelper.IsPlumpString(type))
                    {
                        return Content("未指定类型");
                    }
                    //3个月过期
                    this.X.context.SetCookie(cookie_key, type, expires_minutes: 60 * 24 * 30);
                    model.CategoryType = type;
                }

                var list = bll.GetCategoryByType(type);
                model.HasNodes = ValidateHelper.IsPlumpList(list);
                if (model.HasNodes)
                {
                    //错误节点
                    model.ErrList = bll.AnalysisTreeStructureAndGetErrorNodesList(list);
                    if (!ValidateHelper.IsPlumpList(model.ErrList))
                    {
                        list = list.OrderByDescending(x => x.OrderNum).ToList();
                        //没有错误节点，返回Ztree的json
                        var data = list.Select(x => new
                        {
                            id = x.UID,
                            pId = x.CategoryParent,
                            name = x.OrderNum + "-" + x.CategoryName + "-" + x.CategoryDescription,

                            description = x.CategoryDescription,
                            link_url = x.LinkURL,
                            order_num = x.OrderNum,
                            open_in_new_window = x.OpenInNewWindow,
                            icon_class = x.IconClass,
                            image_url = x.CategoryImage
                        }).ToList();

                        model.Json = JsonHelper.ObjectToJson(data);
                    }
                }
                else
                {
                    return InitTree(type);
                }
                ViewData["model"] = model;
                return View();
            });
        }

        [NonAction]
        private ActionResult InitTree(string type)
        {
            var treemodel = new CategoryModel();
            treemodel.CategoryName = $"新建的默认节点-{DateTime.Now.ToString()}";
            treemodel.CategoryParent = CategoryBll.FIRST_PARENT;
            treemodel.CategoryLevel = CategoryBll.FIRST_LEVEL;
            treemodel.OrderNum = 0;
            treemodel.CategoryType = type;

            var bll = new CategoryBll();
            var res = bll.AddNode(treemodel);
            if (!ValidateHelper.IsPlumpString(res))
            {
                return Redirect($"/Admin/Category/{nameof(CategoryManage)}?type={type}");
            }
            else
            {
                return Content(res);
            }
        }

        /// <summary>
        /// 编辑节点
        /// </summary>
        /// <returns></returns>
        public ActionResult EditNode(string node_id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var bll = new CategoryBll();
                var model = bll.GetCategoryByID(node_id);
                if (model == null) { return Content("节点不存在"); }
                ViewData["model"] = model;
                return View();
            });
        }

        /// <summary>
        /// 清理树
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ClearTree(string category_type)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                if (!ValidateHelper.IsPlumpString(category_type))
                {
                    return GetJsonRes("节点类型未指定");
                }
                var bll = new CategoryBll();
                var list = bll.GetCategoryByType(category_type);
                var errlist = bll.AnalysisTreeStructureAndGetErrorNodesList(list);
                if (ValidateHelper.IsPlumpList(errlist))
                {
                    string res = bll.DeleteSingleNodeByIDS(errlist.Select(x => x.UID).ToArray());
                    return GetJsonRes(res);
                }
                else
                {
                    return GetJsonRes("没有找到需要清理的节点");
                }
            });
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteNode(string id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var bll = new CategoryBll();
                var res = bll.DeleteNodesAndChildren(id);
                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddChildLevel(string id, string name, string desc, string link, string img, string icon, string target, int? order, string type)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new CategoryModel();
                model.UID = id;
                model.CategoryName = name;
                model.CategoryDescription = desc;
                model.LinkURL = link;
                model.CategoryImage = img;
                model.IconClass = icon;
                model.OpenInNewWindow = this.ParseBlankWindow(target);
                model.OrderNum = order ?? 0;
                model.CategoryType = type;

                var bll = new CategoryBll();
                var parentModel = bll.GetCategoryByID(model.UID);
                if (parentModel == null)
                {
                    return GetJsonRes("父级节点不存在");
                }
                model.CategoryLevel = parentModel.CategoryLevel + 1;
                model.CategoryParent = parentModel.UID;
                var res = bll.AddNode(model);
                return GetJsonRes(res);
            });
        }

        [NonAction]
        private int ParseBlankWindow(string target) => (ConvertHelper.GetString(target).Trim().ToLower() == "blank").ToString().ToBoolInt();

        /// <summary>
        /// 添加同级节点
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSameLevel(string id, string name, string desc, string link, string img, string icon, string target, int? order, string type)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new CategoryModel();
                model.UID = id;
                model.CategoryName = name;
                model.CategoryDescription = desc;
                model.LinkURL = link;
                model.CategoryImage = img;
                model.IconClass = icon;
                model.OpenInNewWindow = this.ParseBlankWindow(target);
                model.OrderNum = order ?? 0;
                model.CategoryType = type;

                var bll = new CategoryBll();
                var currentModel = bll.GetCategoryByID(model.UID);
                if (currentModel == null)
                {
                    return GetJsonRes("节点不存在");
                }
                model.CategoryLevel = currentModel.CategoryLevel;
                model.CategoryParent = currentModel.CategoryParent;
                var res = bll.AddNode(model);
                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateNode(string id, string name, string desc, string link, string img, string icon, string target, int? order, string type)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new CategoryModel();
                model.UID = id;
                model.CategoryName = name;
                model.CategoryDescription = desc;
                model.LinkURL = link;
                model.CategoryImage = img;
                model.IconClass = icon;
                model.OpenInNewWindow = this.ParseBlankWindow(target);
                model.OrderNum = order ?? 0;
                model.CategoryType = type;

                var bll = new CategoryBll();
                var res = bll.UpdateNode(model);
                return GetJsonRes(res);
            });
        }

    }
}
