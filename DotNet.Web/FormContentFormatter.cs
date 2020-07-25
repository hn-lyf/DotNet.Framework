using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using DotNet.Linq;

namespace DotNet.Web
{
    /// <summary>
    /// 处理FormContentType
    /// </summary>
    public class FormContentFormatter : IInputFormatter
    {
        /// <summary>
        /// 是否处理
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool CanRead(InputFormatterContext context)
        {
            return context.HttpContext.Request.HasFormContentType;
        }
        /// <summary>
        /// 开始读取内容。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var model = Activator.CreateInstance(context.ModelType);
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(model))
            {
                var formNameValue = context.HttpContext.Request.Form[descriptor.Name].ToString();
                if (!string.IsNullOrEmpty(formNameValue))
                {
                    if (descriptor.PropertyType.IsEnum)
                    {
                        descriptor.SetValue(model, Enum.Parse(descriptor.PropertyType, formNameValue));
                        continue;

                    }
                    descriptor.SetValue(model, Convert.ChangeType(formNameValue, descriptor.PropertyType.GetValueType()));
                }
            }
            return InputFormatterResult.SuccessAsync(model);
        }
    }
}
