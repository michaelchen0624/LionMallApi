using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Prod.Core
{
    public interface IHttpUtils
    {
        string httpPost(string url, string postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null);
        Task<string> httpPostAsync(string url, string postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null);
        string httpGet(string url, string contentType = null, Dictionary<string, string> headers = null);
        Task<string> httpGetAsync(string url, string contentType = null, Dictionary<string, string> headers = null);


    }
}
