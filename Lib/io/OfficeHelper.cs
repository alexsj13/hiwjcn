using System;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Lib.core;
using NPOI.XWPF.UserModel;
using Lib.helper;
using Lib.extension;

namespace Lib.io
{
    /// <summary>
    ///Excel 的摘要说明
    /// </summary>
    public static class ExcelHelper
    {
        public static readonly string ContentType = "application/ms-excel";

        public static ICellStyle GetStyle(XSSFWorkbook workbook,
            short background,
            short color,
            bool BoldFont = false)
        {
            var style = workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            style.FillPattern = FillPattern.SolidForeground;
            style.BorderTop = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.TopBorderColor = NPOI.HSSF.Util.HSSFColor.Black.Index;
            style.LeftBorderColor = NPOI.HSSF.Util.HSSFColor.Black.Index;
            style.RightBorderColor = NPOI.HSSF.Util.HSSFColor.Black.Index;
            style.BottomBorderColor = NPOI.HSSF.Util.HSSFColor.Black.Index;
            style.FillForegroundColor = background;

            var font = workbook.CreateFont();
            font.FontHeightInPoints = 14;
            if (BoldFont)
            {
                font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            }
            font.Color = color;
            style.SetFont(font);
            return style;
        }

        /// <summary>
        /// return File(bs, "application/vnd.ms-excel");
        /// </summary>
        public static byte[] ObjectToExcel<T>(List<T> list, string sheet_name = "sheet")
        {
            list = list ?? throw new Exception("参数为空");
            var t = typeof(T);

            var table = new DataTable();
            table.TableName = sheet_name ??
                t.GetCustomAttributes_<ExcelInfoAttribute>().FirstOrDefault()?.SheetName ??
                throw new Exception($"{nameof(sheet_name)}不能为空");

            IEnumerable<(PropertyInfo p, ExcelInfoAttribute attr)> props = t.GetProperties()
                .Select(x => (x, x.GetCustomAttributes_<ExcelInfoAttribute>().FirstOrDefault()));
            //根据索引排序
            props = props.OrderBy(x => x.attr?.Index ?? 0);

            foreach (var p in props)
            {
                //优先取标签name
                var name = p.attr?.HeaderName ?? p.p.Name;
                table.Columns.Add(name, typeof(string));
            }
            foreach (var m in list)
            {
                var data = props.Select(x => ConvertHelper.GetString(x.p.GetValue(m))).ToArray();
                table.Rows.Add(data);
            }

            return DataTableToExcel(table);
        }

        /// <summary>
        /// return File(bs, "application/vnd.ms-excel");
        /// </summary>
        public static byte[] DataTableToExcel(DataTable tb, bool show_header = true)
        {
            tb = tb ?? throw new Exception($"无法把空{nameof(DataTable)}转成Excel");

            using (var ms = new MemoryStream())
            {
                var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();
                var sheet = workbook.CreateSheet(ValidateHelper.IsPlumpString(tb.TableName) ? tb.TableName : "sheet");

                var columns = tb.Columns.AsEnumerable_<DataColumn>();

                var row_index = 0;
                IRow NewRow() => sheet.CreateRow(row_index++);
                if (show_header)
                {
                    //头部
                    var header_style = GetStyle(workbook,
                        NPOI.HSSF.Util.HSSFColor.Black.Index, NPOI.HSSF.Util.HSSFColor.White.Index);
                    var header = NewRow();
                    var cell_index = 0;
                    foreach (var col in columns)
                    {
                        var cell = header.CreateCell(cell_index++, CellType.String);
                        var data = col.ColumnName;
                        cell.SetCellValue(data);
                        cell.CellStyle = header_style;
                    }
                }

                var style = GetStyle(workbook,
                    NPOI.HSSF.Util.HSSFColor.White.Index, NPOI.HSSF.Util.HSSFColor.Black.Index);

                foreach (var tb_row in tb.Rows.AsEnumerable_<DataRow>())
                {
                    var row = NewRow();
                    var cell_index = 0;
                    foreach (var tb_col in columns)
                    {
                        var cell = row.CreateCell(cell_index++, CellType.String);
                        var data = tb_row[tb_col.ColumnName];
                        cell.SetCellValue(ConvertHelper.GetString(data));
                        cell.CellStyle = style;
                    }
                }

                workbook.Write(ms);
                workbook.Clear();
                tb.Clear();

                var bs = ms.ToArray();
                return bs;
            }
        }

