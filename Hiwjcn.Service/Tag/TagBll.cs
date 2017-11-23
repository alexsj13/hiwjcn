using Bll;
using Hiwjcn.Dal.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.Tag;
using Lib.helper;
using Lib.core;
using Lib.infrastructure;

namespace Hiwjcn.Bll.Tag
{
    public class TagBll : ServiceBase<TagModel>
    {
        public TagBll()
        {
            //
        }

        /// <summary>
        /// 通过id获取标签
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TagModel GetTagByID(string id)
        {
            var tagdal = new TagDal();
            return tagdal.GetFirst(x => x.UID == id);
        }

        /// <summary>
        /// 获取标签
        /// </summary>
        /// <param name="tagtype"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public List<TagModel> GetList(List<string> ids = null, int skip = 0, int take = 1000)
        {
            var tagdal = new TagDal();
            List<TagModel> list = null;
            tagdal.PrepareIQueryable((query) =>
            {
                if (ValidateHelper.IsPlumpList(ids))
                {
                    query = query.Where(x => ids.Contains(x.UID));
                }
                list = query.OrderByDescending(x => x.CreateTime).Skip(skip).Take(take).ToList();
                return true;
            });
            return list;
        }

        /// <summary>
        /// 添加标签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddTag(TagModel model)
        {
            string err = CheckModel(model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            var tagdal = new TagDal();
            if (tagdal.Exist(x => x.TagName == model.TagName)) { return "存在同名标签"; }

            return tagdal.Add(model) > 0 ? SUCCESS : "添加失败";
        }

        /// <summary>
        /// 更新标签
        /// </summary>
        /// <param name="updatemodel"></param>
        /// <returns></returns>
        public string UpdateTag(TagModel updatemodel)
        {
            var tagdal = new TagDal();
            var model = tagdal.GetFirst(x => x.UID == updatemodel.UID);
            if (model == null) { return "数据不存在"; }

            model.TagName = updatemodel.TagName;
            model.TagDesc = updatemodel.TagDesc;
            model.TagLink = updatemodel.TagLink;

            if (tagdal.Exist(x => x.TagName == model.TagName && x.UID != model.UID)) { return "存在同名标签"; }

            return tagdal.Update(model) > 0 ? SUCCESS : "添加失败";
        }

        public string DeleteTag(string id)
        {
            var tagdal = new TagDal();
            var model = tagdal.GetFirst(x => x.UID == id);
            if (model == null) { return "数据不存在"; }
            return tagdal.Delete(model) > 0 ? SUCCESS : "删除失败";
        }

    }
}
