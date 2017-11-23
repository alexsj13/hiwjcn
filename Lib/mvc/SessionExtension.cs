using Lib.helper;
using System;
using System.Web.SessionState;
using Lib.extension;

namespace Lib.mvc
{
    public static class SessionExtension
    {
        /// <summary>
        /// 设置实体
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetObjectAsJson(this HttpSessionState session, string key, object value)
        {
            session[key] = (value ?? throw new Exception("null不能转为json，并存入session")).ToJson();
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        public static T GetObjectFromJsonOrDefault<T>(this HttpSessionState session, string key)
        {
            try
            {
                var value = session[key]?.ToString();
                if (ValidateHelper.IsPlumpString(value))
                {
                    return value.JsonToEntity<T>();
                }
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        public static T PopSessionFromJsonOrDefault<T>(this HttpSessionState session, string key)
        {
            var value = session.GetObjectFromJsonOrDefault<T>(key);
            session.RemoveSession(key);
            return value;
        }

        /// <summary>
        /// 取出后删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T PopSession<T>(this HttpSessionState session, string key)
        {
            var value = session.GetSession<T>(key);
            session.RemoveSession(key);
            return value;
        }

        /// <summary>
        /// 获取session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetSession<T>(this HttpSessionState session, string key)
        {
            var obj = session[key];

            if (obj == null) { return default(T); }

            //判断1
            if (obj is T res)
            {
                return res;
            }
            //判断2
            if (ValidateHelper.Is<T>(obj))
            {
                return (T)obj;
            }
            //使用json转换
            try
            {
                return obj.ToJson().JsonToEntity<T>();
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 保存到session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetSession(this HttpSessionState session, string key, object value)
        {
            session[key] = value;
        }

        /// <summary>
        /// 删除session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="keys"></param>
        public static void RemoveSession(this HttpSessionState session, params string[] keys)
        {
            foreach (var key in keys)
            {
                session.Remove(key);
            }
        }
    }
}
