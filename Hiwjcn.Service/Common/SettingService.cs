using Dal.Sys;
using Hiwjcn.Core.Infrastructure.Common;
using Lib.helper;
using Lib.infrastructure;
using Model.Sys;
using System.Collections.Generic;

namespace Bll.Sys
{
    /// <summary>
    /// 系统配置
    /// </summary>
    public class SettingService : ServiceBase<OptionModel>, ISettingService
    {
        private OptionDal _OptionDal { get; set; }

        public SettingService()
        {
            this._OptionDal = new OptionDal();
        }

        /// <summary>
        /// 保存配置对象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveOption(OptionModel model)
        {
            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo)) { return errinfo; }
            var list = _OptionDal.QueryList<object>(where: x => x.Key == model.Key);
            if (ValidateHelper.IsPlumpList(list))
            {
                _OptionDal.Delete(list.ToArray());
            }
            if (!ValidateHelper.IsPlumpString(model.Value))
            {
                return SUCCESS;
            }
            return _OptionDal.Add(model) > 0 ? SUCCESS : "修改失败";
        }

        /// <summary>
        /// 获取所有配置对象
        /// </summary>
        /// <returns></returns>
        public List<OptionModel> GetAllOptions()
        {
            return _OptionDal.GetList(null);
        }
    }
}
