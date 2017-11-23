using Dal.Sys;
using Lib.helper;
using Lib.infrastructure;
using Model.Sys;
using System;
using System.Collections.Generic;

namespace Bll.Sys
{
    /// <summary>
    /// 站内信
    /// </summary>
    public class MessageBll : ServiceBase<MessageModel>
    {
        public MessageBll()
        {
            //
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SendMessage(MessageModel model)
        {
            string err = CheckModel(model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            var dal = new MessageDal();
            return dal.Add(model) > 0 ? SUCCESS : "发送失败";
        }

        /// <summary>
        /// 获取最新的站内信
        /// </summary>
        /// <param name="receiverID"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<MessageModel> GetTopMessage(string receiverID, int count = 100)
        {
            var mdal = new MessageDal();
            var list = mdal.QueryList(where: x => x.ReceiverUserID == receiverID,
                orderby: x => x.UpdateTime, Desc: true, start: 0, count: count);
            return list;
        }

        /// <summary>
        /// 获取站内信数量
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public int GetSenderMessageCount(string user_id, DateTime start, DateTime end)
        {
            var pdal = new MessageDal();
            return pdal.GetCount(x => x.SenderUserID == user_id
                && x.UpdateTime >= start && x.UpdateTime < end);
        }

    }
}
