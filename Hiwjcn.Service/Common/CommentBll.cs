using Bll;
using Hiwjcn.Core.Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using Hiwjcn.Dal.Sys;
using Bll.User;
using Lib.helper;
using Hiwjcn.Core.Infrastructure.User;
using Lib.ioc;
using Lib.infrastructure;
using Lib.extension;

namespace Hiwjcn.Bll.Sys
{
    public class CommentBll : ServiceBase<CommentModel>
    {
        public CommentBll()
        {
            //
        }

        public CommentModel GetCommentByID(string id)
        {
            var _commentDal = new CommentDal();
            return _commentDal.GetFirst(x => x.UID == id);
        }

        /// <summary>
        /// 获取多个评论对象
        /// </summary>
        /// <param name="idlist"></param>
        /// <returns></returns>
        public List<CommentModel> GetCommentsByIDS(params string[] idlist)
        {
            if (!ValidateHelper.IsPlumpList(idlist)) { return null; }

            var _commentDal = new CommentDal();
            return _commentDal.GetList(x => idlist.Contains(x.UID));
        }

        /// <summary>
        /// 获取评论分页
        /// </summary>
        /// <param name="threadID"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public PagerData<CommentModel> GetComments(string threadID, int page, int pagesize)
        {
            if (!ValidateHelper.IsPlumpString(threadID)) { return null; }

            var data = new PagerData<CommentModel>();
            var _commentDal = new CommentDal();
            _commentDal.PrepareIQueryable((query) =>
            {
                query = query.Where(x => x.ThreadID == threadID);
                data.ItemCount = query.Count();
                data.DataList = query.OrderByDescending(x => x.CreateTime).QueryPage(page, pagesize).ToList();
                return true;
            });
            return data;
        }

        /// <summary>
        /// 添加评论
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddComment(CommentModel model)
        {
            string check = CheckModel(model);
            if (ValidateHelper.IsPlumpString(check))
            {
                return check;
            }
            var _commentDal = new CommentDal();
            var count = _commentDal.Add(model);
            return count > 0 ? SUCCESS : "评论失败";
        }

        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public string DeleteComment(string id, string userid)
        {
            var _commentDal = new CommentDal();
            var model = _commentDal.GetFirst(x => x.UID == id && x.UserID == userid);
            if (model == null) { return "评论不存在"; }
            return _commentDal.Delete(model) > 0 ? SUCCESS : "删除失败";
        }

        /// <summary>
        /// 获取评论数
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public int GetCommentCount(string userid, DateTime? start, DateTime? end)
        {
            var _commentDal = new CommentDal();
            var count = 0;
            _commentDal.PrepareIQueryable((query) =>
            {
                if (ValidateHelper.IsPlumpString(userid))
                {
                    query = query.Where(x => x.UserID == userid);
                }
                if (start != null)
                {
                    query = query.Where(x => x.UpdateTime >= start.Value);
                }
                if (end != null)
                {
                    query = query.Where(x => x.UpdateTime < end.Value);
                }
                count = query.Count();
                return true;
            });
            return count;
        }

    }
}
