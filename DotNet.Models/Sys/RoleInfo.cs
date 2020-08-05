using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DotNet.Models.Sys
{
    /// <summary>
    /// 角色信息表。
    /// </summary>
    [Table("S1_RoleInfo")]
    [DisplayName("角色信息表")]
    public class RoleInfo
    {
        /// <summary>
        /// 获取或设置一个值，该值表示角色编码。
        /// </summary>
        [DisplayName("角色编码"), Column(TypeName = "varchar(20)", Order = 1)]
        public virtual string RoleCode { get; set; }
        /// <summary>
        /// 获取或设置一个值，该值表示角色名称。
        /// </summary>
        [DisplayName("角色名称"), Column(TypeName = "Nvarchar(20)", Order = 2)]
        public virtual string RoleName { get; set; }
    }
}
