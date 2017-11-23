using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dal;
using WebLogic.Model.Page;
using Lib.data;
using Lib.data.ef;

namespace WebLogic.Dal.Page
{
    public class SectionDal : EFRepository<SectionModel>
    {
        public SectionDal() { }
    }
}
