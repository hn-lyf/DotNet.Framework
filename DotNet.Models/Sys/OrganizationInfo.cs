using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DotNet.Models.Sys
{
    /// <summary>
    /// 机构信息表
    /// </summary>
    [Table("S1_OrganizationInfo")]
    [DisplayName("机构信息表")]
    public class OrganizationInfo : Model
    {
        /// <summary>
        /// 获取或设置一个值，该值表示上级编号(0为顶级)
        /// </summary>
        [DisplayName("上级编码Id"), Column(Order = 1)]
        [Required, DefaultValue(0)]
        public virtual long ParentId { get; set; } = 0;

        /// <summary>
        /// 获取或设置一个值，该值表示部门编码
        /// <para>此值设置标准</para>
        /// 编码必须包含父级的编号。
        /// </summary>
        [DisplayName("机构编号"), Column(TypeName = "varchar(200)", Order = 2)]
        public virtual string OrganizationCode { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值表示部门名称。
        /// </summary>
        [DisplayName("机构名称"), Column(TypeName = "Nvarchar(20)", Order = 2)]
        public virtual string OrganizationName { get; set; }

    }
}
