using Hiwjcn.Core.Infrastructure.Page;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebLogic.Model.Page;

namespace WebApp.Areas.Admin.Controllers
{
    public class SectionController : WebCore.MvcLib.Controller.UserBaseController
    {
        private IPageService _IPageService { get; set; }

        public SectionController(IPageService page)
        {
            this._IPageService = page;
        }

        /// <summary>
        /// 内容块列表
        /// </summary>
        /// <returns></returns>
        public ActionResult SectionList(string q, int? page)
        {
            this.PermissionList = new List<string>() { };
            return RunActionWhenLogin((loginuser) =>
            {
                int pageSize = 16;
                page = CheckPage(page);

                var data = _IPageService.GetSectionList(q: q, page: page.Value, pagesize: pageSize);

                if (data != null)
                {
                    ViewData["list"] = data.DataList;
                    ViewData["pager"] = data.GetPagerHtml(this, "page", page.Value, pageSize);
                }
                return View();
            });
        }

        /// <summary>
        /// 删除页面
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteSectionAction(string name)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var res = _IPageService.DeleteSections(name);

                return GetJson(new { success = !ValidateHelper.IsPlumpString(res), msg = res });
            });
        }

        /// <summary>
        /// 更新或者添加页面
        /// </summary>
        /// <param name="section_name"></param>
        /// <param name="section_title"></param>
        /// <param name="section_description"></param>
        /// <param name="section_content"></param>
        /// <param name="section_type"></param>
        /// <param name="rel_group"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateOrSaveSectionAction(int? id,
            string section_name, string section_title, string section_description,
            string section_content, string section_type, string rel_group)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                id = id ?? 0;

                var model = new SectionModel();
                model.IID = id.Value;
                model.SectionName = section_name;
                model.SectionTitle = section_title;
                model.SectionDescription = section_description;
                model.SectionContent = section_content;
                model.SectionType = section_type;
                model.RelGroup = rel_group;
                model.UpdateTime = DateTime.Now;

                var res = string.Empty;

                if (model.IID > 0)
                {
                    //update
                    res = _IPageService.UpdateSection(model);
                }
                else
                {
                    //add
                    res = _IPageService.AddSection(model);
                }

                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 页面编辑页面
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult SectionEdit(string name)
        {
            this.PermissionList = new List<string>() { };
            return RunActionWhenLogin((loginuser) =>
            {
                SectionModel model = _IPageService.GetSection(name);
                ViewData["model"] = model;

                return View();
            });
        }

    }
}
