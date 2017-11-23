using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.IO;

namespace Lib.mvc.view
{
    public class RazorViewRenderService : IViewRenderService
    {
        //
        public string Render(string viewPath)
        {
            throw new NotImplementedException();
        }

        public string Render<T>(string viewPath, T model)
        {
            throw new NotImplementedException();
        }
    }
}
