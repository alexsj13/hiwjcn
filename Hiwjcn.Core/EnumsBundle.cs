using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core
{
    public enum OrderStatusEnum : int
    {
        待付款 = 1,
        待发货 = 2,
        待收货 = 3,
        待评价 = 4,
        关闭 = 8,
        取消 = 9
    }
}
