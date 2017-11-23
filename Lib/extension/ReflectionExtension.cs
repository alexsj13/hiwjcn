using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Lib.data;

namespace Lib.extension
{
    /// <summary>
    /// BaseType是类
    /// GetInterfaces是接口
    /// IsGenericType是泛型
    /// GetGenericTypeDefinition()获取泛型类型比如Consumer《string》
    /// </summary>
    public static class ReflectionExtension
    {
        public static bool IsAsync(this MethodInfo method) =>
            method.ReturnType == typeof(Task) || method.ReturnType.IsGenericType_(typeof(Task<>));

        public static List<Type> FindTypesByBaseType<T>(this Assembly a)
        {
            return a.GetTypes().Where(x => x.BaseType != null && x.BaseType == typeof(T)).ToList();
        }

        public static bool IsDatabaseTable(this Type t) => t.IsAssignableTo_<IDBTable>();

        [Obsolete]
        public static bool IsAssignableToGeneric_xxxxxxxxxxxxxx(this Type t, Type generic_type)
        {
            if (!generic_type.IsGenericType) { throw new Exception("必须是泛型"); }
            return t.IsGenericType_(generic_type) || t.GetAllInterfaces_().Any(x => x.IsGenericType_(generic_type));
        }

        public static bool IsAssignableToGeneric_(this Type t, Type generic_type)
        {
            if (!generic_type.IsGenericType) { throw new Exception("必须是泛型"); }
            if (t.GetAllInterfaces_().Any(x => x.IsGenericType_(generic_type)))
            {
                return true;
            }

            var cur = t;
            while (true)
            {
                if (t == null || t == typeof(object)) { break; }
                if (t.IsGenericType_(generic_type))
                {
                    return true;
                }
                cur = t.BaseType;
            }

            return false;
        }

        /// <summary>
        /// 是否可以赋值给
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsAssignableTo_<T>(this Type t)
        {
            return t.IsAssignableTo_(typeof(T));
        }

        /// <summary>
        /// 是否可以赋值给
        /// </summary>
        /// <param name="t"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAssignableTo_(this Type t, Type type)
        {
            return type.IsAssignableFrom(t);
        }

        /// <summary>
        /// 非抽象类，不是抽象类，不是接口
        /// </summary>
        public static bool IsNormalClass(this Type t)
        {
            return t.IsClass && !t.IsAbstract && !t.IsInterface;
        }

        public static Type[] GetAllNormalClass(this Assembly ass) =>
            ass.GetTypes().Where(x => x.IsNormalClass()).ToArray();

        /// <summary>
        /// 是指定的泛型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsGenericType_<T>(this Type t)
        {
            return t.IsGenericType_(typeof(T));
        }

        /// <summary>
        /// 是指定的泛型
        /// </summary>
        /// <param name="t"></param>
        /// <param name="tt"></param>
        /// <returns></returns>
        public static bool IsGenericType_(this Type t, Type tt)
        {
            if (!tt.IsGenericType) { throw new Exception("传入参数必须是泛型"); }
            return t.IsGenericType && t.GetGenericTypeDefinition() == tt;
        }

        /// <summary>
        /// 获取所有实现接口，包括继承的
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<Type> GetAllInterfaces_(this Type t) => t.GetInterfaces().ToList();

        /// <summary>
        /// 获取可以赋值给T的属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomAttributes_<T>(this MemberInfo prop) where T : Attribute
        {
            var attrs = prop.GetCustomAttributes();
            return attrs.Where(x => x.GetType().IsAssignableTo_<T>()).Select(x => (T)x);
        }
    }
}
