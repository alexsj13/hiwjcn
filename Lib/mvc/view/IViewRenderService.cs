using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mvc.view
{
    public interface IViewRenderService
    {
        string Render(string viewPath);
        string Render<T>(string viewPath, T model);
    }
}
