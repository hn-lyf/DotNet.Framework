using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNet.Models
{
    /// <summary>
    /// 实体类。
    /// </summary>
    public class Model
    {
        /// <summary>
        /// 获取或设置一个值，该值表示此实体的唯一编号
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("编号"), Column(Order = 0)]
        public virtual long Id { get; set; } = DotNet.Snowflake.NewId();
        /// <summary>
        /// 获取一个值，该值表示数据的排序序号。
        /// </summary>
        [DisplayName("排序序号"), Column(Order = 100)]
        [Required, DefaultValue(0)]
        public virtual int OrderInt { get; set; } = 0;
        /// <summary>
        /// 获取或设置一个值，该值表示数据的状态
        ///<para>-1 表示为删除的旧数据</para>
        ///<para>0 表示为正常数据</para>
        /// </summary>
        [DisplayName("状态"), Column(Order = 101)]
        [Required, DefaultValue(0)]
        public virtual DataStatus StatusFlag { get; set; } = DataStatus.Normal;
        /// <summary>
        /// 获取或设置一个值，该值表示该表的数据的备注。
        /// </summary>
        [DisplayName("备注"), Column(TypeName = "Nvarchar(200)", Order = 102)]
        public virtual string Remarks { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值表示创建此数据的用户编号。
        /// </summary>
        [DisplayName("创建此数据的用户编号"), Column(Order = 103)]
        [Required]
        public virtual long CreateUserId { get; set; }
        /// <summary>
        /// 获取或设置一个值，该值表示创建此数据的用户名称。
        /// </summary>
        [DisplayName("创建此数据的用户名称"), Column(TypeName = "Nvarchar(50)", Order = 104)]
        [Required]
        public virtual string CreateUserName { get; set; } = "";
        /// <summary>
        /// 获取或设置一个值，该值表示创建此数据的时间。
        /// </summary>
        [DisplayName("创建此数据的时间"), Column(TypeName = "datetime", Order = 105)]
        [Required]
        public virtual DateTime CreateDate { get; set; } = DateTime.Now;
    }
    /// <summary>
    /// 数据状态数据状态类型
    /// </summary>
    public enum DataStatus
    {
        /// <summary>
        /// 表示为删除昨天。
        /// </summary>
        Delete = -1,
        /// <summary>
        /// 表示为正常状态。
        /// </summary>
        Normal = 0
    }
}
