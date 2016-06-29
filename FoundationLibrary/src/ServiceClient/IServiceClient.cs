using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServiceClients
{
    /// <summary>
    ///     网络请求服务客户端
    /// </summary>
    public interface IServiceClient : IDisposable
    {
        /// <summary>
        ///     默认Http请求头
        /// </summary>
        HttpRequestHeaders DefaultRequestHeaders { get; }

        /// <summary>
        ///     超时时间
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <typeparam name="T">返回结果的类型</typeparam>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="requestObj">请求参数</param>
        /// <returns>结果反序列化为<typeparamref name="T" />后返回</returns>
        Task<T> RequestAsync<T>(string url, HttpVerb method,
            object requestObj);

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="requestObj">请求参数</param>
        /// <returns>返回字符串表示的结果</returns>
        Task<string> RequestAsync(string url, HttpVerb method, object requestObj);

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <typeparam name="T">返回结果的类型</typeparam>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="content">自定义请求Http信息</param>
        /// <returns>结果反序列化为<typeparamref cref="T" />后返回</returns>
        Task<T> RequestAsync<T>(string url, HttpVerb method,
            HttpContent content);

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="content">自定义请求Http信息</param>
        /// <returns>返回字符串表示的结果</returns>
        Task<string> RequestAsync(string url, HttpVerb method, HttpContent content);
    }
}