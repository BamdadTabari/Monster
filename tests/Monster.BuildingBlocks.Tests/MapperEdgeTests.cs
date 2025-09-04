using System;
using System.Collections.Generic;
using FluentAssertions;
using Monster.BuildingBlocks;
using Xunit;

public class MapperEdgeTests
{
    private class HasIndexer { public int this[int i] { get => i; set { } } public string Name { get; set; } = "ok"; }
    private class HasName { public string Name { get; set; } = ""; }

    [Fact]
    public void Skips_indexers_and_maps_rest()
    {
        var dto = Mapper.Map<HasIndexer, HasName>(new HasIndexer());
        dto.Name.Should().Be("ok");
    }

    private class ToArray { public List<int> Items { get; set; } = new() { 1, 2, 3 }; }
    private class ToArrayDto { public int[] Items { get; set; } = Array.Empty<int>(); }

    [Fact]
    public void Converts_list_to_array()
    {
        var dto = Mapper.Map<ToArray, ToArrayDto>(new ToArray());
        dto.Items.Should().BeEquivalentTo([1,2,3]);
    }
}
