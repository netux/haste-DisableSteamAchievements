using Landfall.Haste;
using Landfall.Modding;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Settings;
using Steamworks;
using Zorro.Core;

namespace HasteDisableSteamAchievements;

[LandfallPlugin]
public class DisableSteamAchievementsMod
{
    public static bool AreAchievementsEnabled { get => GameHandler.Instance.SettingsHandler.GetSetting<SteamAchievementsSetting>().Value == OffOnMode.ON; }

    public static GameObject? escapeMenuWarningGameObject = null;

    static DisableSteamAchievementsMod()
    {
        On.Steamworks.SteamUserStats.SetAchievement += static (original, steamAchievementName) =>
        {
            if (!AreAchievementsEnabled)
            {
                var setting = GameHandler.Instance.SettingsHandler.GetSetting<SteamAchievementsSetting>();

                Debug.LogWarning(string.Join("\n", [
                    $"Would have gotten achievement {SteamUserStats.GetAchievementDisplayAttribute(steamAchievementName, "name")}, but Steam Achievements are disabled.",
                    $"To enable them, go to Settings > {new LocalizedString("UI", "button_general").GetLocalizedString()} and enable {setting.GetDisplayName().GetLocalizedString()}"
                ]));
                return false;
            }

            return original(steamAchievementName);
        };

        On.MainMenu.Start += static (original, mainMenu) =>
        {
            original(mainMenu);

            MaybeCreateWarningGameObject(mainMenu.transform);
        };

        On.EscapeMenuMainPage.Start += static (original, escapeMenuMainPage) =>
        {
            original(escapeMenuMainPage);

            MaybeCreateWarningGameObject(escapeMenuMainPage.transform);
        };
    }

    private static void MaybeCreateWarningGameObject(Transform menuTransform)
    {
        if (escapeMenuWarningGameObject == null)
        {
            var parentTransform = menuTransform.Find("ExtraButtons");

            escapeMenuWarningGameObject = new GameObject("SteamAchievementsWarning", [typeof(RectTransform)]);
            escapeMenuWarningGameObject.transform.SetParent(parentTransform, worldPositionStays: false);

            var text = escapeMenuWarningGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "Steam Achievements disabled!";
            text.fontSize = 20;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.color = Color.red;
        }

        UpdateWarningVisibility();
    }

    internal static void UpdateWarningVisibility()
    {
        escapeMenuWarningGameObject?.SetActive(!AreAchievementsEnabled);
    }
}

[HasteSetting]
public class SteamAchievementsSetting : OffOnSetting, IExposedSetting
{
    public override void ApplyValue() => DisableSteamAchievementsMod.UpdateWarningVisibility();

    protected override OffOnMode GetDefaultValue() => OffOnMode.OFF;

    public override List<LocalizedString> GetLocalizedChoices() => [
        // yoink
        new("Settings", "DisabledGraphicOption"),
        new("Settings", "EnabledGraphicOption")
    ];

    public LocalizedString GetDisplayName() => new UnlocalizedString("Steam Achievements");

    public string GetCategory() => SettingCategory.General;
}
