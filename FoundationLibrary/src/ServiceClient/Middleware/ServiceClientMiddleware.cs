namespace ServiceClients.Middleware
{
    /// <summary>
    ///     <see cref="ServiceClient" />的处理中间件
    /// </summary>
    public abstract class ServiceClientMiddleware
    {
        /// <summary>
        ///     初始化中间件
        /// </summary>
        /// <param name="next"></param>
        protected ServiceClientMiddleware(ServiceClientMiddleware next)
        {
            Next = next;
        }

        /// <summary>
        ///     当前处理管道中的下一个中间件.
        /// </summary>
        protected ServiceClientMiddleware Next { get; set; }

        /// <summary>
        ///     执行中间件处理.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract void Invoke(ServiceClientContext context);
    }
}