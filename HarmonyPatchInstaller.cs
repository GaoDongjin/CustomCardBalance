using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace CustomCardBalance;

public static class HarmonyPatchInstaller
{
    public static void Install(Harmony harmony)
    {
        PatchPostfix(harmony, typeof(Untouchable), "get_CanonicalVars", typeof(UntouchableVarsPatch), nameof(UntouchableVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Untouchable), "OnUpgrade", typeof(UntouchableUpgradePatch), nameof(UntouchableUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(CardModel), "get_Rarity", typeof(AcrobaticsRarityPatch), nameof(AcrobaticsRarityPatch.Postfix));
        PatchPrefix(harmony, typeof(Speedster), "OnUpgrade", typeof(SpeedsterUpgradePatch), nameof(SpeedsterUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Anticipate), "get_CanonicalVars", typeof(AnticipateVarsPatch), nameof(AnticipateVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Anticipate), "OnUpgrade", typeof(AnticipateUpgradePatch), nameof(AnticipateUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(CardModel), "get_CanonicalEnergyCost", typeof(LegacyCostPatch), nameof(LegacyCostPatch.Postfix));
        PatchPostfix(harmony, typeof(Alignment), "get_CanonicalStarCost", typeof(AlignmentStarCostPatch), nameof(AlignmentStarCostPatch.Postfix));
        PatchPrefix(harmony, typeof(LocTable), nameof(LocTable.GetRawText), typeof(CardDescriptionPatch), nameof(CardDescriptionPatch.Prefix));
        PatchPrefix(harmony, typeof(Dominate), "OnUpgrade", typeof(DominateUpgradePatch), nameof(DominateUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(ExpectAFight), "OnPlay", typeof(ExpectAFightPlayPatch), nameof(ExpectAFightPlayPatch.Prefix));
        PatchPostfix(harmony, typeof(ForgottenRitual), "get_CanonicalKeywords", typeof(ForgottenRitualKeywordsPatch), nameof(ForgottenRitualKeywordsPatch.Postfix));
        PatchPostfix(harmony, typeof(ForgottenRitual), "get_ExtraHoverTips", typeof(ForgottenRitualHoverTipsPatch), nameof(ForgottenRitualHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(Murder), "OnUpgrade", typeof(MurderUpgradePatch), nameof(MurderUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Glow), "get_CanonicalVars", typeof(GlowVarsPatch), nameof(GlowVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Glow), "OnPlay", typeof(GlowPlayPatch), nameof(GlowPlayPatch.Prefix));
        PatchPostfix(harmony, typeof(VoidForm), "get_CanonicalKeywords", typeof(VoidFormKeywordsPatch), nameof(VoidFormKeywordsPatch.Postfix));
        PatchPrefix(harmony, typeof(VoidForm), "OnUpgrade", typeof(VoidFormUpgradePatch), nameof(VoidFormUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(TheSealedThrone), "OnUpgrade", typeof(TheSealedThroneUpgradePatch), nameof(TheSealedThroneUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(BorrowedTime), "get_CanonicalVars", typeof(BorrowedTimeVarsPatch), nameof(BorrowedTimeVarsPatch.Postfix));
        PatchPostfix(harmony, typeof(BorrowedTime), "get_ExtraHoverTips", typeof(BorrowedTimeHoverTipsPatch), nameof(BorrowedTimeHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(BorrowedTime), "OnPlay", typeof(BorrowedTimePlayPatch), nameof(BorrowedTimePlayPatch.Prefix));
        PatchPrefix(harmony, typeof(BorrowedTime), "OnUpgrade", typeof(BorrowedTimeUpgradePatch), nameof(BorrowedTimeUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(Production), "OnUpgrade", typeof(ProductionUpgradePatch), nameof(ProductionUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(BansheesCry), "OnUpgrade", typeof(BansheesCryUpgradePatch), nameof(BansheesCryUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(WraithForm), "get_CanonicalVars", typeof(WraithFormVarsPatch), nameof(WraithFormVarsPatch.Postfix));
        PatchPostfix(harmony, typeof(WraithForm), "get_ExtraHoverTips", typeof(WraithFormHoverTipsPatch), nameof(WraithFormHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(WraithForm), "OnPlay", typeof(WraithFormPlayPatch), nameof(WraithFormPlayPatch.Prefix));
        PatchPrefix(harmony, typeof(WraithForm), "OnUpgrade", typeof(WraithFormUpgradePatch), nameof(WraithFormUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Dirge), "get_CanonicalKeywords", typeof(DirgeKeywordsPatch), nameof(DirgeKeywordsPatch.Postfix));
        PatchPrefix(harmony, typeof(Seance), "OnUpgrade", typeof(SeanceUpgradePatch), nameof(SeanceUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Seance), "get_ExtraHoverTips", typeof(SeanceHoverTipsPatch), nameof(SeanceHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(Seance), "OnPlay", typeof(SeancePlayPatch), nameof(SeancePlayPatch.Prefix));
        PatchPrefix(harmony, typeof(Defy), "OnUpgrade", typeof(DefyUpgradePatch), nameof(DefyUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(NMainMenu), nameof(NMainMenu._Ready), typeof(MainMenuSettingsPanelPatch), nameof(MainMenuSettingsPanelPatch.Postfix));
        PatchPrefix(harmony, typeof(NGame), nameof(NGame._Input), typeof(MainMenuSettingsHotkeyPatch), nameof(MainMenuSettingsHotkeyPatch.Prefix));
        PatchPostfix(harmony, typeof(NSettingsScreen), nameof(NSettingsScreen._Ready), typeof(SettingsScreenSettingsPanelPatch), nameof(SettingsScreenSettingsPanelPatch.Postfix));
    }

    private static void PatchPrefix(Harmony harmony, Type targetType, string targetMethodName, Type patchType, string patchMethodName)
    {
        harmony.Patch(
            RequiredTargetMethod(targetType, targetMethodName),
            prefix: RequiredHarmonyMethod(patchType, patchMethodName));
    }

    private static void PatchPostfix(Harmony harmony, Type targetType, string targetMethodName, Type patchType, string patchMethodName)
    {
        harmony.Patch(
            RequiredTargetMethod(targetType, targetMethodName),
            postfix: RequiredHarmonyMethod(patchType, patchMethodName));
    }

    private static MethodBase RequiredTargetMethod(Type targetType, string methodName)
    {
        return AccessTools.Method(targetType, methodName)
            ?? throw new MissingMethodException(targetType.FullName, methodName);
    }

    private static HarmonyMethod RequiredHarmonyMethod(Type patchType, string methodName)
    {
        MethodInfo method = AccessTools.Method(patchType, methodName)
            ?? throw new MissingMethodException(patchType.FullName, methodName);
        return new HarmonyMethod(method);
    }
}
