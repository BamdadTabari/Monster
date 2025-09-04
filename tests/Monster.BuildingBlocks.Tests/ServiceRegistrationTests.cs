using System.Linq;
using FluentAssertions;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Monster.BuildingBlocks;
using Xunit;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddMonsterBuildingBlocks_registers_basics()
    {
        var sc = new ServiceCollection();
        sc.AddMonsterBuildingBlocks();

        sc.Any(d => d.ServiceType == typeof(IDateTimeProvider)).Should().BeTrue();
        sc.Any(d => d.ServiceType == typeof(IIdGenerator)).Should().BeTrue();
        sc.Any(d => d.ServiceType == typeof(IPipelineBehavior<,>) &&
                    d.ImplementationType == typeof(ValidationBehavior<,>)).Should().BeTrue();
    }
}
