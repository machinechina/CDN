using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CDN.Infrastructure
{
    public class TimeoutWebClient : WebClient
    {
        int _timeout;
        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);
            req.Timeout = _timeout;
            return req;
        }

        public TimeoutWebClient(int timeout = 0)
        {
            _timeout = timeout;
        }
    }
}
