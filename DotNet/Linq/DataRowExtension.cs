using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace DotNet.Linq
{
    /// <summary>
    /// <see cref="System.Data.DataRow"/>类的扩展。
    /// </summary>
    public static class DataRowExtension
    {
        /// <summary>
        /// 转换成实体类。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="model">要绑定的实体。</param>
        /// <param name="throwOnError">true 将引发所发生的任何异常。 - 或 - false 将忽略所发生的任何异常。</param>
        /// <returns></returns>
        public static Result<T> ToModel<T>(this System.Data.DataRow row, T model, bool throwOnError = false)
        {
            Result<T> result = new Result<T>
            {
                Data = model,
                Message = "操作成功",
                Success = true
            };
            var type = model.GetType();

            foreach (DataColumn column in row.Table.Columns)
            {
                try
                {
                    var property = type.GetProperty(column.ColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        var value = row[column];//[name];
                        if (!(value is DBNull))
                        {
                            property.SetValue(model, value.ChangeType(property.PropertyType), null);
                        }
                    }
                }
                catch (Exception e)
                {
                    result.Message = e.Message;
                    result.Success = false;
                    if (throwOnError)
                    {
                        throw e;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 转换成实体类。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="throwOnError">true 将引发所发生的任何异常。 - 或 - false 将忽略所发生的任何异常。</param>
        /// <returns></returns>
        public static Result<List<T>> ToModels<T>(this DataRowCollection rows, bool throwOnError = false)
        {
            Result<List<T>> result = new Result<List<T>>
            {
                Data = new List<T>()
            };
            foreach (DataRow row in rows)
            {
                T model = Activator.CreateInstance<T>();
                if (row.ToModel(model, throwOnError).Success)
                {
                    result.Data.Add(model);
                }
            }
            result.Success = true;
            return result;

        }
        /// <summary>
        /// 将<see cref="DataRow"/>集合转换成<typeparamref name="T"/>的yield的迭代器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToList<T>(this DataRowCollection rows)
        {
            foreach (DataRow row in rows)
            {
                T model = Activator.CreateInstance<T>();
                row.ToModel(model, false);
                yield return model;
            }
        }
        /// <summary>
        /// 将<see cref="DataTable"/>表格转换成<typeparamref name="T"/>的yield的迭代器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToList<T>(this DataTable dataTable)
        {
            return dataTable.Rows.ToList<T>();
        }
        /// <summary>
        /// 将<see cref="DataSet"/>中指定索引的<see cref="DataTable"/>转换成<typeparamref name="T"/>的yield的迭代器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSet"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToList<T>(this DataSet dataSet, int index = 0)
        {
            return dataSet.Tables[0].Rows.ToList<T>();
        }
       
    }
}
