using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServiceClients;
using Xunit;
using ServiceClient = ServiceClients.ServiceClient;

namespace ServiceClientTest
{
    public class ServiceClientUnitTest
    {
        /// <summary>
        ///     数据转QueryString的并发测试
        /// </summary>
        [Fact]
        public void Parallel_FormUrl_SimpleClass()
        {
            var testA = new TestClassA { A = "a" };

            Parallel.For(1, 100, (i, s) =>
            {
                var result = ServiceClient.FormatUrl("abc.com", testA, HttpVerb.Get);
                Assert.Equal("abc.com?A=a", result);
            });


        }

        /// <summary>
        ///     反序列化测试
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Parallel_Post()
        {
            var serviceClient = new ServiceClient(new TestHttpHandler());
            Parallel.For(1, 60, (i, s) =>
            {
                var result = serviceClient.RequestAsync<ComplateClass>("http://www.bing.com", HttpVerb.Post, null).ConfigureAwait(false).GetAwaiter().GetResult();
                Assert.Equal(result.A, "a");
                Assert.Equal(result.B, "b");
                Assert.NotNull(result.TestClassA);
                Assert.NotNull(result.ListInt);
                Assert.Null(result.TestClassB);
                Assert.Contains(1, result.ListInt);
            });
        }


        public class TestClassA
        {
            public string A { get; set; }
        }

        public class TestClassB
        {
            public string B { get; set; }
        }

        public class ComplateClass
        {
            public string A { get; set; }

            public string B { get; set; }

            public TestClassA TestClassA { get; set; }

            public TestClassB TestClassB { get; set; }

            public List<int> ListInt { get; set; }
        }

        /// <summary>
        ///     测试Handler
        /// </summary>
        public class TestHttpHandler : DelegatingHandler
        {
            /// <inheritdoc />
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(new ComplateClass
                {
                    A = "a",
                    B = "b",
                    ListInt = new List<int> { 1, 2, 3, 4 },
                    TestClassA = new TestClassA { A = "A" }
                }));
                return Task.FromResult(response);
            }
        }

    }
}