        public static string GetCellValue(NPOI.SS.UserModel.ICell cell)
        {
            try
            {
                if (cell == null) { return string.Empty; }
                string value = string.Empty;
                switch (cell.CellType)
                {
                    case CellType.Blank: { value = string.Empty; } break;
                    case CellType.Boolean: { value = ConvertHelper.GetString(cell.BooleanCellValue); } break;
                    case CellType.Error: { value = ConvertHelper.GetString(cell.ErrorCellValue); } break;
                    case CellType.Formula: { value = ConvertHelper.GetString(cell); } break;
                    case CellType.Numeric: { value = ConvertHelper.GetString(cell.NumericCellValue); } break;
                    case CellType.String: { value = ConvertHelper.GetString(cell.StringCellValue); } break;
                    case CellType.Unknown:
                    default: { value = ConvertHelper.GetString(cell); } break;
                }
                return value;
            }
            catch (Exception e)
            {
                return $"获取cell数据异常：{e.Message}";
            }
        }

        private static List<List<string>> SheetToList(ISheet sheet)
        {
            sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));

            var list = new List<List<string>>();
            for (int i = 0; i < sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                if (row == null) { continue; }
                var rowlist = new List<string>();
                for (int j = 0; j < row.LastCellNum; ++j)
                {
                    rowlist.Add(GetCellValue(row.GetCell(j)));
                }
                list.Add(rowlist);
            }
            return list;
        }

        public static List<List<List<string>>> ExcelToList(string path)
        {
            var list = new List<List<List<string>>>();
            if (!IOHelper.FileHelper.Exists(path)) { throw new ArgumentNullException(nameof(path)); }
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var workbook = new XSSFWorkbook(stream);
                for (var i = 0; i < workbook.NumberOfSheets; ++i)
                {
                    var sheet = workbook.GetSheetAt(i);
                    list.Add(SheetToList(sheet));
                }
            }
            return list;
        }

        public static List<List<string>> ExcelToDataTable(string path)
        {
            throw new NotImplementedException();
        }
    }

    public enum ExcelColorEnum : short
    {
        Black = NPOI.HSSF.Util.HSSFColor.Black.Index,
        White = NPOI.HSSF.Util.HSSFColor.White.Index,
        Red = NPOI.HSSF.Util.HSSFColor.Red.Index,
        Green = NPOI.HSSF.Util.HSSFColor.Green.Index,
        Yellow = NPOI.HSSF.Util.HSSFColor.Yellow.Index,
        SkyBlue = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index,
        Orange = NPOI.HSSF.Util.HSSFColor.Orange.Index,
        Grey80Percent = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index,
        Violet = NPOI.HSSF.Util.HSSFColor.Violet.Index,
    }

    public class ExcelInfoAttribute : Attribute
    {
        public virtual string SheetName { get; set; }
        public virtual string HeaderName { get; set; }
        public virtual int Index { get; set; }
        public virtual ExcelColorEnum? Color { get; set; }
        public virtual ExcelColorEnum? BackgroundColor { get; set; }
    }

    public static class WordHelper
    {
        public static byte[] CreateWord(Dictionary<string, string> paragraphs)
        {
            var doc = new XWPFDocument();
            try
            {            //创建段落对象
                paragraphs = paragraphs ?? new Dictionary<string, string>() { };
                paragraphs.Keys.ToList().ForEach(key =>
                {
                    XWPFParagraph paragraph = doc.CreateParagraph();
                    XWPFRun run = paragraph.CreateRun();
                    run.IsBold = true;
                    run.SetText(ConvertHelper.GetString(key));
                    run.SetText(ConvertHelper.GetString(paragraphs[key]));
                });
                using (var stream = new MemoryStream())
                {
                    doc.Write(stream);
                    var bs = stream.ToArray();
                    return bs;
                }
            }
            finally
            {
                doc.Close();
            }
        }
    }

    public static class OfficeExtension
    {
        public static short ToNpoiColorIndex(this ExcelColorEnum color)
        {
            return (short)color;
        }
    }
}
