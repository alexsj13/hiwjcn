using Bll;
using Hiwjcn.Core.Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using Lib.helper;
using Hiwjcn.Dal.Sys;
using Lib.infrastructure;

namespace Hiwjcn.Bll.Sys
{
    public class ReqLogBll : ServiceBase<ReqLogModel>
    {
        public ReqLogBll()
        {
            //
        }

        public string AddLog(ReqLogModel model)
        {
            var err = CheckModel(model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            var dal = new ReqLogDal();
            return dal.Add(model) > 0 ? SUCCESS : "添加错误";
        }

        public List<ReqLogGroupModel> GetGroupedList(DateTime? start = null, int count = 50)
        {
            var dal = new ReqLogDal();
            List<ReqLogGroupModel> list = null;
            dal.PrepareIQueryable((query) =>
            {
                if (start != null)
                {
                    query = query.Where(x => x.UpdateTime > start.Value);
                }
                list = query.GroupBy(x => new { x.AreaName, x.ControllerName, x.ActionName })
                    .Select(x => new ReqLogGroupModel()
                    {
                        AreaName = x.Key.AreaName,
                        ControllerName = x.Key.ControllerName,
                        ActionName = x.Key.ActionName,
                        ReqTime = x.Average(m => m.ReqTime),
                        ReqCount = x.Count()
                    })
                    .OrderByDescending(x => x.ReqTime)
                    .Skip(0).Take(count).ToList();
                return true;
            });
            return list;
        }

        public List<ReqLogModel> GetList(DateTime? start = null,
            string area = null, string controller = null, string action = null,
            int count = 50)
        {
            var dal = new ReqLogDal();
            List<ReqLogModel> list = null;
            dal.PrepareIQueryable((query) =>
            {
                if (start != null)
                {
                    query = query.Where(x => x.UpdateTime > start.Value);
                }
                if (ValidateHelper.IsPlumpString(area))
                {
                    query = query.Where(x => x.AreaName == area);
                }
                if (ValidateHelper.IsPlumpString(controller))
                {
                    query = query.Where(x => x.ControllerName == controller);
                }
                if (ValidateHelper.IsPlumpString(action))
                {
                    query = query.Where(x => x.ActionName == action);
                }
                list = query.OrderByDescending(x => x.ReqTime).Skip(0).Take(count).ToList();
                return true;
            });
            return list;
        }

    }
}
