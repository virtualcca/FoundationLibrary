using System.Net.Http;

namespace ServiceClients
{
    /// <summary>
    ///     <see cref="IServiceClient" />的扩展方法
    /// </summary>
    public static class ServiceClientExtensions
    {
        /// <summary>
        ///     同步执行的Http请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceClient"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="requestObj"></param>
        /// <returns></returns>
        public static T Request<T>(this IServiceClient serviceClient, string url, HttpVerb method,
            object requestObj)
        {
            return serviceClient.RequestAsync<T>(url, method, requestObj).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     同步执行的Http请求
        /// </summary>
        /// <param name="serviceClient"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="requestObj"></param>
        /// <returns></returns>
        public static string Request(this IServiceClient serviceClient, string url, HttpVerb method,
            object requestObj)
        {
            return serviceClient.RequestAsync(url, method, requestObj).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     同步执行的Http请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceClient"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T Request<T>(this IServiceClient serviceClient, string url, HttpVerb method,
            HttpContent context)
        {
            return serviceClient.RequestAsync<T>(url, method, context).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     同步执行的Http请求
        /// </summary>
        /// <param name="serviceClient"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Request(this IServiceClient serviceClient, string url, HttpVerb method,
            HttpContent context)
        {
            return serviceClient.RequestAsync(url, method, context).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}