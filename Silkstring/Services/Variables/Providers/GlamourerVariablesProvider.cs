using System.Collections.Generic;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Glamourer.Api.IpcSubscribers;
using Lumina.Excel.Sheets;
using Newtonsoft.Json.Linq;

namespace Silkstring.Services.Variables.Providers;

public sealed class GlamourerVariablesProvider : IVariableProvider
{
    private readonly GetState _getState;
    private readonly ApiVersion _apiVersion;
    private readonly IDataManager _dataManager;

    public GlamourerVariablesProvider(IDalamudPluginInterface pi, IDataManager dataManager)
    {
        _getState = new GetState(pi);
        _apiVersion = new ApiVersion(pi);
        _dataManager = dataManager;
    }

    public IEnumerable<VariableDescriptor> GetVariables()
    {
        yield return new("glamourer", "Whether Glamourer is installed and available", "Glamourer", IsAvailable);
        yield return new("weapondrawn", "Whether your weapon is drawn", "Glamourer", () => ReadToggle("Weapon", "Show"));
        yield return new("hatvisible", "Whether your headgear is shown", "Glamourer", () => ReadToggle("Hat", "Show"));
        yield return new("visordown", "Whether your visor is toggled down", "Glamourer", () => ReadToggle("Visor", "IsToggled"));

        foreach (var (name, section, key, id) in Slots)
        {
            var s = section;
            var k = key;
            var i = id;
            yield return new(name, $"Item Glamourer has applied in your {Label(name)} slot", "Glamourer", () => Resolve(s, k, i));
        }
    }

    private string IsAvailable()
    {
        try { return _apiVersion.Invoke().Major > 0 ? "true" : "false"; }
        catch { return "false"; }
    }

    private string? ReadToggle(string key, string field)
    {
        try
        {
            var (_, state) = _getState.Invoke(0);
            var token = state?["Equipment"]?[key]?[field];
            return token is null ? null : token.Value<bool>() ? "true" : "false";
        }
        catch { return null; }
    }

    private static readonly (string Name, string Section, string Key, string IdKey)[] Slots =
    [
        ("head", "Equipment", "Head", "ItemId"),
        ("body", "Equipment", "Body", "ItemId"),
        ("hands", "Equipment", "Hands", "ItemId"),
        ("legs", "Equipment", "Legs", "ItemId"),
        ("feet", "Equipment", "Feet", "ItemId"),
        ("ears", "Equipment", "Ears", "ItemId"),
        ("neck", "Equipment", "Neck", "ItemId"),
        ("wrists", "Equipment", "Wrists", "ItemId"),
        ("rightring", "Equipment", "RFinger", "ItemId"),
        ("leftring", "Equipment", "LFinger", "ItemId"),
        ("mainhand", "Equipment", "MainHand", "ItemId"),
        ("offhand", "Equipment", "OffHand", "ItemId"),
        ("facewear", "Bonus", "Glasses", "BonusId"),
    ];

    private string? Resolve(string section, string key, string idKey)
    {
        try
        {
            var (_, state) = _getState.Invoke(0);
            if (state?[section]?[key]?[idKey] is null) return null;
            var itemId = ExtractItemId(state, section, key, idKey);
            if (itemId is not null)
            {
                var name = section == "Bonus"
                    ? _dataManager.GetExcelSheet<Glasses>().GetRowOrDefault(itemId.Value)?.Name.ToString()
                    : _dataManager.GetExcelSheet<Item>().GetRowOrDefault(itemId.Value)?.Name.ToString();
                if (!string.IsNullOrEmpty(name)) return name;
            }
            return "Nothing";
        }
        catch { return null; }
    }

    internal static uint? ExtractItemId(JObject state, string section, string key, string idKey)
    {
        var itemId = state[section]?[key]?[idKey]?.Value<ulong>();
        if (itemId is null or 0 || itemId > uint.MaxValue) return null;
        return (uint)itemId;
    }

    private static string Label(string name) => name switch
    {
        "rightring" => "right ring",
        "leftring" => "left ring",
        "mainhand" => "main hand",
        "offhand" => "off hand",
        _ => name,
    };
}
