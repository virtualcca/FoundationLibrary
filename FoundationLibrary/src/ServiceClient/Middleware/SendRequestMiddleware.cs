using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceClients.Middleware
{
    /// <summary>
    ///     请求发送中间件
    /// </summary>
    /// <remarks>
    ///     此中间件应该位于处理管道的最后一位
    /// </remarks>
    public class SendRequestMiddleware : ServiceClientMiddleware
    {
        /// <summary>
        ///     初始化中间件
        /// </summary>
        /// <param name="next"></param>
        public SendRequestMiddleware(ServiceClientMiddleware next) : base(next)
        {
        }

        /// <summary>
        ///     执行中间件处理.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task Invoke(ServiceClientContext context)
        {
            HttpResponseMessage result;
            switch (context.HttpVerb)
            {
                case HttpVerb.Get:
                    result =
                        await
                            context.InnerHttpClient.GetAsync(context.Url).ConfigureAwait(false);
                    break;
                case HttpVerb.Post:
                    result =
                        await
                            context.InnerHttpClient.PostAsync(context.Url, context.RequestContent).ConfigureAwait(false);
                    break;
                case HttpVerb.Put:
                    result =
                        await context.InnerHttpClient.PutAsync(context.Url, context.RequestContent).ConfigureAwait(false);
                    break;
                case HttpVerb.Delete:
                    result =
                        await
                            context.InnerHttpClient.DeleteAsync(context.Url).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(context.HttpVerb.ToString(), context.HttpVerb, null);
            }
            context.ResponseMessage = result;
        }
    }
}
