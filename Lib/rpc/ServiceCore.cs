using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.extension;
using System.ServiceModel.Channels;
using System.Configuration;
using System.IO;
using System.Web;
using System.Reflection;
using Castle.DynamicProxy;
using System.ServiceModel.Description;

namespace Lib.rpc
{
    /// <summary>
    /// 操作返回值
    /// </summary>
    [Serializable]
    [DataContract]
    public class OperationResult<T>
    {
        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public bool Success { get; set; } = false;

        [DataMember]
        public Exception Ex { get; set; }

        [DataMember]
        public T Result { get; set; }

        /// <summary>
        /// 如果有异常就抛出
        /// </summary>
        public OperationResult<T> ThrowIfException()
        {
            if (this.Ex != null) { throw this.Ex; }
            if (ValidateHelper.IsPlumpString(this.ErrorMessage) || ValidateHelper.IsPlumpString(this.ErrorCode))
            {
                throw new Exception($"服务异常，msg：{this.ErrorMessage}，code：{this.ErrorCode}");
            }
            return this;
        }
    }

    public class ServiceContract_Attribute : Attribute
    {
        /// <summary>
        /// not started with '/'
        /// </summary>
        public virtual string RelativePath { get; set; }
    }
}
