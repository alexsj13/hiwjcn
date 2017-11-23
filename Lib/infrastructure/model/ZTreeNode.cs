using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.extension;
using Newtonsoft.Json;

namespace Lib.infrastructure.model
{
    /// <summary>
    /// { id:2, pId:0, name:"随意勾选 2", checked:true, open:true},
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class ZTreeNode
    {
        [JsonProperty(Required = Required.Always)]
        public virtual string id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string pId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public virtual string name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string icon { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string url { get; set; }

        [JsonProperty(PropertyName = nameof(@checked))]
        public virtual bool @checked { get; set; } = false;

        public virtual bool open { get; set; } = false;

        public virtual bool chkDisabled { get; set; } = true;


        public static implicit operator ZTreeNode(RoleEntityBase role) =>
            new ZTreeNode()
            {
                id = role.UID,
                pId = role.ParentUID,
                name = role.RoleName,
            };

        public static implicit operator ZTreeNode(DepartmentEntityBase department) =>
            new ZTreeNode()
            {
                id = department.UID,
                pId = department.ParentUID,
                name = department.DepartmentName
            };

        public static implicit operator ZTreeNode(MenuEntityBase menu) =>
            new ZTreeNode()
            {
                id = menu.UID,
                pId = menu.ParentUID,
                name = menu.MenuName
            };
    }
}
