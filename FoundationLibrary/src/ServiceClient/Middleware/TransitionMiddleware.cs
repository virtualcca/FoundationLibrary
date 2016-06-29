namespace ServiceClients.Middleware
{
    /// <summary>
    ///     默认的会话中间件
    ///     <remarks>该中间件应该位于处理管道的第一位</remarks>
    /// </summary>
    public class TransitionMiddleware : ServiceClientMiddleware
    {
        /// <summary>
        ///     初始化中间件
        /// </summary>
        /// <param name="next"></param>
        public TransitionMiddleware(ServiceClientMiddleware next) : base(next)
        {
        }

        /// <summary>
        ///     执行中间件处理.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override void Invoke(ServiceClientContext context)
        {
            Next.Invoke(context);
        }
    }
}