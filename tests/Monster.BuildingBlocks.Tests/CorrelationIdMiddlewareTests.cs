using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Monster.BuildingBlocks;
using Xunit;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task Adds_header_when_missing()
    {
        var ctx = new DefaultHttpContext();
        var mw = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await mw.Invoke(ctx);

        ctx.Response.Headers.TryGetValue(CorrelationIdMiddleware.Header, out var val).Should().BeTrue();
        val.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Preserves_existing_header()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers[CorrelationIdMiddleware.Header] = "abc123";
        var mw = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await mw.Invoke(ctx);

        ctx.Response.Headers[CorrelationIdMiddleware.Header].ToString().Should().Be("abc123");
    }
}
