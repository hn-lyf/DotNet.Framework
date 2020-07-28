using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Responses;
using Ocelot.Values;

namespace DotNet.Web.Ocelot
{
    public class CustomLoadBalancer : ILoadBalancer
    {
        private readonly Func<Task<List<Service>>> _services;
        private readonly object _lock = new object();

        private int _last;

        public CustomLoadBalancer(Func<Task<List<Service>>> services)
        {
            _services = services;
        }
        public CustomLoadBalancer()
        {
           
        }
        public async Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
        {
            var services = await _services();
            lock (_lock)
            {
                if (_last >= services.Count)
                {
                    _last = 0;
                }

                var next = services[_last];
                _last++;
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                for (var i = 0; i < st.FrameCount; i++)
                {
                    var method = st.GetFrame(i).GetMethod();
                    Console.WriteLine(method.Name);
                }
                return new OkResponse<ServiceHostAndPort>(next.HostAndPort);
            }
        }

        public void Release(ServiceHostAndPort hostAndPort)
        {
            //Ocelot.Responder.Middleware.ResponderMiddleware
        }
    }
}
