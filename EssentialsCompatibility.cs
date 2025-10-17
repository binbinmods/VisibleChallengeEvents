using System.Runtime.CompilerServices;
using BepInEx;
using static Obeliskial_Essentials.Essentials;
using static VisibleChallengeEvents.Plugin;
public static class EssentialsCompatibility
{
    private static bool? _enabled;

    public static bool Enabled
    {
        get
        {
            _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.stiffmeds.obeliskialessentials");
            return (bool)_enabled;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void EssentialsRegister()
    {
        // Register with Obeliskial Essentials
        // EssentialsInstalled = Chainloader.PluginInfos.ContainsKey("com.stiffmeds.obeliskialessentials");

        RegisterMod(
            _name: PluginName,
            _author: "binbin",
            _description: "Visible Challenge Events",
            _version: PluginVersion,
            _date: ModDate,
            _link: @"https://github.com/binbinmods/VisibleChallengeEvents"
        );
        LogInfo($"{PluginGUID} {PluginVersion} has loaded with Essentials!");


    }
}