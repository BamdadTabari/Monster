using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Monster.BuildingBlocks;
using Xunit;

public class CurrentUserTests
{
    [Fact]
    public void Reads_id_and_roles_when_authenticated()
    {
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "u-1"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Editor")
        }, authenticationType: "test"));

        var accessor = new HttpContextAccessor { HttpContext = ctx };
        ICurrentUser cu = new HttpCurrentUser(accessor);

        cu.IsAuthenticated.Should().BeTrue();
        cu.UserId.Should().Be("u-1");
        cu.Roles.Should().BeEquivalentTo("Admin", "Editor");
    }
}
