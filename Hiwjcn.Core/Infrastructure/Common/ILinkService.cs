using Lib.core;
using Lib.infrastructure;
using Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.Page;

namespace Hiwjcn.Core.Infrastructure.Common
{
    public interface ILinkService : IServiceBase<LinkModel>
    {
        LinkModel GetLinkByID(string id);

        /// <summary>
        /// 读取链接
        /// </summary>
        /// <param name="link_type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<LinkModel> GetTopLinks(string link_type, int count = 99999);

        /// <summary>
        /// 删除链接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string DeleteLink(string id);

        /// <summary>
        /// 添加链接
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string AddLink(LinkModel model);

        /// <summary>
        /// 更新链接
        /// </summary>
        /// <param name="updatemodel"></param>
        /// <returns></returns>
        string UpdateLink(LinkModel updatemodel);
    }
}
