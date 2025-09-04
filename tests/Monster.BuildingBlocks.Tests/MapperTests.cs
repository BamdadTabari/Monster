using System;
using System.Collections.Generic;
using FluentAssertions;
using Monster.BuildingBlocks;
using Xunit;

namespace Monster.BuildingBlocks.Tests;

public class MapperTests
{
    // ----- model types used only for tests -----
    private class Node
    {
        public string? Name { get; set; }
        public Node? Parent { get; set; }
        public List<Node>? Children { get; set; }
    }

    private class NodeDto
    {
        public string? Name { get; set; }
        public NodeDto? Parent { get; set; }
        public IList<NodeDto>? Children { get; set; }
    }

    private class Basket
    {
        public Dictionary<string, int>? Items { get; set; }
        public string? PlacedAt { get; set; }   // ISO string to be converted
        public string? Status { get; set; }     // string -> enum
    }

    private enum OrderStatus { Pending = 0, Paid = 1 }

    private class BasketDto
    {
        public Dictionary<string, long>? Items { get; set; } // int -> long
        public DateTime PlacedAt { get; set; }               // string -> DateTime
        public OrderStatus Status { get; set; }              // string -> enum
    }

    [Fact]
    public void Maps_simple_properties_and_nullables()
    {
        var src = new SimpleEntity{ Id = Guid.NewGuid(), Count = (int?)null };
        var dst = Mapper.Map<SimpleEntity, SimpleDto>(src);

        dst.Id.Should().NotBeEmpty();
        dst.Count.Should().BeNull();
    }

    private class SimpleDto { public Guid Id { get; set; } public int? Count { get; set; } }
    private class SimpleEntity { public Guid Id { get; set; } public int? Count { get; set; } }

    [Fact]
    public void Handles_cycles_without_infinite_loop()
    {
        var parent = new Node { Name = "P" };
        var child = new Node { Name = "C", Parent = parent };
        parent.Children = new List<Node> { child };

        var dto = Mapper.Map<Node, NodeDto>(parent);

        dto.Name.Should().Be("P");
        dto.Children.Should().NotBeNull();
        dto.Children![0].Parent.Should().NotBeNull();
        dto.Children[0].Parent!.Name.Should().Be("P");
    }

    [Fact]
    public void Maps_collections_and_arrays_between_shapes()
    {
        var parent = new Node { Name = "Root", Children = new List<Node> {
            new Node { Name = "A" }, new Node { Name = "B" }
        }};
        var dto = Mapper.Map<Node, NodeDto>(parent);

        dto.Children.Should().NotBeNull();
        dto.Children!.Count.Should().Be(2);
        dto.Children[0].Name.Should().Be("A");
        dto.Children[1].Name.Should().Be("B");
    }

    [Fact]
    public void Maps_dictionary_and_converts_numeric_and_enum_and_dates()
    {
        var src = new Basket
        {
            Items = new Dictionary<string, int> { ["apple"] = 2, ["pear"] = 5 },
            PlacedAt = "2024-01-02T03:04:05.0000000Z",
            Status = "Paid"
        };

        var dto = Mapper.Map<Basket, BasketDto>(src);

        dto.Items!.Should().ContainKey("apple").WhoseValue.Should().Be(2);
        dto.Items.Should().ContainKey("pear").WhoseValue.Should().Be(5);
        dto.PlacedAt.Kind.Should().Be(DateTimeKind.Utc);
        dto.Status.Should().Be(OrderStatus.Paid);
    }
}
