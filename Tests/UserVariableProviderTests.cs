using Silkstring.Models;
using Silkstring.Services;
using Silkstring.Services.Variables;

public class UserVariableProviderTests
{
    private static List<UserVariable> Vars(params (string Name, string Value)[] vars)
        => vars.Select(v => new UserVariable { Name = v.Name, Value = v.Value }).ToList();

    [Fact]
    public void ResolvesStoredValue()
    {
        var descriptor = new UserVariableProvider(() => Vars(("greeting", "hello"))).GetVariables().Single();
        Assert.Equal("greeting", descriptor.Name);
        Assert.Equal("User", descriptor.Category);
        Assert.Equal("hello", descriptor.Resolve());
    }

    [Fact]
    public void YieldsOneDescriptorPerVariable()
        => Assert.Equal(new[] { "a", "b" }, new UserVariableProvider(() => Vars(("a", "1"), ("b", "2"))).GetVariables().Select(v => v.Name));

    [Fact]
    public void EmptyWhenNoVariables()
        => Assert.Empty(new UserVariableProvider(() => Vars()).GetVariables());

    [Fact]
    public void ReflectsValueChangedAfterResolveCaptured()
    {
        var vars = Vars(("mode", "raid"));
        var resolve = new UserVariableProvider(() => vars).GetVariables().Single().Resolve;
        vars[0].Value = "solo";
        Assert.Equal("solo", resolve());
    }

    [Fact]
    public void ResolvesThroughCommandResolver()
    {
        var resolver = new CommandResolver(new[] { new UserVariableProvider(() => Vars(("name", "Oreo"))) });
        Assert.Equal("hi Oreo", resolver.Resolve("hi {name}"));
    }

    [Fact]
    public void ResolverIsCaseInsensitive()
    {
        var resolver = new CommandResolver(new[] { new UserVariableProvider(() => Vars(("name", "Oreo"))) });
        Assert.Equal("Oreo", resolver.Resolve("{NAME}"));
    }

    [Fact]
    public void ResolverReflectsLaterValueChange()
    {
        var vars = Vars(("name", "Oreo"));
        var resolver = new CommandResolver(new[] { new UserVariableProvider(() => vars) });
        vars[0].Value = "Biscuit";
        Assert.Equal("Biscuit", resolver.Resolve("{name}"));
    }
}
