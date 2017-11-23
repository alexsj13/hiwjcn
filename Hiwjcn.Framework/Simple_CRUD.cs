using Lib.helper;
using Lib.infrastructure.entity;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Hiwjcn.Framework
{
    public class Simple_CRUD
    {
        public const string VIEW_DIR = "/Views/Simple_CRUD/";
        public string View(string name)
        {
            return VIEW_DIR + name;
        }
    }
    public class Simple_CRUD_List<T> : Simple_CRUD where T : BaseEntity
    {
        public string DeleteActionUrl { get; set; }
        public ActionResult View()
        {
            var t = Type.GetType("");
            return null;
        }
    }
    public class Simple_CRUD_Form<T> : Simple_CRUD where T : BaseEntity
    {
        public string SaveActionUrl { get; set; }
        public ActionResult View()
        {
            return null;
        }
    }
    public class ListViewDataContainer
    {
        public string Title { get; set; }

        public List<BaseEntity> Body { get; set; }

        public string Pager { get; set; }

        public string DeleteAction { get; set; }

        public string EditAction { get; set; }

        public void GetCols(ref PropertyInfo keyCol, ref List<PropertyInfo> displayCols)
        {
            if (!ValidateHelper.IsPlumpList(Body))
            {
                throw new Exception("list为空");
            }
            var props = Body[0].GetType().GetProperties().ToList();
            displayCols = props.Where(x => x.GetCustomAttributes<DisplayColumnAttribute>().Any()).ToList();
            var keyCols = props.Where(x => x.GetCustomAttributes<KeyAttribute>().Any()).ToList();
            if (keyCols.Count != 1) { throw new Exception("有且只有一个主键"); }
            keyCol = keyCols[0];
        }
    }
    public class DisplayColumnAttribute : Attribute
    { }
}
