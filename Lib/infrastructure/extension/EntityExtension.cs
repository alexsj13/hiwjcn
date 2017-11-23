using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure.entity;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;

namespace Lib.infrastructure.extension
{
    public static class EntityExtension
    {
        public static async Task<_<string>> DeleteByMultipleUIDS<T>(this IRepository<T> repo, params string[] uids)
            where T : BaseEntity
        {
            var data = new _<string>();
            if (!ValidateHelper.IsPlumpList(uids))
            {
                data.SetErrorMsg("ID为空");
                return data;
            }
            if (await repo.DeleteWhereAsync(x => uids.Contains(x.UID)) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("删除数据错误");
        }
        
        public static async Task<_<string>> AddEntity<T>(this IRepository<T> repo, T model, string model_flag)
            where T : BaseEntity
        {
            var data = new _<string>();

            model.Init(model_flag);
            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await repo.AddAsync(model) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("保存失败");
        }
    }
}
