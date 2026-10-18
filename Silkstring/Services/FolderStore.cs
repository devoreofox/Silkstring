using System;
using System.Collections.Generic;
using System.Linq;
using Silkstring.Models;

namespace Silkstring.Services;

public class FolderStore<T> where T : class
{
    private readonly List<T> _items;
    private readonly List<Folder<T>> _folders;
    private readonly Action _onChanged;

    public IEnumerable<T> All => _items.Concat(_folders.SelectMany(f => f.Items));
    public IReadOnlyList<T> TopLevel => _items;
    public IReadOnlyList<Folder<T>> Folders => _folders;

    public FolderStore(List<T> items, List<Folder<T>> folders, Action onChanged)
    {
        _items = items;
        _folders = folders;
        _onChanged = onChanged;
    }

    public Folder<T>? FolderOf(T item) => _folders.FirstOrDefault(f => f.Items.Contains(item));

    public void MarkChanged() => _onChanged();

    public void Add(T item, Folder<T>? folder = null)
    {
        var items = folder?.Items ?? _items;
        items.Add(item);
        _onChanged();
    }

    public void Remove(T item)
    {
        if (!_items.Remove(item))
        {
            foreach (var f in _folders)
            {
                if (f.Items.Remove(item)) break;
            }
        }
        _onChanged();
    }

    public Folder<T> AddFolder()
    {
        var folder = new Folder<T>();
        _folders.Add(folder);
        _onChanged();
        return folder;
    }

    public void RemoveFolder(Folder<T> folder)
    {
        _items.AddRange(folder.Items);
        _folders.Remove(folder);
        _onChanged();
    }

    public void MoveTo(T item, Folder<T>? target)
    {
        _items.Remove(item);
        foreach (var f in _folders) f.Items.Remove(item);
        if (target != null) target.Items.Add(item);
        else _items.Add(item);
        _onChanged();
    }
}
