using Bll;
using Hiwjcn.Dal.Tag;
using Lib.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.Tag;
using Lib.helper;
using Lib.infrastructure;

namespace Hiwjcn.Bll.Tag
{
    /// <summary>
    /// 标签关联
    /// </summary>
    public class TagMapBll : ServiceBase<TagMapModel>
    {
        public TagMapBll()
        {
            //
        }

        /// <summary>
        /// 保存标签
        /// </summary>
        /// <param name="mapkey"></param>
        /// <param name="maptype"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public string SaveTagsMap(string mapkey, string maptype, List<string> tags)
        {
            if (!ValidateHelper.IsAllPlumpString(mapkey, maptype)) { return "参数错误"; }
            tags = ConvertHelper.NotNullList(tags).Where(x => ValidateHelper.IsPlumpString(x)).Distinct().ToList();
            var mapdal = new TagMapDal();
            var deletedlist = mapdal.GetList(x => x.MapKey == mapkey && x.MapType == maptype);
            if (deletedlist == null) { deletedlist = new List<TagMapModel>(); }

            //交集(不用删除,不用添加)
            var mixlist = Com.GetInterSection(deletedlist.Select(x => x.TagName).Distinct().ToList(), tags);
            //不是交集内的数据删除
            deletedlist = deletedlist.Where(x => !mixlist.Contains(x.TagName)).ToList();
            //不是交集内的数据添加
            tags = tags.Where(x => !mixlist.Contains(x)).ToList();

            if (ValidateHelper.IsPlumpList(deletedlist))
            {
                if (mapdal.Delete(deletedlist.ToArray()) != deletedlist.Count())
                {
                    return "清理旧数据失败";
                }
            }
            if (!ValidateHelper.IsPlumpList(tags)) { return SUCCESS; }
            var addlist = tags.Select(x => new TagMapModel()
            {
                MapKey = mapkey,
                TagName = x,
                MapType = maptype
            }).ToList();

            return mapdal.Add(addlist.ToArray()) == addlist.Count() ? SUCCESS : "保存失败";
        }

    }
}
