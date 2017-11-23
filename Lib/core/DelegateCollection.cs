using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    public delegate TResult RefFunc<T, TResult>(ref T arg);
    public delegate void RefAction<T>(ref T arg);

    public delegate void VoidFunc();
    public delegate void VoidFunc<T>(T arg);
    public delegate void VoidFunc<T1, T2>(T1 arg1, T2 arg2);
    public delegate void VoidFunc<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void VoidFunc<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void VoidFunc<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}
