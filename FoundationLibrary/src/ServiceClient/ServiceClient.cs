using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
// ReSharper disable PossibleMultipleEnumeration
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
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> PropertiesCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        #endregion

        #region [ Property ]

        /// <summary>
        ///     默认网络请求客户端实例
        ///     <para>由于<see cref="HttpClient" />是为线程安全的类,所以本类以静态单例的模式提供服务也同样是线程安全的</para>
        /// </summary>
        public static ServiceClient Default => _defaultInstance ?? (_defaultInstance = new ServiceClient());

        /// <summary>
        ///     Json序列化设置
        /// </summary>
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        /// <summary>
        ///     内部的<see cref="HttpClient" />实例
        /// </summary>
        private HttpClient InnerHttpClient { get; }

        /// <summary>
        ///     超时时间
        /// </summary>
        public TimeSpan Timeout
        {
            get => InnerHttpClient.Timeout;
        }

        /// <summary>
        ///     默认Http请求头
        /// </summary>
        public HttpRequestHeaders DefaultRequestHeaders => InnerHttpClient.DefaultRequestHeaders;

        /// <summary>
        ///     异常日志记录委托
        /// </summary>
        public static Action<ExceptionData> ExceptionLogger { get; set; }

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
                BaseAddress = baseAddress,
                Timeout = timeout
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
        /// <returns>结果反序列化为<typeparamref name="T"/>后返回</returns>
        public Task<T> RequestAsync<T>(string url, HttpVerb method)
        {
            return RequestAsync<T>(url, method, null);
        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <typeparam name="T">返回结果的类型</typeparam>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="requestObj">请求参数</param>
        /// <returns>结果反序列化为<typeparamref name="T"/>后返回</returns>
        public Task<T> RequestAsync<T>(string url, HttpVerb method, object requestObj)
        {
            return RequestAsync<T>(url, method, requestObj, CancellationToken.None);
        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <returns>返回字符串表示的结果</returns>
        public Task<string> RequestAsync(string url, HttpVerb method)
        {
            return RequestAsync(url, method, null, CancellationToken.None);
        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="requestObj">请求参数</param>
        /// <returns>返回字符串表示的结果</returns>
        public Task<string> RequestAsync(string url, HttpVerb method, object requestObj)
        {
            return RequestAsync(url, method, requestObj, CancellationToken.None);
        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <typeparam name="T">返回结果的类型</typeparam>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="content">自定义请求Http信息</param>
        /// <returns>结果反序列化为<typeparamref name="T"/>后返回</returns>
        public Task<T> RequestAsync<T>(string url, HttpVerb method, HttpContent content)
        {
            return RequestAsync<T>(url, method, content, CancellationToken.None);
        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="content">自定义请求Http信息</param>
        /// <returns>返回字符串表示的结果</returns>
        public Task<string> RequestAsync(string url, HttpVerb method, HttpContent content)
        {
            return RequestAsync(url, method, content, CancellationToken.None);
        }

        /// <summary>
        ///     透过<see cref="HttpRequestMessage"/>进行原始的Http请求并获取未处理的<see cref="HttpResponseMessage"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(Timeout);
            return InnerHttpClient.SendAsync(request, cts.Token);
        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <typeparam name="T">返回结果的类型</typeparam>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="requestObj">请求参数</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> RequestAsync<T>(string url, HttpVerb method, object requestObj, CancellationToken cancellationToken)
        {
            var body = FormatParameter(requestObj, method);
            url = FormatUrl(url, requestObj, method);

            return RequestAsync<T>(url, method, body, cancellationToken);
        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="requestObj">请求参数</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> RequestAsync(string url, HttpVerb method, object requestObj, CancellationToken cancellationToken)
        {
            var body = FormatParameter(requestObj, method);
            url = FormatUrl(url, requestObj, method);

            return RequestAsync(url, method, body, cancellationToken);
        }


        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <typeparam name="T">返回结果的类型</typeparam>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="content">自定义请求Http信息</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> RequestAsync<T>(string url, HttpVerb method, HttpContent content,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await SendRequest(url, method, content, cancellationToken).ConfigureAwait(false);
                return DeserializeFromStream<T>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
            }
            finally
            {
                content?.Dispose();
            }


        }

        /// <summary>
        ///     通过Http请求数据
        /// </summary>
        /// <param name="url">请求地址Url</param>
        /// <param name="method">Http请求谓词</param>
        /// <param name="content">自定义请求Http信息</param>
        /// <param name="cancellationToken">请求取消令牌</param>
        /// <returns>返回字符串表示的结果</returns>
        public async Task<string> RequestAsync(string url, HttpVerb method, HttpContent content, CancellationToken cancellationToken)
        {
            try
            {
                var response = await SendRequest(url, method, content, cancellationToken).ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            finally
            {
                content?.Dispose();
            }
        }


        #endregion

        #region [ Private Method ]

        private static T DeserializeFromStream<T>(Stream jsonStream)
        {
            if (jsonStream == null)
                throw new ArgumentNullException(nameof(jsonStream));

            if (!jsonStream.CanRead)
                throw new ArgumentException("Json Stream must support reading", nameof(jsonStream));

            T deserializedObj;
            using (var sr = new StreamReader(jsonStream))
            {
                using (var reader = new JsonTextReader(sr))
                {
                    var serializer = JsonSerializer.CreateDefault();
                    deserializedObj = serializer.Deserialize<T>(reader);
                }
            }
            return deserializedObj;
        }


        private async Task<HttpResponseMessage> SendRequest(string url, HttpVerb httpVerb, HttpContent body, CancellationToken cancellationToken)
        {
            HttpResponseMessage result;
            try
            {
                switch (httpVerb)
                {
                    case HttpVerb.Get:
                        result =
                            await
                                InnerHttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
                        break;
                    case HttpVerb.Post:
                        result =
                            await
                                InnerHttpClient.PostAsync(url, body, cancellationToken).ConfigureAwait(false);
                        break;
                    case HttpVerb.Put:
                        result =
                            await InnerHttpClient.PutAsync(url, body, cancellationToken).ConfigureAwait(false);
                        break;
                    case HttpVerb.Patch:
                        result = await PatchAsync(url, body, cancellationToken).ConfigureAwait(false);
                        break;
                    case HttpVerb.Delete:
                        result =
                            await
                                InnerHttpClient.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(httpVerb.ToString(), httpVerb, null);
                }
            }
            catch (TaskCanceledException te)
            {
                ExceptionData.LogRequestTimeout(te, url, httpVerb);
                throw;
            }
            catch (Exception e)
            {
                ExceptionLogger?.Invoke(ExceptionData.LogRequest(e, url, httpVerb));
                throw;
            }


            if (!IsThrow)
                return result;

            if (!result.IsSuccessStatusCode)
            {
                var e = new HttpRequestException(string.Format(CultureInfo.InvariantCulture, "Response status code does not indicate success: {0} ({1}).",
       result.StatusCode,
       result.ReasonPhrase
   ));
                if ((int)result.StatusCode >= 500)
                    ExceptionLogger?.Invoke(ExceptionData.LogEnsureSuccessed(e, url, httpVerb));
                throw e;
            }

            return result;
        }

        private Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            return InnerHttpClient.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = content
            }, cancellationToken);
        }

        internal static string FormatUrl(string url, object requestObj, HttpVerb method)
        {
            if (method == HttpVerb.Get || method == HttpVerb.Delete)
            {
                if (requestObj == null)
                    return url;

                var type = requestObj.GetType();

                IEnumerable<PropertyInfo> props;
                if (!PropertiesCache.ContainsKey(type))
                {
                    props = requestObj.GetType().GetRuntimeProperties();
                    PropertiesCache.TryAdd(type, props);
                }
                else
                {
                    props = PropertiesCache[type];
                }

                var paramater = GetNameValueCollectionString(props.ToDictionary(
                        x => x.Name,
                        x => x.GetValue(requestObj) == null ? string.Empty : x.GetValue(requestObj).ToString()));

                return url.Contains("?")
                    ? $"{url}&{paramater}"
                    : $"{url}?{paramater}";
            }
            return url;
        }

        private static HttpContent FormatParameter(object requestObj, HttpVerb method)
        {
            HttpContent body;
            if (method != HttpVerb.Get && method != HttpVerb.Delete)
            {
                body = requestObj != null ? new StringContent(JsonConvert.SerializeObject(requestObj, Formatting.None, JsonSerializerSettings), Encoding.UTF8, "application/json") : new StringContent(string.Empty);
            }
            else
            {
                body = null;
            }
            return body;
        }

        private static string GetNameValueCollectionString(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
        {
            if (nameValueCollection == null)
                return string.Empty;
            var stringBuilder = new StringBuilder();
            foreach (var nameValue in nameValueCollection)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append('&');
                stringBuilder.Append(Encode(nameValue.Key));
                stringBuilder.Append('=');
                stringBuilder.Append(Encode(nameValue.Value));
            }
            return stringBuilder.ToString();
        }


        private static string Encode(string data)
        {
            return string.IsNullOrEmpty(data) ? string.Empty : Uri.EscapeDataString(data).Replace("%20", "+");
        }

        #endregion

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            InnerHttpClient.Dispose();
        }
    }
}