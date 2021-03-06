using System;

#if NET461
using System.Net.Http;
#endif

using System.Reflection;
using System.Threading.Tasks;

using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Abstractions;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Visitors;

using FluentAssertions;

#if !NET461
using Microsoft.AspNetCore.Http;
#endif

using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Aliencube.AzureFunctions.Extensions.OpenApi.Core.Tests
{
    [TestClass]
    public class DocumentTests
    {
        [TestMethod]
        public void Given_Null_Constructor_Should_Throw_Exception()
        {
            Action action = () => new Document(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Given_That_When_InitialiseDocument_Invoked_Then_It_Should_Return_Result()
        {
            var helper = new Mock<IDocumentHelper>();
            var doc = new Document(helper.Object);

            var result = doc.InitialiseDocument();

            result.Should().NotBeNull();
            doc.OpenApiDocument.Should().NotBeNull();
        }

        [TestMethod]
        public void Given_That_When_AddNamingStrategy_Invoked_Then_It_Should_Return_Result()
        {
            var field = typeof(Document).GetField("_strategy", BindingFlags.Instance | BindingFlags.NonPublic);
            var strategy = new DefaultNamingStrategy();
            var helper = new Mock<IDocumentHelper>();
            var doc = new Document(helper.Object);

            var result = doc.AddNamingStrategy(strategy);

            field.GetValue(result).Should().NotBeNull();
            field.GetValue(result).Should().BeOfType<DefaultNamingStrategy>();
        }

        [TestMethod]
        public void Given_That_When_AddVisitors_Invoked_Then_It_Should_Return_Result()
        {
            var field = typeof(Document).GetField("_collection", BindingFlags.Instance | BindingFlags.NonPublic);
            var collection = new VisitorCollection();
            var helper = new Mock<IDocumentHelper>();
            var doc = new Document(helper.Object);

            var result = doc.AddVisitors(collection);

            field.GetValue(result).Should().NotBeNull();
            field.GetValue(result).Should().BeOfType<VisitorCollection>();
        }

        [TestMethod]
        public async Task Given_VersionAndFormat_RenderAsync_Should_Return_Result()
        {
            var helper = new Mock<IDocumentHelper>();
            var doc = new Document(helper.Object);

            var result = await doc.InitialiseDocument()
                                  .RenderAsync(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json);

            dynamic json = JObject.Parse(result);

            ((string)json?.swagger).Should().BeEquivalentTo("2.0");
        }

        [TestMethod]
        public async Task Given_Metadata_RenderAsync_Should_Return_Result()
        {
            var helper = new Mock<IDocumentHelper>();

            var title = "hello world";
            var info = new OpenApiInfo() { Title = title };

            var doc = new Document(helper.Object);

            var result = await doc.InitialiseDocument()
                                  .AddMetadata(info)
                                  .RenderAsync(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json);

            dynamic json = JObject.Parse(result);

            ((string)json?.info?.title).Should().BeEquivalentTo(title);
        }

        [TestMethod]
        public async Task Given_ServerDetails_RenderAsync_Should_Return_Result()
        {
            var helper = new Mock<IDocumentHelper>();

            var scheme = "https";
            var host = "localhost";
            var routePrefix = "api";
            var url = $"{scheme}://{host}";
#if NET461
            var uri = new Uri(url);
            var req = new HttpRequestMessage() { RequestUri = uri };
#else
            var req = new Mock<HttpRequest>();
            req.SetupGet(p => p.Scheme).Returns(scheme);
            req.SetupGet(p => p.Host).Returns(new HostString(host));
#endif
            var doc = new Document(helper.Object);

            var result = await doc.InitialiseDocument()
#if NET461
                                  .AddServer(req, routePrefix)
#else
                                  .AddServer(req.Object, routePrefix)
#endif
                                  .RenderAsync(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json);

            dynamic json = JObject.Parse(result);

            ((string)json?.host).Should().BeEquivalentTo(host);
            ((string)json?.basePath).Should().BeEquivalentTo($"/{routePrefix}");
            ((string)json?.schemes[0]).Should().BeEquivalentTo(scheme);
        }

        [TestMethod]
        public async Task Given_ServerDetails_WithNullRoutePrefix_RenderAsync_Should_Return_Result()
        {
            var helper = new Mock<IDocumentHelper>();

            var scheme = "https";
            var host = "localhost";
            string routePrefix = null;
            var url = $"{scheme}://{host}";
#if NET461
            var uri = new Uri(url);
            var req = new HttpRequestMessage() { RequestUri = uri };
#else
            var req = new Mock<HttpRequest>();
            req.SetupGet(p => p.Scheme).Returns(scheme);
            req.SetupGet(p => p.Host).Returns(new HostString(host));
#endif
            var doc = new Document(helper.Object);

            var result = await doc.InitialiseDocument()
#if NET461
                                  .AddServer(req, routePrefix)
#else
                                  .AddServer(req.Object, routePrefix)
#endif
                                  .RenderAsync(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json);

            dynamic json = JObject.Parse(result);

            ((string)json?.host).Should().BeEquivalentTo(host);
            ((string)json?.basePath).Should().BeEquivalentTo(null);
            ((string)json?.schemes[0]).Should().BeEquivalentTo(scheme);
        }

        [TestMethod]
        public async Task Given_ServerDetails_WithEmptyRoutePrefix_RenderAsync_Should_Return_Result()
        {
            var helper = new Mock<IDocumentHelper>();

            var scheme = "https";
            var host = "localhost";
            var routePrefix = string.Empty;
            var url = $"{scheme}://{host}";
#if NET461
            var uri = new Uri(url);
            var req = new HttpRequestMessage() { RequestUri = uri };
#else
            var req = new Mock<HttpRequest>();
            req.SetupGet(p => p.Scheme).Returns(scheme);
            req.SetupGet(p => p.Host).Returns(new HostString(host));
#endif
            var doc = new Document(helper.Object);

            var result = await doc.InitialiseDocument()
#if NET461
                                  .AddServer(req, routePrefix)
#else
                                  .AddServer(req.Object, routePrefix)
#endif
                                  .RenderAsync(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json);

            dynamic json = JObject.Parse(result);

            ((string)json?.host).Should().BeEquivalentTo(host);
            ((string)json?.basePath).Should().BeEquivalentTo(null);
            ((string)json?.schemes[0]).Should().BeEquivalentTo(scheme);
        }
    }
}
