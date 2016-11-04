using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDN.Infrastructure
{
    public static class Extensions
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            return col.Keys.Cast<string>()
                      .ToDictionary(k => k, v => col[v]);
        }
    }
}
