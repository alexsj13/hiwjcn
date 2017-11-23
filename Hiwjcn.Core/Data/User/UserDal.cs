using Dapper;
using Hiwjcn.Dal.User;
using Lib.data;
using Lib.helper;
using Model.User;
using System;
using Lib.extension;
using Lib.data.ef;

namespace Dal.User
{
    /// <summary>
    /// 用户数据连接类
    /// </summary>
    public class UserDal : EFRepository<UserModel>, IUserDal
    {
        /// <summary>
        /// 修改数据库中的用户头像
        /// </summary>
        /// <param name="db"></param>
        /// <param name="uid"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int UpdateUserMask(string uid, byte[] b)
        {
            int count = 0;
            DBHelper.PrepareConnection(con =>
            {
                var sql = "delete from UserAvatar where UserUID=@uid";
                count = con.Execute(sql, new { img = b, uid = uid });
                var model = new UserAvatar()
                {
                    UID = Com.GetUUID(),
                    UserUID = uid.ToString(),
                    AvatarBytes = b,
                    CreateTime = DateTime.Now
                };
                count = con.Insert(model);
            });
            return count;
        }

        /// <summary>
        /// 从数据库中读取用户头像
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public byte[] ReadUserImage(string uid)
        {
            byte[] b = null;
            DBHelper.PrepareConnection(con =>
            {
                var sql = "select AvatarBytes from UserAvatar where UserUID=@uid limit 0,1";
                b = con.ExecuteScalar<byte[]>(sql, new { uid = uid });
            });
            return b;
        }

    }
}
