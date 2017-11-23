using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public abstract class VersionBaseEntity : BaseEntity
    {
        /// <summary>
        /// 记录的版本
        /// 用一个uuid来标识此次数据更新，以后ES就可以用这个字段判断数据是否是最新的
        /// </summary>
        public virtual string UpdateFlag { get; set; }

        /// <summary>
        /// 版本，每次更新+1，不在EF中使用
        /// </summary>
        public virtual int VersionID { get; set; }

        public override void Update()
        {
            base.Update();
            this.UpdateFlag = Com.GetUUID();
            ++this.VersionID;
        }
    }

    /// <summary>
    /// 实体基类
    /// </summary>
    [Serializable]
    public abstract class BaseEntity : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(nameof(IID))]
        public virtual long IID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "UID必填")]
        [Required]
        [Index(IsUnique = true), Column(nameof(UID))]
        public virtual string UID { get; set; }

        [Index(IsUnique = false)]
        public virtual int IsRemove { get; set; }

        [Index(IsUnique = false)]
        public virtual DateTime CreateTime { get; set; }

        [Index(IsUnique = false)]
        public virtual DateTime UpdateTime { get; set; }

        /// <summary>
        /// 第一次写库初始化
        /// </summary>
        public virtual void Init(string flag = null)
        {
            flag = ConvertHelper.GetString(flag);
            if (flag.ToArray().Any(x => x == ' '))
            {
                throw new Exception("init.flag不可以出现空格");
            }
            var prefix = string.Empty;
            if (ValidateHelper.IsPlumpString(flag))
            {
                prefix = $"{flag}-";
            }

            var now = DateTime.Now;

            this.IID = default(long);
            this.UID = prefix + Com.GetUUID();
            this.IsRemove = (int)YesOrNoEnum.否;
            this.CreateTime = now;

            this.Update();
        }

        /// <summary>
        /// 更新时间等信息
        /// </summary>
        public virtual void Update()
        {
            var now = DateTime.Now;
            this.UpdateTime = now;
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToJson();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }

        private static bool IsTransient(BaseEntity obj)
        {
            return obj != null && Equals(obj.IID, default(int));
        }

        private Type GetUnproxiedType()
        {
            return GetType();
        }

        public virtual bool Equals(BaseEntity other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(IID, other.IID))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                        otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (Equals(IID, default(int)))
                return base.GetHashCode();
            return IID.GetHashCode();
        }

        public static bool operator ==(BaseEntity x, BaseEntity y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }
    }
}
