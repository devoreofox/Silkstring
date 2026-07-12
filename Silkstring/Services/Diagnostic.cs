namespace Silkstring.Services;

public enum Severity { Error, Warning }

public readonly record struct Diagnostic(string Message, int? Line = null, Severity Severity = Severity.Error);
