using Bll;
using Hiwjcn.Core.Infrastructure.Common;
using Lib.helper;
using Lib.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebLogic.Dal.Sys;
using WebLogic.Model.Sys;
using Lib.infrastructure;

namespace WebLogic.Bll.Sys
{
    /// <summary>
    /// 全国省市区
    /// </summary>
    public class AreaService : ServiceBase<AreaModel>, IAreaService
    {
        public static readonly int FIRST_LEVEL = 1;
        public static readonly string FIRST_PARENT = "0";

        public AreaService()
        {
            //
        }

        /// <summary>
        /// 通过父级获取子集
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public List<AreaModel> GetAreas(int level, string parent, int max_count = 500)
        {
            if (level < 0 || !ValidateHelper.IsPlumpString(parent)) { return null; }

            var dal = new AreaDal();
            return dal.GetList(x => x.AreaLevel == level && x.ParentID == parent, count: max_count);
        }

    }
}
