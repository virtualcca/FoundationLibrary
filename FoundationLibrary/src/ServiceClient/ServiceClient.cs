using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace ServiceClients
{
    /// <summary>
    ///     网络请求服务客户端
    ///     <para>对<see cref="System.Net.Http.HttpClient" />的一层封装,提供各种对REST风格的API请求操作</para>
    ///     <para>该类底层请求使用了<see cref="System.Net.Http.HttpClient" />,其请求方法均为线程安全</para>
    ///     <remarks>可使用<see cref="Default" />获取默认实例</remarks>
    /// </summary>
    public class ServiceClient : IServiceClient
    {
        #region [ Field ]
        private static ServiceClient _defaultInstance;
        private static readonly TimeSpan DefaultTimeout = new TimeSpan(0, 0, 30);
        #endregion

        #region [ Property ]

        /// <summary>
        ///     默认网络请求客户端实例
        ///     <para>由于<see cref="HttpClient" />是为线程安全的类,所以本类以静态单例的模式提供服务也同样是线程安全的</para>
        /// </summary>
        public static ServiceClient Default => _defaultInstance ?? (_defaultInstance = new ServiceClient());

        /// <summary>
        ///     内部的<see cref="HttpClient" />实例
        /// </summary>
        private HttpClient InnerHttpClient { get; }

        /// <summary>
        ///     超时时间
        /// </summary>
        public TimeSpan Timeout
        {
            get { return InnerHttpClient.Timeout; }
            set { InnerHttpClient.Timeout = value; }
        }

        /// <summary>
        ///     默认Http请求头
        /// </summary>
        public HttpRequestHeaders DefaultRequestHeaders => InnerHttpClient.DefaultRequestHeaders;

        /// <summary>
        ///     异常日志记录委托
        /// </summary>
        public static Action<string, Dictionary<string, string>, Exception> ExceptionLogger { get; set; }

        /// <summary>
        ///     当Http返回的请求状态码大于400的时候是否抛出异常
        ///     Default:true
        /// </summary>
        public bool IsThrow { get; set; }
        #endregion

        #region [ Ctor ]

        /// <summary>
        ///     创建用于网络请求的<see cref="ServiceClient" />实例
        /// </summary>
        public ServiceClient() : this(null, DefaultTimeout)
        {
        }

        /// <summary>
        ///     创建用于网络请求的<see cref="ServiceClient" />实例
        /// </summary>
        /// <param name="handler">Http消息处理程序</param>
        public ServiceClient(HttpMessageHandler handler) : this(null, DefaultTimeout, handler)
        {
        }

        /// <summary>
        ///     创建用于网络请求的<see cref="ServiceClient" />实例
        /// </summary>
        /// <param name="baseAddress">请求地址的基地址</param>
        public ServiceClient(Uri baseAddress) : this(baseAddress, DefaultTimeout)
        {
        }

        /// <summary>
        ///     创建用于网络请求的<see cref="ServiceClient" />实例
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="timeout"></param>
        /// <param name="handler"></param>
        public ServiceClient(Uri baseAddress, TimeSpan timeout, HttpMessageHandler handler = null)
        {
            handler = handler ?? new HttpClientHandler();
            IsThrow = true;
            InnerHttpClient = new HttpClient(handler)
            {
                Timeout = timeout,
                BaseAddress = baseAddress
            };
        }

        #endregion

        #region [ Public Method ]

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <typeparam name="T">返回结果的类型</typeparam>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="requestObj">请求参数</param>
        /// <returns>结果反序列化为<typeparamref cref="T" />后返回</returns>
        public async Task<T> RequestAsync<T>(string url, HttpVerb method, object requestObj)
        {
            using (var body = FormatParameter(requestObj, method))
            {
                url = await FormatUrl(url, body, method).ConfigureAwait(false);

                var response = await SendRequest(url, method, body).ConfigureAwait(false);

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    return JsonConvert.DeserializeObject<T>(result);
                }
                catch (Exception e)
                {
                    ExceptionLogger?.Invoke("Http请求反序列化异常", new Dictionary<string, string>
                    {
                        {"result",result },
                        {"httpverb",method.ToString() },
                        {"url",url }
                    }, e);
                    throw;
                }

            }

        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="requestObj">请求参数</param>
        /// <returns>返回字符串表示的结果</returns>
        public async Task<string> RequestAsync(string url, HttpVerb method, object requestObj)
        {
            using (var body = FormatParameter(requestObj, method))
            {
                url = await FormatUrl(url, body, method).ConfigureAwait(false);

                var response = await SendRequest(url, method, body).ConfigureAwait(false);

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <typeparam name="T">返回结果的类型</typeparam>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="content">自定义请求Http信息</param>
        /// <returns>结果反序列化为<typeparamref cref="T" />后返回</returns>
        public async Task<T> RequestAsync<T>(string url, HttpVerb method, HttpContent content)
        {
            using (content)
            {
                var response = await SendRequest(url, method, content).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(result);
            }

        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="content">自定义请求Http信息</param>
        /// <returns>返回字符串表示的结果</returns>
        public async Task<string> RequestAsync(string url, HttpVerb method, HttpContent content)
        {
            using (content)
            {
                var response = await SendRequest(url, method, content).ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region [ Private Method ]

        private async Task<HttpResponseMessage> SendRequest(string url, HttpVerb httpVerb, HttpContent body)
        {
            HttpResponseMessage result;
            switch (httpVerb)
            {
                case HttpVerb.Get:
                    result =
                        await
                            InnerHttpClient.GetAsync(url).ConfigureAwait(false);
                    break;
                case HttpVerb.Post:
                    result =
                        await
                            InnerHttpClient.PostAsync(url, body).ConfigureAwait(false);
                    break;
                case HttpVerb.Put:
                    result =
                        await InnerHttpClient.PutAsync(url, body).ConfigureAwait(false);
                    break;
                case HttpVerb.Delete:
                    result =
                        await
                            InnerHttpClient.DeleteAsync(url).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(httpVerb.ToString(), httpVerb, null);
            }

            if (!IsThrow)
                return result;

            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                if ((int)result.StatusCode >= 500)
                    ExceptionLogger?.Invoke("Http请求响应状态异常.", new Dictionary<string, string>
                    {
                        {"httpverb",httpVerb.ToString() },
                        {"url",url }
                    }, e);
            }

            return result;
        }

        private static async Task<string> FormatUrl(string url, HttpContent body, HttpVerb method)
        {
            if (method == HttpVerb.Get || method == HttpVerb.Delete)
            {
                var paramater = await body.ReadAsStringAsync().ConfigureAwait(false) ?? string.Empty;
                if (string.IsNullOrEmpty(paramater))
                    return url;
                return url.Contains("?")
                    ? $"{url}&{paramater}"
                    : $"{url}?{paramater}";
            }
            return url;
        }

        private static HttpContent FormatParameter(object requestObj, HttpVerb method)
        {
            HttpContent body;
            if (requestObj != null)
            {
                if (method != HttpVerb.Get && method != HttpVerb.Delete)
                {
                    body = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
                }
                else
                {
#if NET45
                    var props = (from x in requestObj.GetType().GetRuntimeProperties()
                                 select x).ToDictionary(
                            GetPropertyAttrName,
                            x => x.GetValue(requestObj) == null ? string.Empty : x.GetValue(requestObj).ToString());
                    body = new FormUrlEncodedContent(props);
#elif NET4
                    var props = (from x in requestObj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                 select x).ToDictionary(
                            GetPropertyAttrName,
                            x => x.GetValue(requestObj, null) == null ? string.Empty : x.GetValue(requestObj, null).ToString());
                    body = new FormUrlEncodedContent(props);
#endif
                }
            }
            else
            {
                body = new StringContent(string.Empty);
            }
            return body;
        }

        /// <summary>
        ///     如果定义了<see cref="JsonPropertyAttribute" />将读取其定义的属性名,否则将使用属性本身的名字
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private static string GetPropertyAttrName(PropertyInfo prop)
        {
#if NET4
            var attrName = string.Empty;
            var attr = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
            if (attr.Any())
            {
                var jsonPropertyAttribute = attr.First() as JsonPropertyAttribute;
                if (jsonPropertyAttribute != null)
                    attrName = jsonPropertyAttribute.PropertyName;
            }
            return attrName.Equals(string.Empty) ? prop.Name : attrName;
#else
            var attrName = string.Empty;
            var attr = prop.GetCustomAttribute<JsonPropertyAttribute>();
            if (!string.IsNullOrEmpty(attr?.PropertyName))
                attrName = attr.PropertyName;

            return attrName.Equals(string.Empty) ? prop.Name : attrName;
#endif
        }

        #endregion

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            InnerHttpClient.Dispose();
        }
    }
}