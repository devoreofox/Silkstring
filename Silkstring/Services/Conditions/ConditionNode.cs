internal abstract record ConditionNode;

internal sealed record OrNode(ConditionNode L, ConditionNode R) : ConditionNode;
internal sealed record AndNode(ConditionNode L, ConditionNode R) : ConditionNode;
internal sealed record CmpNode(string L, string Op, string R) : ConditionNode;
internal sealed record BareNode(string Value) : ConditionNode;
