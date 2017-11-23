using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Lib.data
{
    /// <summary>
    /// redis基类，实现序列化和反序列化
    /// </summary>
    public abstract class SerializeBase
    {
        private Encoding _encoding { get => Encoding.UTF8; }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual byte[] Serialize(object item)
        {
            if (item != null)
            {
                var jsonString = item.ToJson();
                return this._encoding.GetBytes(jsonString);
            }
            return new byte[] { };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        public virtual T Deserialize<T>(byte[] serializedObject)
        {
            if (serializedObject?.Length > 0)
            {
                var jsonString = this._encoding.GetString(serializedObject);
                return jsonString.JsonToEntity<T>();
            }
            return default(T);
        }

        #region binary formatter
        /// <summary>
        /// 获取对象序列化的二进制版本
        /// </summary>
        /// <param name="obj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public virtual byte[] BinaryFormatterSerialize(object obj)
        {
            if (obj == null) { return null; }

            using (var ms = new MemoryStream())
            {
                var formartter = new BinaryFormatter();
                formartter.Serialize(ms, obj);

                ms.Position = 0;
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 从已序列化数据中(byte[])获取对象实体
        /// </summary>
        public virtual T BinaryFormatterDeserialize<T>(byte[] bs)
        {
            if (bs == null) { return default(T); }
            using (var ms = new MemoryStream(bs))
            {
                var formatter = new BinaryFormatter();

                return (T)formatter.Deserialize(ms);
            }
        }
        #endregion
    }

    /// <summary>
    /// 只是继承SerializeBase，没有更多实现
    /// </summary>
    public class SerializeHelper : SerializeBase
    {
        public static readonly SerializeHelper Instance = new SerializeHelper();
    }
}
