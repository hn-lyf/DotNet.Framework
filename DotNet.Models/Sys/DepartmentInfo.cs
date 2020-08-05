using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DotNet.Models.Sys
{
    /// <summary>
    /// 部门信息表
    /// </summary>
    [Table("S1_DepartmentInfo")]
    [DisplayName("部门信息表")]
    public class DepartmentInfo : Model
    {

        /// <summary>
        /// 获取或设置一个值，该值表示上级编号(0为顶级)
        /// </summary>
        [DisplayName("上级Id"), Column(Order = 1)]
        [Required, DefaultValue(0)]
        public virtual long ParentId { get; set; } = 0;
        /// <summary>
        /// 获取或设置一个值，该值表示所属机构编号
        /// </summary>
        [DisplayName("机构编号"), Column(Order = 1)]
        [Required]
        public virtual long OrganizationId { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值表示部门编码
        /// <para>此值设置标准</para>
        /// 编码必须包含父级的编号。
        /// </summary>
        [DisplayName("部门编码"), Column(TypeName = "varchar(200)", Order = 2)]
        public virtual string DepartmentCode { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值表示部门名称。
        /// </summary>
        [DisplayName("部门名称"), Column(TypeName = "Nvarchar(20)", Order = 2)]
        public virtual string DepartmentName { get; set; }


    }
}
