using Silkstring.Services;
using Silkstring.Services.Variables;

public class CommandResolverTests
{
    private sealed class FakeProvider : IVariableProvider
    {
        public IEnumerable<VariableDescriptor> GetVariables()
        {
            yield return new("job", "", "", () => "WHM");
            yield return new("hpp", "", "", () => "40");
            yield return new("none", "", "", () => null);
        }
    }

    private static CommandResolver Resolver() => new(new[] { new FakeProvider() });

    [Theory]
    [InlineData("{job}", "", "WHM")]
    [InlineData("{JOB}", "", "WHM")]
    [InlineData("{none}", "", "{none}")]
    [InlineData("{bogus}", "", "{bogus}")]
    [InlineData("hi {job}!", "", "hi WHM!")]
    [InlineData("{0}", "a b", "a")]
    [InlineData("{1}", "a b", "b")]
    [InlineData("{2}", "a b", "{2}")]
    [InlineData("{*}", "a b c", "a b c")]
    [InlineData("{1..}", "a b c", "b c")]
    [InlineData("{..2}", "a b c", "a b")]
    [InlineData("{0..2}", "a b c", "a b")]
    [InlineData("{5..}", "a", "")]
    [InlineData("{0} {job}", "x", "x WHM")]
    public void Resolves(string command, string args, string expected)
        => Assert.Equal(expected, Resolver().Resolve(command, args.Split(' ', StringSplitOptions.RemoveEmptyEntries)));
}
