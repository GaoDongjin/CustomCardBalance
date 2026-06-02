using System.Diagnostics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace CustomCardBalance;

[ModInitializer(nameof(Initialize))]
public static class Plugin
{
    public static void Initialize()
    {
        var stopwatch = Stopwatch.StartNew();
        ModConfiguration.Load();
        var harmony = new Harmony("com.bruiser.customcardbalance");
        harmony.PatchAll(typeof(Plugin).Assembly);
        Log.Info($"[CustomCardBalance] Initialization completed in {stopwatch.ElapsedMilliseconds}ms.");
    }
}
