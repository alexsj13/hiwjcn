using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.data;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.core;
using Lib.infrastructure.extension;
using Lib.data.ef;

namespace Lib.infrastructure.service
{
    public interface IMenuServiceBase<MenuBase, MenuGroupBase>
    {
        Task<_<string>> AddMenu(MenuBase model);

        Task<_<string>> DeleteMenuRecursively(string group_key, string menu_uid);

        Task<_<string>> DeleteMenus(params string[] menu_uids);

        Task<List<MenuBase>> QueryMenuList(string group_key, string parent = null);

        Task<_<string>> UpdateMenu(MenuBase model);

        Task<_<string>> AddMenuGroup(MenuGroupBase model);

        Task<_<string>> DeleteMenuGroup(params string[] group_uids);

        Task<_<string>> UpdateMenuGroup(MenuGroupBase model);

        Task<PagerData<MenuGroupBase>> QueryMenuGroupList(string q, int page = 1, int pagesize = 20);
    }

    public abstract class MenuServiceBase<MenuBase, MenuGroupBase> :
        IMenuServiceBase<MenuBase, MenuGroupBase>
        where MenuBase : MenuEntityBase, new()
        where MenuGroupBase : MenuGroupEntityBase, new()
    {
        private readonly IEFRepository<MenuBase> _menuRepo;
        private readonly IEFRepository<MenuGroupBase> _menuGroupRepo;

        public MenuServiceBase(
            IEFRepository<MenuBase> _menuRepo,
            IEFRepository<MenuGroupBase> _menuGroupRepo)
        {
            this._menuRepo = _menuRepo;
            this._menuGroupRepo = _menuGroupRepo;
        }

        public virtual async Task<_<string>> AddMenu(MenuBase model)
        {
            var data = new _<string>();
            if (!await this._menuGroupRepo.ExistAsync(x => x.GroupKey == model.GroupKey))
            {
                data.SetErrorMsg("菜单分组不存在");
                return data;
            }

            return await this._menuRepo.AddTreeNode(model, "mn");
        }

        public virtual async Task<_<string>> DeleteMenuRecursively(string group_key, string menu_uid)
        {
            if (!ValidateHelper.IsPlumpString(group_key)) { throw new ArgumentNullException(nameof(group_key)); }
            var data = new _<string>();
            var list = await this._menuRepo.GetListEnsureMaxCountAsync(x => x.GroupKey == group_key, 5000, "树结构节点数量达到上限");
            var node = list.Where(x => x.UID == menu_uid).FirstOrThrow("节点不存在");
            var dead_nodes = await list.AsQueryable().FindNodeChildrenRecursively_(node);

            if (await this._menuRepo.DeleteAsync(dead_nodes.ToArray()) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("删除错误");
        }

        public virtual async Task<_<string>> DeleteMenus(params string[] menu_uids) =>
            await this._menuRepo.DeleteByMultipleUIDS(menu_uids);

        public virtual async Task<List<MenuBase>> QueryMenuList(string group_key, string parent = null)
        {
            if (!ValidateHelper.IsPlumpString(group_key)) { throw new ArgumentNullException(nameof(group_key)); }
            return await this._menuRepo.PrepareIQueryableAsync_(async query =>
            {
                query = query.Where(x => x.GroupKey == group_key);
                if (ValidateHelper.IsPlumpString(parent))
                {
                    query = query.Where(x => x.ParentUID == parent);
                }

                return await query.OrderByDescending(x => x.Sort).Take(5000).ToListAsync();
            });
        }

        public abstract void UpdateMenuEntity(ref MenuBase old_menu, ref MenuBase new_menu);

        public virtual async Task<_<string>> UpdateMenu(MenuBase model)
        {
            var data = new _<string>();
            var menu = await this._menuRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(menu, "菜单不存在");
            menu.Update();
            if (!menu.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._menuRepo.UpdateAsync(menu) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新菜单错误");
        }

        public virtual async Task<_<string>> AddMenuGroup(MenuGroupBase model) =>
            await this._menuGroupRepo.AddEntity(model, "mn_group");

        public virtual async Task<_<string>> DeleteMenuGroup(params string[] group_uids) =>
            await this._menuGroupRepo.DeleteByMultipleUIDS(group_uids);

        public abstract void UpdateMenuGroupEntity(ref MenuGroupBase old_group, ref MenuGroupBase new_group);

        public virtual async Task<_<string>> UpdateMenuGroup(MenuGroupBase model)
        {
            var data = new _<string>();
            var group = await this._menuGroupRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(group, "分组不存在");
            this.UpdateMenuGroupEntity(ref group, ref model);
            group.Update();
            if (!group.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._menuGroupRepo.UpdateAsync(group) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新失败");
        }

        public virtual async Task<PagerData<MenuGroupBase>> QueryMenuGroupList(string q, int page = 1, int pagesize = 20)
        {
            var data = new PagerData<MenuGroupBase>();

            await this._menuGroupRepo.PrepareIQueryableAsync(async query =>
            {
                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.GroupName.Contains(q) || x.Description.Contains(q));
                }

                data.ItemCount = await query.CountAsync();
                data.DataList = await query.OrderByDescending(x => x.UpdateTime).QueryPage(page, pagesize).ToListAsync();
            });

            return data;
        }

    }
}
