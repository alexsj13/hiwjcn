using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lib.extension
{
    /// <summary>
    /// 对Linq的扩展
    /// </summary>
    public static class LinqExtensions
    {
        public static T FirstOrThrow<T>(this IEnumerable<T> query, string error_msg) =>
            query.AsQueryable().FirstOrThrow(error_msg);

        public static T FirstOrThrow<T>(this IQueryable<T> query, string error_msg)
        {
            var model = query.FirstOrDefault();
            Com.AssertNotNull(model, error_msg);
            return model;
        }

        public static async Task<T> FirstOrThrowAsync<T>(this IQueryable<T> query, string error_msg)
        {
            var model = await query.FirstOrDefaultAsync();
            Com.AssertNotNull(model, error_msg);
            return model;
        }

        /// <summary>
        /// 返回非null list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<T> NotNullList<T>(this IQueryable<T> query)
        {
            return ConvertHelper.NotNullList(query.ToList());
        }

        /// <summary>
        /// 返回非null list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<List<T>> NotNullListAsync<T>(this IQueryable<T> query)
        {
            return ConvertHelper.NotNullList(await query.ToListAsync());
        }

        /// <summary>
        /// 如果条件不为空就使用条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereIfNotNull<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
        {
            if (where != null)
            {
                query = query.Where(where);
            }
            return query;
        }

        /// <summary>
        /// 自动分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static (List<T> list, int rowcount, int pagecount) ToPagedList<T>(this IOrderedQueryable<T> query, int page, int pagesize)
        {
            var count = query.QueryRowCountAndPageCount(pagesize);
            var list = query.QueryPage(page, pagesize).ToList();
            return (list, count.item_count, count.page_count);
        }

        /// <summary>
        /// 自动分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static async Task<(List<T> list, int rowcount, int pagecount)> ToPagedListAsync<T>(this IOrderedQueryable<T> query, int page, int pagesize)
        {
            var count = await query.QueryRowCountAndPageCountAsync(pagesize);
            var list = await query.QueryPage(page, pagesize).ToListAsync();
            return (list, count.item_count, count.page_count);
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static IQueryable<T> QueryPage<T>(this IOrderedQueryable<T> query, int page, int pagesize)
        {
            var pager = PagerHelper.GetQueryRange(page, pagesize);
            return query.Skip(pager.skip).Take(pager.take);
        }

        /// <summary>
        /// 获取记录总数和分页总数
        /// </summary>
        public static (int item_count, int page_count) QueryRowCountAndPageCount<T>(this IQueryable<T> query, int page_size)
        {
            var item_count = query.Count();
            var page_count = PagerHelper.GetPageCount(item_count, page_size);
            return (item_count, page_count);
        }

        /// <summary>
        /// 获取记录总数和分页总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public static async Task<(int item_count, int page_count)> QueryRowCountAndPageCountAsync<T>(this IQueryable<T> query, int page_size)
        {
            var item_count = await query.CountAsync();
            var page_count = PagerHelper.GetPageCount(item_count, page_size);
            return (item_count, page_count);
        }

        /// <summary>
        /// take别名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IQueryable<T> Limit<T>(this IQueryable<T> query, int count) => query.Take(count);

        /// <summary>
        /// 动态生成or条件
        /// http://www.albahari.com/nutshell/predicatebuilder.aspx
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        /// <summary>
        /// 动态生成and条件
        /// http://www.albahari.com/nutshell/predicatebuilder.aspx
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
