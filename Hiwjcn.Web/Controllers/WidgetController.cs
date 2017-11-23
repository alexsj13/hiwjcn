using Bll.Category;
using Lib.mvc.attr;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    [HideAction]
    public class WidgetController : WebCore.MvcLib.Controller.UserBaseController
    {
        private CategoryBll _CategoryBll { get; set; }
        public WidgetController()
        {
            this._CategoryBll = new CategoryBll();
        }

        [ChildActionOnly]
        public ActionResult Nav()
        {
            ViewData["nav_list"] = _CategoryBll.GetCategoryByType("nav_list", maxCount: 500);

            return PartialView();
        }

    }
}
