using System.Collections.Generic;

namespace Silkstring.Services.Conditions;

internal abstract record BlockNode;
internal sealed record LineNode(string Text) : BlockNode;
internal sealed record IfNode(IReadOnlyList<Branch> Branches) : BlockNode;
internal sealed record Branch(string? Condition, IReadOnlyList<BlockNode> Body);
