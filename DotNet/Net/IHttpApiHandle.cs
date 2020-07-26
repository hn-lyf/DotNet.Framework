using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net
{
    public interface IHttpApiHandle
    {
        HttpRequest Request { get; }
        void OnRequest(HttpRequest request);
    }
}
