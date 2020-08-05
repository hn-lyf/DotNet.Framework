using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Linq;

namespace DotNet.Data.Linq
{
    /// <summary>
    /// 数据库扩展类
    /// </summary>
    public static class DbHelperExtension
    {
        /// <summary>
        /// 根据编号查询信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="helper"></param>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="IdName"></param>
        /// <returns></returns>
        public static Result<T> QueryById<T>(this DbHelper helper, string tableName, object id, string IdName = "Id")
        {
            var result = helper.QuerySqlToDataReader($"SELECT * FROM {tableName} where {IdName}=@Id", new { Id = id });
            if (result.Success && result.Data.HasRows)
            {
                var model = result.Data.ToModel<T>();
                return model;
            }
            return new Result<T>(false) { Message = result.Success ? $"信息不存在" : result.Message };
        }
        /// <summary>
        /// 删除指定Id的记录。
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="IdName"></param>
        /// <returns></returns>
        public static Result DeleteById(this DbHelper helper, string tableName, object id, string IdName = "Id")
        {
            var result = helper.ExecuteSqlNonQuery($"delete FROM {tableName} where {IdName}=@Id", new { Id = id });
            if (result.Success && result.Code == 1)
            {
                return true;
            }
            return new Result(false) { Message = result.Success ? $"信息不存在" : result.Message };

        }
    }
}
