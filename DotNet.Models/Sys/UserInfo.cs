using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DotNet.Models.Sys
{
    /// <summary>
    /// 用户信息表。
    /// </summary>
    [Table("S1_UserInfo")]
    [DisplayName("用户信息表")]
    public class UserInfo : Model
    {
        /// <summary>
        /// 获取或设置一个值，该值表示用户所属的部门编号。
        /// </summary>
        [DisplayName("部门编号"), Column(Order = 1)]
        [Required]
        public virtual long DepartmentId { get; set; } = 0;

        /// <summary>
        /// 获取或设置一个值，该值表示用户名称。
        /// </summary>
        [DisplayName("用户名称"), Column(TypeName = "Nvarchar(20)", Order = 2)]
        public virtual string UserName { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值表示用户登录名称。
        /// </summary>
        [DisplayName("用户登录名称"), Column(TypeName = "varchar(20)", Order = 3)]
        public virtual string LoginName { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值表示用户登录密码。
        /// </summary>
        [DisplayName("用户登录密码"), Column(TypeName = "varchar(50)", Order = 4)]
        public virtual string LoginPassword { get; set; }
        ///// <summary>
        ///// 获取或设置一个值，该值表示用户手机号码。
        ///// </summary>
        [DisplayName("用户手机号码"), Column(TypeName = "varchar(50)", Order = 4)]
        public virtual string MobilePhone { get; set; }

    }
}
