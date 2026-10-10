using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using System.Reflection;

namespace NhxDayLength
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    // Each game works out the time of day for itself: the server when everyone sleeps, and each player for the monsters
    // that spawn around them by day or by night. So everyone needs the mod, with the server's settings
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "NhxDayLength";
        private const string PluginName = "NhxDayLength";
        private const string PluginVersion = "1.0.0";

        // The game's: a 30 minute cycle, 70% of it daylight. The mod doubles it by default.
        private const int DefaultDayMinutes = 42;
        private const int DefaultNightMinutes = 18;

        internal static Plugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        private ConfigEntry<int> _dayLength;
        private ConfigEntry<int> _nightLength;
        private Harmony _harmonyInstance;

        internal static int DayMinutes => Instance?._dayLength != null ? Instance._dayLength.Value : DefaultDayMinutes;
        internal static int NightMinutes => Instance?._nightLength != null ? Instance._nightLength.Value : DefaultNightMinutes;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            // Admin only: Jotunn syncs the server's values to every player
            _dayLength = Config.Bind(
                "General",
                "Day Length",
                DefaultDayMinutes,
                new ConfigDescription(
                    "Minutes of daylight, from sunrise to sunset. The game's is 21. The server's setting applies to every player.",
                    new AcceptableValueRange<int>(1, 240),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            _nightLength = Config.Bind(
                "General",
                "Night Length",
                DefaultNightMinutes,
                new ConfigDescription(
                    "Minutes of night, from sunset to sunrise. The game's is 9. The server's setting applies to every player.",
                    new AcceptableValueRange<int>(1, 240),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            // Raised for the player's own changes, and also when Jotunn applies the server's values on joining and puts
            // the player's own back on leaving
            _dayLength.SettingChanged += (sender, args) => DayCycle.Apply();
            _nightLength.SettingChanged += (sender, args) => DayCycle.Apply();

            DayCycle.Apply();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll(assembly);

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void OnDestroy()
        {
            _harmonyInstance?.UnpatchSelf();

            Log?.LogInfo($"{PluginName} v{PluginVersion} unloaded.");

            Instance = null;
        }
    }
}
