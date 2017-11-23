using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lib.extension;

namespace Lib.data
{
    /// <summary>
    /// 标记是数据表（不要使用类，因为单继承，有时候无法标记）
    /// </summary>
    public interface IDBTable
    {
        //
    }

    /// <summary>
    /// 约定了数据表默认属性
    /// </summary>
    public abstract class DBTable : IDBTable
    {
        /// <summary>
        /// 自增的主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long IID { get; set; }

        /// <summary>
        /// UUID
        /// </summary>
        [StringLength(100, MinimumLength = 20, ErrorMessage = "UID必须是长度为20~100的唯一字符串")]
        public virtual string UID { get; set; }

        /// <summary>
        /// 转换为json
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToJson();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DBTable);
        }

        private static bool IsTransient(DBTable obj)
        {
            return obj != null && Equals(obj.IID, default(long));
        }

        private Type GetUnproxiedType()
        {
            return GetType();
        }

        public virtual bool Equals(DBTable other)
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

        public static bool operator ==(DBTable x, DBTable y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(DBTable x, DBTable y)
        {
            return !(x == y);
        }

    }

}
