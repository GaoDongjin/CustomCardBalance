using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace CustomCardBalance;

public static class HarmonyPatchInstaller
{
    public static void Install(Harmony harmony)
    {
        PatchPostfix(harmony, typeof(ForgottenRitual), "get_CanonicalVars", typeof(ForgottenRitualVarsPatch), nameof(ForgottenRitualVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(ForgottenRitual), "OnUpgrade", typeof(ForgottenRitualUpgradePatch), nameof(ForgottenRitualUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(ForgottenRitual), "get_ExtraHoverTips", typeof(ForgottenRitualHoverTipsPatch), nameof(ForgottenRitualHoverTipsPatch.Postfix));
        PatchPostfix(harmony, typeof(Spite), "get_CanonicalVars", typeof(SpiteVarsPatch), nameof(SpiteVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Spite), "OnPlay", typeof(SpitePlayPatch), nameof(SpitePlayPatch.Prefix));
        PatchPrefix(harmony, typeof(Spite), "OnUpgrade", typeof(SpiteUpgradePatch), nameof(SpiteUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(CardModel), "get_Rarity", typeof(AcrobaticsRarityPatch), nameof(AcrobaticsRarityPatch.Postfix));
        PatchPostfix(harmony, typeof(Untouchable), "get_CanonicalVars", typeof(UntouchableVarsPatch), nameof(UntouchableVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Untouchable), "OnUpgrade", typeof(UntouchableUpgradePatch), nameof(UntouchableUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Anticipate), "get_CanonicalVars", typeof(AnticipateVarsPatch), nameof(AnticipateVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Anticipate), "OnUpgrade", typeof(AnticipateUpgradePatch), nameof(AnticipateUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(Speedster), "OnUpgrade", typeof(SpeedsterUpgradePatch), nameof(SpeedsterUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(WraithForm), "get_CanonicalVars", typeof(WraithFormVarsPatch), nameof(WraithFormVarsPatch.Postfix));
        PatchPostfix(harmony, typeof(WraithForm), "get_ExtraHoverTips", typeof(WraithFormHoverTipsPatch), nameof(WraithFormHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(WraithForm), "OnPlay", typeof(WraithFormPlayPatch), nameof(WraithFormPlayPatch.Prefix));
        PatchPrefix(harmony, typeof(WraithForm), "OnUpgrade", typeof(WraithFormUpgradePatch), nameof(WraithFormUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(Voltaic), "OnUpgrade", typeof(VoltaicUpgradePatch), nameof(VoltaicUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(Hotfix), "OnUpgrade", typeof(HotfixUpgradePatch), nameof(HotfixUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Defragment), "get_CanonicalVars", typeof(DefragmentVarsPatch), nameof(DefragmentVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Defragment), "OnUpgrade", typeof(DefragmentUpgradePatch), nameof(DefragmentUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Coolant), "get_CanonicalVars", typeof(CoolantVarsPatch), nameof(CoolantVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Coolant), "OnUpgrade", typeof(CoolantUpgradePatch), nameof(CoolantUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(BiasedCognition), "get_CanonicalVars", typeof(BiasedCognitionVarsPatch), nameof(BiasedCognitionVarsPatch.Postfix));
        PatchPostfix(harmony, typeof(BiasedCognition), "get_ExtraHoverTips", typeof(BiasedCognitionHoverTipsPatch), nameof(BiasedCognitionHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(BiasedCognition), "OnPlay", typeof(BiasedCognitionPlayPatch), nameof(BiasedCognitionPlayPatch.Prefix));
        PatchPrefix(harmony, typeof(BiasedCognition), "OnUpgrade", typeof(BiasedCognitionUpgradePatch), nameof(BiasedCognitionUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(BiasedCognitionPower), nameof(BiasedCognitionPower.AfterSideTurnStart), typeof(BiasedCognitionPowerTurnStartPatch), nameof(BiasedCognitionPowerTurnStartPatch.Prefix));
        PatchPrefix(harmony, typeof(BiasedCognitionPower), "get_StackType", typeof(BiasedCognitionPowerStackTypePatch), nameof(BiasedCognitionPowerStackTypePatch.Prefix));
        PatchPrefix(harmony, typeof(AbstractModel), nameof(AbstractModel.AfterModifyingPowerAmountReceived), typeof(BiasedCognitionPowerFlashPatch), nameof(BiasedCognitionPowerFlashPatch.Prefix));
        PatchPostfix(harmony, typeof(Hook), nameof(Hook.ModifyPowerAmountReceived), typeof(BiasedCognitionFocusGainPatch), nameof(BiasedCognitionFocusGainPatch.Postfix));
        PatchPostfix(harmony, typeof(Hailstorm), "get_CanonicalVars", typeof(HailstormVarsPatch), nameof(HailstormVarsPatch.Postfix));
        PatchPostfix(harmony, typeof(Hailstorm), "get_ExtraHoverTips", typeof(HailstormHoverTipsPatch), nameof(HailstormHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(Hailstorm), "OnUpgrade", typeof(HailstormUpgradePatch), nameof(HailstormUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(HailstormPower), "BeforeSideTurnEnd", typeof(HailstormPowerTurnEndPatch), nameof(HailstormPowerTurnEndPatch.Prefix));
        PatchPrefix(harmony, typeof(AbstractModel), "AfterOrbEvoked", typeof(HailstormAfterOrbEvokedPatch), nameof(HailstormAfterOrbEvokedPatch.Prefix));
        PatchPostfix(harmony, typeof(CardModel), "get_CanonicalEnergyCost", typeof(LegacyCostPatch), nameof(LegacyCostPatch.Postfix));
        PatchPostfix(harmony, typeof(Alignment), "get_CanonicalStarCost", typeof(AlignmentStarCostPatch), nameof(AlignmentStarCostPatch.Postfix));
        PatchPostfix(harmony, typeof(Glow), "get_CanonicalVars", typeof(GlowVarsPatch), nameof(GlowVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Glow), "OnPlay", typeof(GlowPlayPatch), nameof(GlowPlayPatch.Prefix));
        PatchPostfix(harmony, typeof(VoidForm), "get_CanonicalKeywords", typeof(VoidFormKeywordsPatch), nameof(VoidFormKeywordsPatch.Postfix));
        PatchPrefix(harmony, typeof(VoidForm), "OnUpgrade", typeof(VoidFormUpgradePatch), nameof(VoidFormUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(TheSealedThrone), "OnUpgrade", typeof(TheSealedThroneUpgradePatch), nameof(TheSealedThroneUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(BansheesCry), "OnUpgrade", typeof(BansheesCryUpgradePatch), nameof(BansheesCryUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Dirge), "get_CanonicalKeywords", typeof(DirgeKeywordsPatch), nameof(DirgeKeywordsPatch.Postfix));
        PatchPrefix(harmony, typeof(Seance), "OnUpgrade", typeof(SeanceUpgradePatch), nameof(SeanceUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Seance), "get_ExtraHoverTips", typeof(SeanceHoverTipsPatch), nameof(SeanceHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(Seance), "OnPlay", typeof(SeancePlayPatch), nameof(SeancePlayPatch.Prefix));
        PatchPostfix(harmony, typeof(BorrowedTime), "get_CanonicalVars", typeof(BorrowedTimeVarsPatch), nameof(BorrowedTimeVarsPatch.Postfix));
        PatchPostfix(harmony, typeof(BorrowedTime), "get_ExtraHoverTips", typeof(BorrowedTimeHoverTipsPatch), nameof(BorrowedTimeHoverTipsPatch.Postfix));
        PatchPrefix(harmony, typeof(BorrowedTime), "OnPlay", typeof(BorrowedTimePlayPatch), nameof(BorrowedTimePlayPatch.Prefix));
        PatchPrefix(harmony, typeof(BorrowedTime), "OnUpgrade", typeof(BorrowedTimeUpgradePatch), nameof(BorrowedTimeUpgradePatch.Prefix));
        PatchPostfix(harmony, typeof(Debilitate), "get_CanonicalVars", typeof(DebilitateVarsPatch), nameof(DebilitateVarsPatch.Postfix));
        PatchPrefix(harmony, typeof(Debilitate), "OnUpgrade", typeof(DebilitateUpgradePatch), nameof(DebilitateUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(Defy), "OnUpgrade", typeof(DefyUpgradePatch), nameof(DefyUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(Production), "OnUpgrade", typeof(ProductionUpgradePatch), nameof(ProductionUpgradePatch.Prefix));
        PatchPrefix(harmony, typeof(HiddenGem), "OnPlay", typeof(HiddenGemPlayPatch), nameof(HiddenGemPlayPatch.Prefix));
        PatchPrefix(harmony, typeof(LocTable), nameof(LocTable.GetRawText), typeof(CardDescriptionPatch), nameof(CardDescriptionPatch.Prefix));
        PatchPostfix(harmony, typeof(LocTable), nameof(LocTable.HasEntry), typeof(LocTableHasEntryPatch), nameof(LocTableHasEntryPatch.Postfix));
        PatchPrefix(harmony, typeof(PowerModel), "get_Icon", typeof(PowerIconPatch), nameof(PowerIconPatch.IconPrefix));
        PatchPrefix(harmony, typeof(PowerModel), "get_BigIcon", typeof(PowerIconPatch), nameof(PowerIconPatch.IconPrefix));
        PatchPostfix(harmony, typeof(ModManager), nameof(ModManager.GetGameplayRelevantModNameList), typeof(GameplayRelevantModNameListPatch), nameof(GameplayRelevantModNameListPatch.Postfix));
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
