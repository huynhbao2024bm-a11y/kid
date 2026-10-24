using Kid.Mcp;

namespace Kid.Mcp.Tests;

public sealed class UniqueElementSearchPolicyTests
{
    [Fact]
    public void ResolveReturnsOnlyMatchWhenSearchWasComplete()
    {
        var match = new object();

        Assert.Same(match, UniqueElementSearchPolicy.Resolve([match], wasTruncated: false, skippedUnavailableElements: 0));
    }

    [Fact]
    public void ResolveRejectsOneMatchWhenSearchWasTruncated()
    {
        var match = new object();

        var exception = Assert.Throws<InvalidOperationException>(
            () => UniqueElementSearchPolicy.Resolve([match], wasTruncated: true, skippedUnavailableElements: 0));

        Assert.Contains("Could not verify", exception.Message);
    }

    [Fact]
    public void ResolveRejectsSearchWhenElementDisappearedDuringTraversal()
    {
        var match = new object();

        var exception = Assert.Throws<InvalidOperationException>(
            () => UniqueElementSearchPolicy.Resolve([match], wasTruncated: false, skippedUnavailableElements: 1));

        Assert.Contains("1 UI element(s) became unavailable", exception.Message);
    }

    [Fact]
    public void ResolveRejectsMultipleMatches()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => UniqueElementSearchPolicy.Resolve([new object(), new object()], wasTruncated: false, skippedUnavailableElements: 0));

        Assert.Contains("More than one element", exception.Message);
    }

    [Fact]
    public void ResolveRejectsNoMatchesWhenTreeWasTruncated()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => UniqueElementSearchPolicy.Resolve(Array.Empty<object>(), wasTruncated: true, skippedUnavailableElements: 0));

        Assert.Contains("Could not verify", exception.Message);
    }

    [Fact]
    public void ResolveReportsNoMatchWhenSearchWasComplete()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => UniqueElementSearchPolicy.Resolve(Array.Empty<object>(), wasTruncated: false, skippedUnavailableElements: 0));

        Assert.Contains("No accessible element matched", exception.Message);
    }

    [Fact]
    public void BoundedSearchDoesNotTreatMatchAsUniqueWhenSiblingExceedsLimit()
    {
        var matchingElements = new[]
        {
            new SearchNode("target"),
            new SearchNode("other"),
            new SearchNode("target")
        };
        var root = new SearchNode("root", matchingElements);
        var result = Search(root, maximumElements: 2, maximumDepth: 12);

        Assert.Single(result.Matches);
        Assert.True(result.WasTruncated);
        Assert.Throws<InvalidOperationException>(
            () => UniqueElementSearchPolicy.Resolve(
                result.Matches,
                result.WasTruncated,
                result.SkippedUnavailableElements));
    }

    [Fact]
    public void BoundedSearchDetectsChildrenBeyondDepthLimit()
    {
        var deeplyNestedMatch = new SearchNode("target");
        var childAtDepthLimit = new SearchNode("target", [deeplyNestedMatch]);
        var root = new SearchNode("root", [childAtDepthLimit]);
        var result = Search(root, maximumElements: 10, maximumDepth: 1);

        Assert.Single(result.Matches);
        Assert.True(result.WasTruncated);
        Assert.Throws<InvalidOperationException>(
            () => UniqueElementSearchPolicy.Resolve(
                result.Matches,
                result.WasTruncated,
                result.SkippedUnavailableElements));
    }

    [Fact]
    public void BoundedSearchAllowsUniqueMatchWhenTreeWasFullyVisited()
    {
        var match = new SearchNode("target");
        var root = new SearchNode("root", [match, new SearchNode("other")]);

        var result = Search(root, maximumElements: 10, maximumDepth: 12);

        Assert.False(result.WasTruncated);
        Assert.Same(match, UniqueElementSearchPolicy.Resolve(
            result.Matches,
            result.WasTruncated,
            result.SkippedUnavailableElements));
    }

    [Fact]
    public void BoundedSearchMarksUnavailableElementAsIncomplete()
    {
        var root = new SearchNode("root", [new SearchNode("target"), new SearchNode("disappeared")]);
        var result = BoundedTreeSearch.Search(
            root,
            (node, capacity) =>
            {
                var children = node.Children.Take(capacity).ToArray();
                return new BoundedTreeSearch.BoundedChildren<SearchNode>(
                    children,
                    node.Children.Count > children.Length);
            },
            node => node.Name switch
            {
                "target" => BoundedTreeSearch.TreeMatchState.Match,
                "disappeared" => BoundedTreeSearch.TreeMatchState.Unavailable,
                _ => BoundedTreeSearch.TreeMatchState.NoMatch
            },
            maximumElements: 10,
            maximumDepth: 12);

        Assert.Single(result.Matches);
        Assert.True(result.WasTruncated);
        Assert.Equal(1, result.SkippedUnavailableElements);
        Assert.Throws<InvalidOperationException>(
            () => UniqueElementSearchPolicy.Resolve(
                result.Matches,
                result.WasTruncated,
                result.SkippedUnavailableElements));
    }

    private static BoundedTreeSearch.TreeSearchResult<SearchNode> Search(
        SearchNode root,
        int maximumElements,
        int maximumDepth) =>
        BoundedTreeSearch.Search(
            root,
            (node, capacity) =>
            {
                var children = node.Children.Take(capacity).ToArray();
                return new BoundedTreeSearch.BoundedChildren<SearchNode>(
                    children,
                    node.Children.Count > children.Length);
            },
            node => node.Name == "target"
                ? BoundedTreeSearch.TreeMatchState.Match
                : BoundedTreeSearch.TreeMatchState.NoMatch,
            maximumElements,
            maximumDepth);

    private sealed record SearchNode(string Name, IReadOnlyList<SearchNode> Children)
    {
        internal SearchNode(string name) : this(name, Array.Empty<SearchNode>())
        {
        }
    }
}
