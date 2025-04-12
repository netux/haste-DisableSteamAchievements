using Landfall.Haste;
using Landfall.Modding;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Settings;
using Steamworks;

namespace HasteDisableSteamAchievements;

[LandfallPlugin]
public class DisableSteamAchievementsMod
{
    static DisableSteamAchievementsMod()
    {
        On.Steamworks.SteamUserStats.SetAchievement += (original, steamAchievementName) =>
        {
            var achievementsEnabledSetting = GameHandler.Instance.SettingsHandler.GetSetting<SteamAchievementsSetting>();
            var achievementsEnabled = achievementsEnabledSetting.Value == OffOnMode.ON;
            
            if (!achievementsEnabled)
            {
                Debug.LogWarning(string.Join("\n", [
                    $"Would have gotten achievement {SteamUserStats.GetAchievementDisplayAttribute(steamAchievementName, "name")}, but Steam Achievements are disabled.",
                    $"To enable them, go to Settings > {new LocalizedString("UI", "button_general").GetLocalizedString()} and enable {achievementsEnabledSetting.GetDisplayName().GetLocalizedString()}"
                ]));
                return false;
            }

            return original(steamAchievementName);
        };
    }
}

[HasteSetting]
public class SteamAchievementsSetting : OffOnSetting, IExposedSetting
{
    public override void ApplyValue() { /* no op */ }
    protected override OffOnMode GetDefaultValue() => OffOnMode.OFF;
    public override List<LocalizedString> GetLocalizedChoices()
    {
        return [
            // yoink
            new("Settings", "DisabledGraphicOption"),
            new("Settings", "EnabledGraphicOption")
        ];
    }
    public LocalizedString GetDisplayName() => new UnlocalizedString("Steam Achievements");
    public string GetCategory() => SettingCategory.General;
}
