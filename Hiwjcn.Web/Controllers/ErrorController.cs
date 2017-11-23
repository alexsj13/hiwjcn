using Lib.mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    /// <summary>
    /// 不要继承basecontroller
    /// </summary>
    public class ErrorController : Controller
    {

        public ActionResult Http404()
        {
            return new Http404();
        }

        public ActionResult Http403()
        {
            return new Http403();
        }

        public ActionResult Http500()
        {
            return new Http500();
        }

    }
}
