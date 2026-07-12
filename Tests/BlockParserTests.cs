using Silkstring.Services;
using Silkstring.Services.Conditions;

public class BlockParserTests
{
    private static (IReadOnlyList<BlockNode> Nodes, IReadOnlyList<Diagnostic> Diags) Parse(params string[] lines)
        => BlockParser.Parse(lines);

    private static string Line(BlockNode node) => Assert.IsType<LineNode>(node).Text;

    [Fact]
    public void SingleIf()
    {
        var (nodes, diags) = Parse("if (a == 1) {", "/x", "}");
        Assert.Empty(diags);
        var ifn = Assert.IsType<IfNode>(Assert.Single(nodes));
        var branch = Assert.Single(ifn.Branches);
        Assert.Equal("a == 1", branch.Condition);
        Assert.Equal("/x", Line(Assert.Single(branch.Body)));
    }

    [Fact]
    public void IfElse()
    {
        var (nodes, diags) = Parse("if (a) {", "/x", "}", "else {", "/y", "}");
        Assert.Empty(diags);
        var ifn = Assert.IsType<IfNode>(Assert.Single(nodes));
        Assert.Equal(2, ifn.Branches.Count);
        Assert.Equal("a", ifn.Branches[0].Condition);
        Assert.Null(ifn.Branches[1].Condition);
        Assert.Equal("/x", Line(Assert.Single(ifn.Branches[0].Body)));
        Assert.Equal("/y", Line(Assert.Single(ifn.Branches[1].Body)));
    }

    [Fact]
    public void IfElseIfElse()
    {
        var (nodes, diags) = Parse("if (a) {", "/a", "}", "else if (b) {", "/b", "}", "else {", "/c", "}");
        Assert.Empty(diags);
        var ifn = Assert.IsType<IfNode>(Assert.Single(nodes));
        Assert.Equal(3, ifn.Branches.Count);
        Assert.Equal("a", ifn.Branches[0].Condition);
        Assert.Equal("b", ifn.Branches[1].Condition);
        Assert.Null(ifn.Branches[2].Condition);
    }

    [Fact]
    public void NestedIf()
    {
        var (nodes, diags) = Parse("if (a) {", "/x", "if (b) {", "/y", "}", "}");
        Assert.Empty(diags);
        var outer = Assert.IsType<IfNode>(Assert.Single(nodes));
        var body = outer.Branches[0].Body;
        Assert.Equal(2, body.Count);
        Assert.Equal("/x", Line(body[0]));
        var inner = Assert.IsType<IfNode>(body[1]);
        Assert.Equal("b", inner.Branches[0].Condition);
        Assert.Equal("/y", Line(Assert.Single(inner.Branches[0].Body)));
    }

    [Fact]
    public void LeafLinesInOrder()
    {
        var (nodes, diags) = Parse("/a", ":wait 2", "hello there", ":set x 1");
        Assert.Empty(diags);
        Assert.Collection(nodes,
            n => Assert.Equal("/a", Line(n)),
            n => Assert.Equal(":wait 2", Line(n)),
            n => Assert.Equal("hello there", Line(n)),
            n => Assert.Equal(":set x 1", Line(n)));
    }

    [Fact]
    public void BlanksAndCommentsSkipped()
    {
        var (nodes, diags) = Parse("if (a) {", "", "# note", "/x", "}");
        Assert.Empty(diags);
        var ifn = Assert.IsType<IfNode>(Assert.Single(nodes));
        Assert.Equal("/x", Line(Assert.Single(ifn.Branches[0].Body)));
    }

    [Fact]
    public void IndentationTolerated()
    {
        var (nodes, diags) = Parse("if (a) {", "    /x", "}");
        Assert.Empty(diags);
        var ifn = Assert.IsType<IfNode>(Assert.Single(nodes));
        Assert.Equal("/x", Line(Assert.Single(ifn.Branches[0].Body)));
    }

    [Fact]
    public void MissingBraceIsPlainLine()
    {
        var (nodes, diags) = Parse("if (a) matters");
        Assert.Empty(diags);
        Assert.Equal("if (a) matters", Line(Assert.Single(nodes)));
    }

    [Fact]
    public void StrayClose()
    {
        var (_, diags) = Parse("/x", "}");
        Assert.Equal(1, Assert.Single(diags).Line);
    }

    [Fact]
    public void OrphanElse()
    {
        var (_, diags) = Parse("else {", "}");
        Assert.NotEmpty(diags);
        Assert.Equal(0, diags[0].Line);
    }

    [Fact]
    public void OrphanElseIf()
    {
        var (_, diags) = Parse("else if (a) {", "}");
        Assert.NotEmpty(diags);
    }

    [Fact]
    public void UnclosedPointsAtOpener()
    {
        var (_, diags) = Parse("if (a) {", "/x");
        var d = Assert.Single(diags);
        Assert.Equal(0, d.Line);
        Assert.Contains("Missing", d.Message);
    }

    [Fact]
    public void InvalidCondition()
    {
        var (_, diags) = Parse("if (a <) {", "}");
        var d = Assert.Single(diags);
        Assert.Equal(0, d.Line);
        Assert.Contains("Invalid condition", d.Message);
    }
}
