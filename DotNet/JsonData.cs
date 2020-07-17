using DotNet.Linq;
using System.Collections.Generic;

namespace DotNet
{
    /// <summary>
    /// json的动态类。
    /// </summary>
    class JsonData : System.Dynamic.DynamicObject, System.Collections.IEnumerable
    {
        private readonly Dictionary<string, object> data;
        private readonly System.Collections.ArrayList list;
        private readonly string m_JsonStr;
        private JsonData(Dictionary<string, object> json)
        {
            data = json;
            m_JsonStr = json.ToJson();
        }//TryUnaryOperation
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            if (list == null)
            {
                foreach (string key in data.Keys)
                {
                    yield return key;
                }
            }
            else
            {
                foreach (string key in new string[] { "Count" })
                {
                    yield return key;
                }
            }
        }
        private JsonData(System.Collections.ArrayList json)
        {
            list = json;
            m_JsonStr = json.ToJson();
        }
        public override string ToString()
        {
            return m_JsonStr.ToString();
        }
        public JsonData(string json)
        {
            if (json.StartsWith("["))
            {
                list = json.JsonToObject<System.Collections.ArrayList>();
            }
            else
            {
                data = json.JsonToObject<Dictionary<string, object>>();
            }
            m_JsonStr = json;
        }
        public override bool TryGetIndex(System.Dynamic.GetIndexBinder binder, object[] indexes, out object result)
        {
            if (list != null && indexes[0] is int i)
            {
                object v = list[i];
                result = v;
                if (result is Dictionary<string, object>)
                {
                    result = new JsonData(result as Dictionary<string, object>);
                }
                else if (result is System.Collections.ArrayList)
                {
                    result = new JsonData(result as System.Collections.ArrayList);
                }
                return true;
            }
            else if (data != null)
            {
                if (indexes[0] is int i1)
                {
                    string[] keys = new string[data.Keys.Count];
                    data.Keys.CopyTo(keys, 0);
                    result = keys[i1];
                    return true;
                }
                else if (indexes[0] is string)
                {
                    if (data.ContainsKey(indexes[0].ToString()))
                    {
                        result = data[indexes[0].ToString()];
                        if (result is Dictionary<string, object>)
                        {
                            result = new JsonData(result as Dictionary<string, object>);
                        }
                        else if (result is System.Collections.ArrayList)
                        {
                            result = new JsonData(result as System.Collections.ArrayList);
                        }
                        return true;
                    }
                }
            }
            return base.TryGetIndex(binder, indexes, out result);
        }
        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name == "ToArray")
            {
                if (list != null)
                {
                    result = list.ToArray();
                    return true;
                }
            }
            return base.TryInvokeMember(binder, args, out result);
        }
        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            if (data != null)
            {
                if (data.ContainsKey(binder.Name))
                {
                    result = data[binder.Name];
                    if (result is Dictionary<string, object>)
                    {
                        result = new JsonData(result as Dictionary<string, object>);
                    }
                    else if (result is System.Collections.ArrayList)
                    {
                        result = new JsonData(result as System.Collections.ArrayList);
                    }
                    return true;
                }
                else
                {
                    result = data.GetType().InvokeMember(binder.Name, System.Reflection.BindingFlags.GetProperty, null, data, null);
                    return true;
                }
            }
            else
            {
                result = list.GetType().InvokeMember(binder.Name, System.Reflection.BindingFlags.GetProperty, null, list, null);
                return true;
            }
        }
        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            if (data != null)
            {
                if (data.ContainsKey(binder.Name))
                {
                    data[binder.Name] = value;
                    return true;
                }
            }
            else
            {
                list.GetType().InvokeMember(binder.Name, System.Reflection.BindingFlags.SetProperty, null, list, new object[] { value });
                return true;
            }
            return base.TrySetMember(binder, value);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            object result;
            if (list == null)
            {
                foreach (string key in data.Keys)
                {
                    result = data[key];
                    if (result is Dictionary<string, object>)
                    {
                        result = new KeyValuePair<string, object>(key, new JsonData(result as Dictionary<string, object>));// new JsonData(result as System.Collections.ArrayList);
                        //result = new JsonData(result as Dictionary<string, object>);
                    }
                    else if (result is System.Collections.ArrayList)
                    {
                        result = new KeyValuePair<string, object>(key, new JsonData(result as System.Collections.ArrayList));// new JsonData(result as System.Collections.ArrayList);
                    }
                    else
                    {
                        result = new KeyValuePair<string, object>(key, result);
                    }
                    yield return result;
                }
            }
            else
            {
                foreach (object key in list)
                {
                    result = key;
                    if (result is Dictionary<string, object>)
                    {
                        result = new JsonData(result as Dictionary<string, object>);
                    }
                    else if (result is System.Collections.ArrayList)
                    {
                        result = new JsonData(result as System.Collections.ArrayList);
                    }
                    yield return result;
                }
            }
        }
    }
}
