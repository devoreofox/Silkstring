using System;

namespace Silkstring.Services.Conditions;

public sealed class ConditionException : Exception
{
    public ConditionException(string message) : base(message) { }
}
