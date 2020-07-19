using System.Reflection;
using System.Resources;

namespace DotNet
{
    /// <summary>
    /// 资源读取文件
    /// </summary>
    public class SR
    {
        private readonly Assembly myAssembly;
        private readonly string AssemblyName;
        private string resourceName;
        /// <summary>
        /// 使用指定的程序集初始化<see cref="SR"/>新的实例。
        /// </summary>
        /// <param name="assembly">要获取资源的程序集</param>
        /// <param name="resourceName">默认资源名称</param>
        public SR(Assembly assembly, string resourceName = null)
        {
            myAssembly = assembly;
            AssemblyName = $"{assembly.GetName().Name}.";
            if (resourceName == null)
            {
                foreach (string keyName in Keys)
                {
                    if (keyName.EndsWith(".resources"))
                    {
                        resourceName = keyName.Remove(keyName.Length - 10); ;
                        break;
                    }
                }
            }

            this.resourceName = resourceName;
        }
        /// <summary>
        /// 获取当前正在执行的方法的资源读取类。
        /// </summary>
        public static SR Current
        {
            get
            {
                return new SR(Assembly.GetCallingAssembly());
            }
        }
        /// <summary>
        /// 返回指定字符串资源的值。
        /// </summary>
        /// <param name="name">要检索的资源的名称。</param>
        /// <returns></returns>
        public string GetString(string name)
        {
            return name;
            return Resource.GetString(name);
        }
        /// <summary>
        /// 返回指定字符串资源的值。
        /// </summary>
        /// <param name="name">要检索的资源的名称。</param>
        /// <param name="cultureName">要获取资源字符串的语言名称，如zh-cn等</param>
        /// <returns></returns>
        public string GetString(string name, string cultureName)
        {
            return Resource.GetString(name, new System.Globalization.CultureInfo(cultureName));
        }
        /// <summary>
        /// 获取或设置一个值，该值指示要获取资源的程序集。
        /// </summary>
        public Assembly CurrentAssembly { get { return myAssembly; } }
        /// <summary>
        /// 获取此程序集中的所有资源的名称。
        /// </summary>
        public string[] Keys
        {
            get
            {
                return CurrentAssembly.GetManifestResourceNames();
            }
        }
        /// <summary>
        /// 资源名称。
        /// </summary>
        public string ResourceName { get => resourceName; set => resourceName = value; }

        /// <summary>
        /// 根据指定的资源名称获取资源文件流。
        /// </summary>
        /// <param name="fullName">资源文件名称（包含程序集名称和文件夹名称）</param>
        /// <returns></returns>
        public System.IO.Stream GetStream(string fullName)
        {
            return CurrentAssembly.GetManifestResourceStream(fullName);
        }
        private ResourceManager resource;
        /// <summary>
        /// 默认资源
        /// </summary>
        public ResourceManager Resource
        {
            get
            {
                if (resource == null)
                {
                    resource = new ResourceManager(ResourceName, myAssembly);
                }
                return resource;
            }
        }
        /// <summary>
        /// 获取资源文件管理类。
        /// </summary>
        /// <param name="name">资源的根名称。例如，名为“DotNet.MyResource.resources”的资源文件的根名称为“MyResource”或DotNet.MyResource。</param>
        /// <returns></returns>
        public ResourceManager this[string name]
        {
            get
            {
                if (!name.StartsWith(AssemblyName))
                {
                    name = $"{AssemblyName}{name}";
                }
                return new ResourceManager(name, myAssembly);
            }
        }
    }
}
