using System.Net.Http;

namespace ServiceClients.Middleware
{
    /// <summary>
    ///     <see cref="ServiceClient"/> 请求的上下文
    /// </summary>
    public abstract class ServiceClientContext
    {
        /// <summary>
        ///     请求Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     请求的Http方法
        /// </summary>
        public HttpVerb HttpVerb { get; set; }

        /// <summary>
        ///     请求内容
        /// </summary>
        public HttpContent RequestContent { get; set; }

        /// <summary>
        ///     返回的数据
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; internal set; }
    }
}
