using Autofac.Extras.DynamicProxy;
using Dal.Sys;
using Hiwjcn.Bll;
using Hiwjcn.Core.Infrastructure.Common;
using Lib.helper;
using Lib.core;
using Model.Sys;
using System.Collections.Generic;
using System.Linq;
using Lib.infrastructure;

namespace Bll.Sys
{
    /// <summary>
    /// 链接
    /// </summary>
    [Intercept(typeof(AopLogError_))]
    public class LinkService : ServiceBase<LinkModel>, ILinkService
    {
        private LinkDal _LinkDal { get; set; }
        public LinkService()
        {
            this._LinkDal = new LinkDal();
        }

        /// <summary>
        /// 根据ID获取连接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LinkModel GetLinkByID(string id)
        {
            return _LinkDal.GetFirst(x => x.UID == id);
        }

        /// <summary>
        /// 读取链接
        /// </summary>
        /// <param name="link_type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual List<LinkModel> GetTopLinks(string link_type, int count = 99999)
        {
            if (!ValidateHelper.IsPlumpString(link_type) || count <= 0)
            {
                return null;
            }

            var list = _LinkDal.QueryList(
                where: x => x.LinkType == link_type,
                orderby: x => x.OrderNum, Desc: true,
                start: 0, count: count);
            return list;
        }

        /// <summary>
        /// 删除链接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string DeleteLink(string id)
        {
            var model = _LinkDal.GetFirst(x => x.UID == id);
            if (model == null) { return "记录不存在"; }
            return _LinkDal.Delete(model) > 0 ? SUCCESS : "删除失败";
        }

        /// <summary>
        /// 添加链接
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddLink(LinkModel model)
        {
            string check = CheckModel(model);
            if (ValidateHelper.IsPlumpString(check))
            {
                return check;
            }
            return _LinkDal.Add(model) > 0 ? SUCCESS : "添加失败";
        }

        /// <summary>
        /// 更新链接
        /// </summary>
        /// <param name="updatemodel"></param>
        /// <returns></returns>
        public string UpdateLink(LinkModel updatemodel)
        {
            var model = _LinkDal.GetByKeys(updatemodel.IID);
            if (model == null) { return "链接不存在"; }
            model.Image = updatemodel.Image;
            model.Name = updatemodel.Name;
            model.OrderNum = updatemodel.OrderNum;
            model.Target = updatemodel.Target;
            model.Title = updatemodel.Title;
            model.Url = updatemodel.Url;

            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo)) { return errinfo; }
            return _LinkDal.Update(model) > 0 ? SUCCESS : "更新链接失败";
        }
    }
}
