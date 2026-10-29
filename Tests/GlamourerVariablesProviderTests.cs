using Newtonsoft.Json.Linq;
using Silkstring.Services.Variables.Providers;

public class GlamourerVariablesProviderTests
{
    private static readonly JObject State = JObject.Parse("""
        {
          "Equipment": {
            "MainHand": { "ItemId": 51012 },
            "Neck": { "ItemId": 38567 },
            "RFinger": { "ItemId": 9295 },
            "LFinger": { "ItemId": 8575 },
            "Head": { "ItemId": 4294967164 },
            "Feet": { "ItemId": 0 }
          },
          "Bonus": {
            "Glasses": { "BonusId": 844424946909184 }
          }
        }
        """);

    [Fact]
    public void ReadsEquipmentSlotItemId()
    {
        Assert.Equal(51012u, GlamourerVariablesProvider.ExtractItemId(State, "Equipment", "MainHand", "ItemId"));
        Assert.Equal(38567u, GlamourerVariablesProvider.ExtractItemId(State, "Equipment", "Neck", "ItemId"));
        Assert.Equal(9295u, GlamourerVariablesProvider.ExtractItemId(State, "Equipment", "RFinger", "ItemId"));
        Assert.Equal(8575u, GlamourerVariablesProvider.ExtractItemId(State, "Equipment", "LFinger", "ItemId"));
    }

    [Fact]
    public void EmptyOrMissingSlotIsNull()
    {
        Assert.Null(GlamourerVariablesProvider.ExtractItemId(State, "Equipment", "Feet", "ItemId"));
        Assert.Null(GlamourerVariablesProvider.ExtractItemId(State, "Equipment", "Body", "ItemId"));
    }

    [Fact]
    public void OversizedBonusSentinelIsNull()
        => Assert.Null(GlamourerVariablesProvider.ExtractItemId(State, "Bonus", "Glasses", "BonusId"));
}
