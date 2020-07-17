using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;

namespace DotNet.Linq
{
    /// <summary>
    /// <see cref="IDataReader"/>扩展类。
    /// </summary>
    public static class DataReaderExtension
    {

        /// <summary>
        /// 将<see cref="IDataReader"/>转换成<see cref="List{T}"/>对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this IDataReader dataReader)
        {
            using (dataReader)
            {
                List<T> list = new List<T>();
                var modelType = typeof(T);
                while (dataReader.Read())
                {
                    list.Add(dataReader.ToModel<T>(modelType));
                }
                return list;
            }

        }

        /// <summary>
        /// 将<see cref="IDataReader"/>转换成<typeparamref name="T"/>对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="type">如果为null则为<typeparamref name="T"/>类型</param>
        /// <returns></returns>
        public static T ToModel<T>(this IDataReader dataReader, Type type = null)
        {
            T model = Activator.CreateInstance<T>();

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (dataReader[i].IsNull())
                {
                    continue;
                }
                PropertyInfo property = (type ?? typeof(T)).GetProperty(dataReader.GetName(i), BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                property?.SetValue(model, dataReader[i].ChangeType(property.PropertyType), null);
            }
            return model;
        }
    }
}
