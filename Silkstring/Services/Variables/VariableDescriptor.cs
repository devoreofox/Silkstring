using System;

namespace Silkstring.Services.Variables;

public sealed record VariableDescriptor(
    string Name,
    string Description,
    string Category,
    Func<string?> Resolve
    );
