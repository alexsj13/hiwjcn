using Hiwjcn.Bll.Tag;
using Lib.helper;
using System.Web.Mvc;
using WebLogic.Model.Tag;

namespace WebApp.Areas.Admin.Controllers
{
    public class TagController : WebCore.MvcLib.Controller.UserBaseController
    {
        public TagBll _TagBll = new TagBll();

        public ActionResult TagManage()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                ViewData["list"] = _TagBll.GetList();
                return View();
            });
        }

        public ActionResult TagEdit(string id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                if (ValidateHelper.IsPlumpString(id))
                {
                    var model = _TagBll.GetTagByID(id);
                    ViewData["model"] = model;
                }
                return View();
            });
        }

        [HttpPost]
        public ActionResult SaveTagAction(int? id, string name, string desc, string link)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new TagModel();
                model.IID = id ?? 0;
                model.TagName = name;
                model.TagDesc = desc;
                model.TagLink = link;
                var res = string.Empty;
                if (model.IID > 0)
                {
                    res = _TagBll.UpdateTag(model);
                }
                else
                {
                    model.ItemCount = 0;
                    res = _TagBll.AddTag(model);
                }
                return GetJsonRes(res);
            });
        }

        [HttpPost]
        public ActionResult DeleteTagAction(string id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var res = _TagBll.DeleteTag(id);
                return GetJsonRes(res);
            });
        }

    }
}