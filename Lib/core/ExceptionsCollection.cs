using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    [Serializable]
    public class MsgException : Exception
    {
        public virtual string ErrorCode { get; set; }

        public MsgException(string msg) : base(msg)
        { }
    }

    [Serializable]
    public class SourceNotExistException : Exception
    { }

    [Serializable]
    public class AccessDenyException : Exception
    { }

    [Serializable]
    public class DeleteNodeException : Exception
    {
        public DeleteNodeException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NodeNotExistException : Exception
    { }

    [Serializable]
    public class NodeTooMuchException : Exception
    { }
}
