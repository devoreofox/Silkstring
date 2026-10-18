using Silkstring.Models;
using Silkstring.Services;

public class FolderStoreTests
{
    private static FolderStore<UserVariable> Store(List<UserVariable> items)
        => new(items, new List<Folder<UserVariable>>(), () => { });

    [Fact]
    public void MovesBetweenTopLevelAndFolder()
    {
        var a = new UserVariable { Name = "a" };
        var items = new List<UserVariable> { a };
        var store = Store(items);
        var folder = store.AddFolder();
        store.MoveTo(a, folder);
        Assert.Empty(items);
        Assert.Same(folder, store.FolderOf(a));
        store.MoveTo(a, null);
        Assert.Same(a, Assert.Single(items));
        Assert.Null(store.FolderOf(a));
    }

    [Fact]
    public void RemovingFolderKeepsItsItems()
    {
        var a = new UserVariable { Name = "a" };
        var items = new List<UserVariable>();
        var store = Store(items);
        var folder = store.AddFolder();
        store.Add(a, folder);
        store.RemoveFolder(folder);
        Assert.Empty(store.Folders);
        Assert.Same(a, Assert.Single(items));
    }

    [Fact]
    public void RemoveFindsItemInsideFolder()
    {
        var a = new UserVariable { Name = "a" };
        var store = Store(new List<UserVariable>());
        store.Add(a, store.AddFolder());
        store.Remove(a);
        Assert.Empty(store.All);
    }
}
