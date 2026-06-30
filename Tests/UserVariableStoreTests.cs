using Silkstring.Models;
using Silkstring.Services.Variables;

public class UserVariableStoreTests
{
    private static List<UserVariable> Vars(params (string Name, string Value)[] vars)
        => vars.Select(v => new UserVariable { Name = v.Name, Value = v.Value }).ToList();

    private static UserVariableStore Store(List<UserVariable> vars, Action onChanged, params string[] reserved)
        => new(vars, new HashSet<string>(reserved, StringComparer.OrdinalIgnoreCase), onChanged);

    [Fact]
    public void SetsKnownVariable()
    {
        var vars = Vars(("mode", "raid"));
        var changed = false;
        var store = Store(vars, () => changed = true);
        Assert.True(store.TrySet("mode", "solo"));
        Assert.Equal("solo", vars[0].Value);
        Assert.True(changed);
    }

    [Fact]
    public void MatchesNameCaseInsensitively()
    {
        var vars = Vars(("Mode", "raid"));
        var store = Store(vars, () => { });
        Assert.True(store.TrySet("mode", "solo"));
        Assert.Equal("solo", vars[0].Value);
    }

    [Fact]
    public void UnknownNameDoesNothing()
    {
        var vars = Vars(("mode", "raid"));
        var changed = false;
        var store = Store(vars, () => changed = true);
        Assert.False(store.TrySet("other", "x"));
        Assert.Equal("raid", vars[0].Value);
        Assert.False(changed);
    }

    [Fact]
    public void BuiltInNameIsNotSettable()
    {
        var store = Store(Vars(("mode", "raid")), () => { });
        Assert.False(store.TrySet("job", "WHM"));
    }

    [Fact]
    public void AddsValidName()
    {
        var vars = Vars();
        var changed = false;
        var store = Store(vars, () => changed = true);
        Assert.True(store.TryAdd("greeting"));
        Assert.Equal("greeting", Assert.Single(vars).Name);
        Assert.True(changed);
    }

    [Fact]
    public void DoesNotAddInvalidName()
    {
        var vars = Vars();
        var changed = false;
        var store = Store(vars, () => changed = true);
        Assert.False(store.TryAdd("has space"));
        Assert.Empty(vars);
        Assert.False(changed);
    }

    [Fact]
    public void Removes()
    {
        var vars = Vars(("a", "1"), ("b", "2"));
        var changed = false;
        var store = Store(vars, () => changed = true);
        store.Remove(vars[0]);
        Assert.Equal("b", Assert.Single(vars).Name);
        Assert.True(changed);
    }

    [Theory]
    [InlineData("greeting", null)]
    [InlineData("greet_2", null)]
    [InlineData("", "Enter a name")]
    [InlineData("   ", "Enter a name")]
    [InlineData("has space", "Use only letters, numbers, and underscores")]
    [InlineData("no-dash", "Use only letters, numbers, and underscores")]
    public void ValidatesNameFormat(string name, string? expected)
        => Assert.Equal(expected, Store(Vars(), () => { }).ValidateName(name));

    [Fact]
    public void RejectsReservedName()
        => Assert.Equal("JOB is a built-in variable", Store(Vars(), () => { }, "job").ValidateName("JOB"));

    [Fact]
    public void RejectsDuplicateName()
        => Assert.Equal("greeting is already defined", Store(Vars(("Greeting", "")), () => { }).ValidateName("greeting"));

    [Fact]
    public void ExcludedRowIsNotADuplicate()
    {
        var vars = Vars(("greeting", ""));
        var store = Store(vars, () => { });
        Assert.Null(store.ValidateName("greeting", vars[0]));
    }
}
