using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CustomCardBalance;

[HarmonyPatch(typeof(ForgottenRitual), "get_CanonicalVars")]
public static class ForgottenRitualVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.ForgottenRitual))
            __result = new DynamicVar[] { new EnergyVar(3) };
    }
}

[HarmonyPatch(typeof(ForgottenRitual), "OnUpgrade")]
public static class ForgottenRitualUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(ForgottenRitual __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.ForgottenRitual))
            return true;

        __instance.RemoveKeyword(CardKeyword.Exhaust);
        return false;
    }
}

[HarmonyPatch(typeof(ForgottenRitual), "get_ExtraHoverTips")]
public static class ForgottenRitualHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ForgottenRitual __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.ForgottenRitual) || !__instance.IsUpgraded)
            return;

        string exhaustTipId = HoverTipFactory.FromKeyword(CardKeyword.Exhaust).Id;
        __result = __result.Where(tip => tip.Id != exhaustTipId).ToArray();
    }
}

[HarmonyPatch(typeof(Spite), "get_CanonicalVars")]
public static class SpiteVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Spite))
            __result = new DynamicVar[] { new DamageVar(6m, ValueProp.Move), new CardsVar(1) };
    }
}

[HarmonyPatch(typeof(Spite), "OnPlay")]
public static class SpitePlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Spite __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Spite))
            return true;

        __result = PlayVer100Effect(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PlayVer100Effect(Spite card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (LostHpThisTurn(card))
            await CardPileCmd.Draw(choiceContext, card.DynamicVars.Cards.IntValue, card.Owner);
    }

    private static bool LostHpThisTurn(Spite card)
    {
        return CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>().Any(e =>
            e.HappenedThisTurn(card.CombatState) &&
            e.Receiver == card.Owner.Creature &&
            e.Result.UnblockedDamage > 0);
    }
}

[HarmonyPatch(typeof(Spite), "OnUpgrade")]
public static class SpiteUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Spite __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Spite))
            return true;

        __instance.DynamicVars.Damage.UpgradeValueBy(3m);
        return false;
    }
}

[HarmonyPatch(typeof(CardModel), "get_Rarity")]
public static class AcrobaticsRarityPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref CardRarity __result, CardModel __instance)
    {
        if (ModConfiguration.IsEnabled(CardIds.Acrobatics) && __instance is Acrobatics)
            __result = CardRarity.Common;
    }
}

[HarmonyPatch(typeof(Untouchable), "get_CanonicalVars")]
public static class UntouchableVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Untouchable))
            __result = new DynamicVar[] { new BlockVar(7m, ValueProp.Move) };
    }
}

[HarmonyPatch(typeof(Untouchable), "OnUpgrade")]
public static class UntouchableUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Untouchable __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Untouchable))
            return true;

        __instance.DynamicVars.Block.UpgradeValueBy(3m);
        return false;
    }
}

[HarmonyPatch(typeof(Anticipate), "get_CanonicalVars")]
public static class AnticipateVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Anticipate))
            __result = new DynamicVar[] { new PowerVar<DexterityPower>(3m) };
    }
}

[HarmonyPatch(typeof(Anticipate), "OnUpgrade")]
public static class AnticipateUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Anticipate __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Anticipate))
            return true;

        __instance.DynamicVars.Dexterity.UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(Speedster), "OnUpgrade")]
public static class SpeedsterUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Speedster __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Speedster))
            return true;

        __instance.DynamicVars["SpeedsterPower"].UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(WraithForm), "get_CanonicalVars")]
public static class WraithFormVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.WraithForm))
            __result = new DynamicVar[] { new PowerVar<IntangiblePower>(2m) };
    }
}

[HarmonyPatch(typeof(WraithForm), "get_ExtraHoverTips")]
public static class WraithFormHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.WraithForm))
            __result = new IHoverTip[] { HoverTipFactory.FromPower<IntangiblePower>(), HoverTipFactory.FromPower<NoDexterityGainPower>() };
    }
}

[HarmonyPatch(typeof(WraithForm), "OnPlay")]
public static class WraithFormPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(WraithForm __instance, PlayerChoiceContext choiceContext, ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.WraithForm))
            return true;

        __result = PlayAdjustedEffect(__instance, choiceContext);
        return false;
    }

    private static async Task PlayAdjustedEffect(WraithForm card, PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<IntangiblePower>(choiceContext, card.Owner.Creature, card.DynamicVars["IntangiblePower"].BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<NoDexterityGainPower>(choiceContext, card.Owner.Creature, 1m, card.Owner.Creature, card);
    }
}

[HarmonyPatch(typeof(WraithForm), "OnUpgrade")]
public static class WraithFormUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(WraithForm __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.WraithForm))
            return true;

        __instance.DynamicVars["IntangiblePower"].UpgradeValueBy(1m);
        __instance.AddKeyword(CardKeyword.Retain);
        return false;
    }
}

[HarmonyPatch(typeof(Voltaic), "OnUpgrade")]
public static class VoltaicUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Voltaic __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Voltaic))
            return true;

        __instance.RemoveKeyword(CardKeyword.Exhaust);
        return false;
    }
}

[HarmonyPatch(typeof(Hotfix), "OnUpgrade")]
public static class HotfixUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Hotfix __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Hotfix))
            return true;

        __instance.RemoveKeyword(CardKeyword.Exhaust);
        __instance.DynamicVars["FocusPower"].UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(Defragment), "get_CanonicalVars")]
public static class DefragmentVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Defragment))
            __result = new DynamicVar[] { new PowerVar<FocusPower>(2m) };
    }
}

[HarmonyPatch(typeof(Defragment), "OnUpgrade")]
public static class DefragmentUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Defragment __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Defragment))
            return true;

        __instance.DynamicVars["FocusPower"].UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(Coolant), "get_CanonicalVars")]
public static class CoolantVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Coolant))
            __result = new DynamicVar[] { new PowerVar<CoolantPower>(3m) };
    }
}

[HarmonyPatch(typeof(Coolant), "OnUpgrade")]
public static class CoolantUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Coolant __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Coolant))
            return true;

        __instance.DynamicVars["CoolantPower"].UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(BiasedCognition), "get_CanonicalVars")]
public static class BiasedCognitionVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.BiasedCognition))
            __result = new DynamicVar[] { new PowerVar<FocusPower>(4m) };
    }
}

[HarmonyPatch(typeof(BiasedCognition), "get_ExtraHoverTips")]
public static class BiasedCognitionHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.BiasedCognition))
            __result = new IHoverTip[] { HoverTipFactory.FromPower<FocusPower>() };
    }
}

[HarmonyPatch(typeof(BiasedCognition), "OnPlay")]
public static class BiasedCognitionPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(BiasedCognition __instance, PlayerChoiceContext choiceContext, ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.BiasedCognition))
            return true;

        __result = PlayAdjustedEffect(__instance, choiceContext);
        return false;
    }

    private static async Task PlayAdjustedEffect(BiasedCognition card, PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<FocusPower>(choiceContext, card.Owner.Creature, card.DynamicVars["FocusPower"].BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<BiasedCognitionPower>(choiceContext, card.Owner.Creature, 1m, card.Owner.Creature, card);
    }
}

[HarmonyPatch(typeof(BiasedCognition), "OnUpgrade")]
public static class BiasedCognitionUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(BiasedCognition __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.BiasedCognition))
            return true;

        __instance.DynamicVars["FocusPower"].UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(BiasedCognitionPower), nameof(BiasedCognitionPower.AfterSideTurnStart))]
public static class BiasedCognitionPowerTurnStartPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.BiasedCognition))
            return true;

        __result = Task.CompletedTask;
        return false;
    }
}

[HarmonyPatch(typeof(BiasedCognitionPower), "get_StackType")]
public static class BiasedCognitionPowerStackTypePatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref PowerStackType __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.BiasedCognition))
            return true;

        __result = PowerStackType.None;
        return false;
    }
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterModifyingPowerAmountReceived))]
public static class BiasedCognitionPowerFlashPatch
{
    private static readonly MethodInfo FlashMethod = AccessTools.Method(typeof(PowerModel), "Flash")
        ?? throw new MissingMethodException(typeof(PowerModel).FullName, "Flash");

    [HarmonyPrefix]
    public static bool Prefix(AbstractModel __instance, ref Task __result)
    {
        if (__instance is not BiasedCognitionPower biasedCognitionPower || !ModConfiguration.IsEnabled(CardIds.BiasedCognition))
            return true;

        FlashMethod.Invoke(biasedCognitionPower, null);
        __result = Task.CompletedTask;
        return false;
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyPowerAmountReceived))]
public static class BiasedCognitionFocusGainPatch
{
    [HarmonyPostfix]
    public static void Postfix(PowerModel canonicalPower, Creature target, ref decimal __result, ref IEnumerable<AbstractModel> modifiers)
    {
        if (!ModConfiguration.IsEnabled(CardIds.BiasedCognition) || __result <= 0m)
            return;

        if (canonicalPower is not FocusPower && canonicalPower is not TemporaryFocusPower)
            return;

        BiasedCognitionPower? biasedCognitionPower = target.GetPower<BiasedCognitionPower>();
        if (biasedCognitionPower == null)
            return;

        __result = 0m;
        modifiers = modifiers.Concat(new AbstractModel[] { biasedCognitionPower }).ToList();
    }
}

[HarmonyPatch(typeof(Hailstorm), "get_CanonicalVars")]
public static class HailstormVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Hailstorm))
            __result = new DynamicVar[] { new PowerVar<HailstormPower>(2m) };
    }
}

[HarmonyPatch(typeof(Hailstorm), "get_ExtraHoverTips")]
public static class HailstormHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Hailstorm))
            __result = new IHoverTip[] { HoverTipFactory.FromOrb<FrostOrb>() };
    }
}

[HarmonyPatch(typeof(Hailstorm), "OnUpgrade")]
public static class HailstormUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Hailstorm __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Hailstorm))
            return true;

        __instance.AddKeyword(CardKeyword.Innate);
        return false;
    }
}

[HarmonyPatch(typeof(HailstormPower), "BeforeSideTurnEnd")]
public static class HailstormPowerTurnEndPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !ModConfiguration.IsEnabled(CardIds.Hailstorm);
    }
}

[HarmonyPatch(typeof(AbstractModel), "AfterOrbEvoked")]
public static class HailstormAfterOrbEvokedPatch
{
    [HarmonyPrefix]
    public static bool Prefix(AbstractModel __instance, PlayerChoiceContext choiceContext, OrbModel orb, ref Task __result)
    {
        if (__instance is not HailstormPower power || !ModConfiguration.IsEnabled(CardIds.Hailstorm))
            return true;

        __result = AfterOrbEvoked(power, choiceContext, orb);
        return false;
    }

    private static async Task AfterOrbEvoked(HailstormPower power, PlayerChoiceContext choiceContext, OrbModel orb)
    {
        if (orb is not FrostOrb || orb.Owner?.Creature != power.Owner)
            return;

        await CreatureCmd.Damage(choiceContext, power.CombatState.HittableEnemies, power.Amount, ValueProp.Unpowered, power.Owner);
    }
}

[HarmonyPatch(typeof(CardModel), "get_CanonicalEnergyCost")]
public static class LegacyCostPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref int __result, CardModel __instance)
    {
        if (ModConfiguration.IsEnabled(CardIds.Seance) && __instance is Seance)
            __result = 0;
        else if (ModConfiguration.IsEnabled(CardIds.BansheesCry) && __instance is BansheesCry)
            __result = 6;
        else if (ModConfiguration.IsEnabled(CardIds.Voltaic) && __instance is Voltaic)
            __result = 2;
        else if (ModConfiguration.IsEnabled(CardIds.Rainbow) && __instance is Rainbow)
            __result = 1;
        else if (ModConfiguration.IsEnabled(CardIds.BorrowedTime) && __instance is BorrowedTime)
            __result = 0;
    }
}

[HarmonyPatch(typeof(Alignment), "get_CanonicalStarCost")]
public static class AlignmentStarCostPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref int __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Alignment))
            __result = 2;
    }
}

[HarmonyPatch(typeof(Glow), "get_CanonicalVars")]
public static class GlowVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Glow))
            __result = new DynamicVar[] { new StarsVar(1), new CardsVar(2) };
    }
}

[HarmonyPatch(typeof(Glow), "OnPlay")]
public static class GlowPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Glow __instance, PlayerChoiceContext choiceContext, ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Glow))
            return true;

        __result = PlayAdjustedEffect(__instance, choiceContext);
        return false;
    }

    private static async Task PlayAdjustedEffect(Glow card, PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await PlayerCmd.GainStars(card.DynamicVars.Stars.BaseValue, card.Owner);
        await CardPileCmd.Draw(choiceContext, card.DynamicVars.Cards.BaseValue, card.Owner);
    }
}

[HarmonyPatch(typeof(VoidForm), "get_CanonicalKeywords")]
public static class VoidFormKeywordsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<CardKeyword> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.VoidForm))
            __result = Array.Empty<CardKeyword>();
    }
}

[HarmonyPatch(typeof(VoidForm), "OnUpgrade")]
public static class VoidFormUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(VoidForm __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.VoidForm))
            return true;

        __instance.DynamicVars["VoidFormPower"].UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(TheSealedThrone), "OnUpgrade")]
public static class TheSealedThroneUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(TheSealedThrone __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.TheSealedThrone))
            return true;

        __instance.AddKeyword(CardKeyword.Innate);
        return false;
    }
}

[HarmonyPatch(typeof(BansheesCry), "OnUpgrade")]
public static class BansheesCryUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(BansheesCry __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.BansheesCry))
            return true;

        __instance.DynamicVars.Damage.UpgradeValueBy(6m);
        return false;
    }
}

[HarmonyPatch(typeof(Dirge), "get_CanonicalKeywords")]
public static class DirgeKeywordsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<CardKeyword> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Dirge))
            __result = Array.Empty<CardKeyword>();
    }
}

[HarmonyPatch(typeof(Seance), "OnUpgrade")]
public static class SeanceUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !ModConfiguration.IsEnabled(CardIds.Seance);
    }
}

[HarmonyPatch(typeof(Seance), "get_ExtraHoverTips")]
public static class SeanceHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result, Seance __instance)
    {
        if (ModConfiguration.IsEnabled(CardIds.Seance))
            __result = new IHoverTip[] { HoverTipFactory.FromCard<Soul>(__instance.IsUpgraded) };
    }
}

[HarmonyPatch(typeof(Seance), "OnPlay")]
public static class SeancePlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Seance __instance, PlayerChoiceContext choiceContext, ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Seance))
            return true;

        __result = PlayLegacyEffect(__instance, choiceContext);
        return false;
    }

    private static async Task PlayLegacyEffect(Seance card, PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        List<CardModel> cardsIn = (from c in PileType.Draw.GetPile(card.Owner).Cards
            orderby c.Rarity, c.Id
            select c).ToList();
        List<CardModel> selectedCards = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            cardsIn,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, card.DynamicVars.Cards.IntValue))).ToList();
        foreach (CardModel selectedCard in selectedCards)
        {
            CardPileAddResult? result = await CardCmd.TransformTo<Soul>(selectedCard);
            if (card.IsUpgraded && result.HasValue)
                CardCmd.Upgrade(result.Value.cardAdded);
        }
    }
}

[HarmonyPatch(typeof(BorrowedTime), "get_CanonicalVars")]
public static class BorrowedTimeVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.BorrowedTime))
            __result = new DynamicVar[] { new PowerVar<DoomPower>(3m), new EnergyVar(1) };
    }
}

[HarmonyPatch(typeof(BorrowedTime), "get_ExtraHoverTips")]
public static class BorrowedTimeHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(BorrowedTime __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.BorrowedTime))
            __result = new IHoverTip[] { HoverTipFactory.FromPower<DoomPower>(), HoverTipFactory.ForEnergy(__instance) };
    }
}

[HarmonyPatch(typeof(BorrowedTime), "OnPlay")]
public static class BorrowedTimePlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(BorrowedTime __instance, PlayerChoiceContext choiceContext, ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.BorrowedTime))
            return true;

        __result = PlayAdjustedEffect(__instance, choiceContext);
        return false;
    }

    private static async Task PlayAdjustedEffect(BorrowedTime card, PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DoomPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Doom.BaseValue, card.Owner.Creature, card);
        await PlayerCmd.GainEnergy(card.DynamicVars.Energy.BaseValue, card.Owner);
    }
}

[HarmonyPatch(typeof(BorrowedTime), "OnUpgrade")]
public static class BorrowedTimeUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(BorrowedTime __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.BorrowedTime))
            return true;

        __instance.DynamicVars.Energy.UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(Debilitate), "get_CanonicalVars")]
public static class DebilitateVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Debilitate))
            __result = new DynamicVar[] { new DamageVar(7m, ValueProp.Move), new PowerVar<DebilitatePower>(3m) };
    }
}

[HarmonyPatch(typeof(Debilitate), "OnUpgrade")]
public static class DebilitateUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Debilitate __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Debilitate))
            return true;

        __instance.DynamicVars.Damage.UpgradeValueBy(2m);
        __instance.DynamicVars["DebilitatePower"].UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(Defy), "OnUpgrade")]
public static class DefyUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Defy __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Defy))
            return true;

        __instance.DynamicVars.Block.UpgradeValueBy(1m);
        __instance.DynamicVars.Weak.UpgradeValueBy(1m);
        return false;
    }
}

[HarmonyPatch(typeof(Production), "OnUpgrade")]
public static class ProductionUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Production __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Production))
            return true;

        __instance.RemoveKeyword(CardKeyword.Exhaust);
        return false;
    }
}

[HarmonyPatch(typeof(HiddenGem), "OnPlay")]
public static class HiddenGemPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(HiddenGem __instance, PlayerChoiceContext choiceContext, ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.HiddenGem))
            return true;

        __result = PlayVer100Effect(__instance, choiceContext);
        return false;
    }

    private static async Task PlayVer100Effect(HiddenGem card, PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        List<CardModel> drawPileCards = PileType.Draw.GetPile(card.Owner).Cards.ToList();
        if (drawPileCards.Count == 0)
            return;

        List<CardModel> validCards = drawPileCards.Where(c =>
        {
            bool valid = !c.Keywords.Contains(CardKeyword.Unplayable);
            if (valid)
            {
                CardType type = c.Type;
                valid = !((uint)(type - 5) <= 1u);
            }
            return valid;
        }).ToList();
        List<CardModel> playableCards = validCards.Where(c =>
        {
            CardType type = c.Type;
            return (uint)(type - 1) <= 2u;
        }).ToList();
        IEnumerable<CardModel> candidates = playableCards.Count == 0 ? validCards : playableCards;
        CardModel? selectedCard = card.Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (selectedCard != null)
        {
            selectedCard.BaseReplayCount += card.DynamicVars["Replay"].IntValue;
            CardCmd.Preview(selectedCard);
        }
    }
}

public static class CustomLocalizationOverrides
{
    public static bool ShouldOverride(string key)
    {
        return key switch
        {
            "WRAITH_FORM.description" => ModConfiguration.IsEnabled(CardIds.WraithForm),
            "BIASED_COGNITION.description" => ModConfiguration.IsEnabled(CardIds.BiasedCognition),
            "BIASED_COGNITION_POWER.description" => ModConfiguration.IsEnabled(CardIds.BiasedCognition),
            "BIASED_COGNITION_POWER.smartDescription" => ModConfiguration.IsEnabled(CardIds.BiasedCognition),
            "NO_DEXTERITY_GAIN_POWER.title" => true,
            "NO_DEXTERITY_GAIN_POWER.description" => true,
            "NO_DEXTERITY_GAIN_POWER.smartDescription" => true,
            _ => false
        };
    }

    public static bool HasKey(string key)
    {
        return key is
            "WRAITH_FORM.description" or
            "BIASED_COGNITION.description" or
            "BIASED_COGNITION_POWER.description" or
            "BIASED_COGNITION_POWER.smartDescription" or
            "NO_DEXTERITY_GAIN_POWER.title" or
            "NO_DEXTERITY_GAIN_POWER.description" or
            "NO_DEXTERITY_GAIN_POWER.smartDescription";
    }

    public static bool TryGet(string key, out string text)
    {
        text = string.Empty;
        if (!ShouldOverride(key) || !HasKey(key))
            return false;

        string language = LocManager.Instance.Language ?? "eng";
        text = key switch
        {
            "WRAITH_FORM.description" => WraithFormDescription(language),
            "BIASED_COGNITION.description" => BiasedCognitionDescription(language),
            "BIASED_COGNITION_POWER.description" => BiasedCognitionPowerDescription(language),
            "BIASED_COGNITION_POWER.smartDescription" => BiasedCognitionPowerDescription(language),
            "NO_DEXTERITY_GAIN_POWER.title" => NoDexterityGainTitle(language),
            "NO_DEXTERITY_GAIN_POWER.description" => NoDexterityGainDescription(language),
            "NO_DEXTERITY_GAIN_POWER.smartDescription" => NoDexterityGainDescription(language),
            _ => string.Empty
        };
        return text.Length > 0;
    }

    private static string WraithFormDescription(string language)
    {
        return language switch
        {
            "deu" => "Erhalte {IntangiblePower:diff()} [gold]Immateriell[/gold].\nDu kannst in diesem Kampf keine [gold]Geschicklichkeit[/gold] erhalten.",
            "esp" => "Obtienes {IntangiblePower:diff()} de [gold]intangibilidad[/gold].\nNo puedes obtener [gold]destreza[/gold] en este combate.",
            "fra" => "Gagnez [gold]Intangible[/gold] ({IntangiblePower:diff()}).\nVous ne pouvez pas gagner de [gold]Dextérité[/gold] pendant ce combat.",
            "ita" => "Ottieni {IntangiblePower:diff()} di [gold]Intangibilità[/gold].\nNon puoi ottenere [gold]Destrezza[/gold] in questo combattimento.",
            "jpn" => "[gold]霊体[/gold]{IntangiblePower:diff()}を得る。\nこの戦闘中、[gold]敏捷[/gold]を得られない。",
            "kor" => "[gold]불가침[/gold]을 {IntangiblePower:diff()} 얻습니다.\n이번 전투 동안 [gold]민첩[/gold]을 얻을 수 없습니다.",
            "pol" => "Zyskaj {IntangiblePower:diff()} pkt. [gold]Nieuchwytności[/gold].\nNie możesz zyskać [gold]Zręczności[/gold] w tej walce.",
            "ptb" => "Receba {IntangiblePower:diff()} de [gold]Intangível[/gold].\nVocê não pode receber [gold]Destreza[/gold] neste combate.",
            "rus" => "Дает {IntangiblePower:diff()} [gold]неосязаемости[/gold].\nВы не можете получать [gold]ловкость[/gold] в этом бою.",
            "spa" => "Gana {IntangiblePower:diff()} [gold]Intangible[/gold].\nNo puedes ganar [gold]Destreza[/gold] durante este combate.",
            "tha" => "ได้รับ {IntangiblePower:diff()} [gold]ไร้ตัวตน[/gold]\nใน combat นี้ คุณไม่สามารถได้รับ [gold]ความชำนาญ[/gold] ได้",
            "tur" => "{IntangiblePower:diff()} [gold]Soyutluk[/gold] elde et.\nBu savaşta [gold]Beceriklilik[/gold] elde edemezsin.",
            "zhs" => "获得{IntangiblePower:diff()}层[gold]无实体[/gold]。\n本次战斗中不能再获得[gold]敏捷[/gold]。",
            _ => "Gain {IntangiblePower:diff()} [gold]Intangible[/gold].\nYou cannot gain [gold]Dexterity[/gold] this combat."
        };
    }

    private static string BiasedCognitionDescription(string language)
    {
        return language switch
        {
            "deu" => "Erhalte {FocusPower:diff()} [gold]Fokus[/gold].\nDu kannst in diesem Kampf keinen [gold]Fokus[/gold] erhalten.",
            "esp" => "Obtienes {FocusPower:diff()} de [gold]concentración[/gold].\nNo puedes obtener [gold]concentración[/gold] en este combate.",
            "fra" => "Gagnez {FocusPower:diff()} de [gold]Focalisation[/gold].\nVous ne pouvez pas gagner de [gold]Focalisation[/gold] pendant ce combat.",
            "ita" => "Ottieni {FocusPower:diff()} di [gold]Focus[/gold].\nNon puoi ottenere [gold]Focus[/gold] in questo combattimento.",
            "jpn" => "[gold]集中力[/gold]{FocusPower:diff()}を得る。\nこの戦闘中、[gold]集中力[/gold]を得られない。",
            "kor" => "[gold]밀집[/gold]을 {FocusPower:diff()} 얻습니다.\n이번 전투 동안 [gold]밀집[/gold]을 얻을 수 없습니다.",
            "pol" => "Zyskaj {FocusPower:diff()} pkt. [gold]Skupienia[/gold].\nNie możesz zyskać [gold]Skupienia[/gold] w tej walce.",
            "ptb" => "Receba {FocusPower:diff()} de [gold]Foco[/gold].\nVocê não pode receber [gold]Foco[/gold] neste combate.",
            "rus" => "Дает {FocusPower:diff()} [gold]фокуса[/gold].\nВы не можете получать [gold]фокус[/gold] в этом бою.",
            "spa" => "Gana {FocusPower:diff()} de [gold]Concentración[/gold].\nNo puedes ganar [gold]Concentración[/gold] durante este combate.",
            "tha" => "ได้รับ {FocusPower:diff()} [gold]โฟกัส[/gold]\nใน combat นี้ คุณไม่สามารถได้รับ [gold]โฟกัส[/gold] ได้",
            "tur" => "{FocusPower:diff()} [gold]Odak[/gold] elde et.\nBu savaşta [gold]Odak[/gold] elde edemezsin.",
            "zhs" => "获得{FocusPower:diff()}点[gold]集中[/gold]。\n本次战斗中不能再获得[gold]集中[/gold]。",
            _ => "Gain {FocusPower:diff()} [gold]Focus[/gold].\nYou cannot gain [gold]Focus[/gold] this combat."
        };
    }

    private static string NoDexterityGainTitle(string language)
    {
        return language switch
        {
            "deu" => "Kein Geschicklichkeitsgewinn",
            "esp" => "Sin ganancia de destreza",
            "fra" => "Pas de gain de Dextérité",
            "ita" => "Niente Destrezza",
            "jpn" => "敏捷獲得不可",
            "kor" => "민첩 획득 불가",
            "pol" => "Brak zysku Zręczności",
            "ptb" => "Sem ganho de Destreza",
            "rus" => "Нет прироста ловкости",
            "spa" => "Sin ganancia de Destreza",
            "tha" => "ไม่ได้รับความชำนาญ",
            "tur" => "Beceriklilik kazanılamaz",
            "zhs" => "无法获得敏捷",
            _ => "No Dexterity Gain"
        };
    }

    private static string NoDexterityGainDescription(string language)
    {
        return language switch
        {
            "deu" => "Du kannst in diesem Kampf keine [gold]Geschicklichkeit[/gold] erhalten.",
            "esp" => "No puedes obtener [gold]destreza[/gold] en este combate.",
            "fra" => "Vous ne pouvez pas gagner de [gold]Dextérité[/gold] pendant ce combat.",
            "ita" => "Non puoi ottenere [gold]Destrezza[/gold] in questo combattimento.",
            "jpn" => "この戦闘中、[gold]敏捷[/gold]を得られない。",
            "kor" => "이번 전투 동안 [gold]민첩[/gold]을 얻을 수 없습니다.",
            "pol" => "Nie możesz zyskać [gold]Zręczności[/gold] w tej walce.",
            "ptb" => "Você não pode receber [gold]Destreza[/gold] neste combate.",
            "rus" => "Вы не можете получать [gold]ловкость[/gold] в этом бою.",
            "spa" => "No puedes ganar [gold]Destreza[/gold] durante este combate.",
            "tha" => "ใน combat นี้ คุณไม่สามารถได้รับ [gold]ความชำนาญ[/gold] ได้",
            "tur" => "Bu savaşta [gold]Beceriklilik[/gold] elde edemezsin.",
            "zhs" => "本次战斗中不能再获得[gold]敏捷[/gold]。",
            _ => "You cannot gain [gold]Dexterity[/gold] this combat."
        };
    }

    private static string BiasedCognitionPowerDescription(string language)
    {
        return language switch
        {
            "deu" => "Du kannst in diesem Kampf keinen [gold]Fokus[/gold] erhalten.",
            "esp" => "No puedes obtener [gold]concentración[/gold] en este combate.",
            "fra" => "Vous ne pouvez pas gagner de [gold]Focalisation[/gold] pendant ce combat.",
            "ita" => "Non puoi ottenere [gold]Focus[/gold] in questo combattimento.",
            "jpn" => "この戦闘中、[gold]集中力[/gold]を得られない。",
            "kor" => "이번 전투 동안 [gold]밀집[/gold]을 얻을 수 없습니다.",
            "pol" => "Nie możesz zyskać [gold]Skupienia[/gold] w tej walce.",
            "ptb" => "Você não pode receber [gold]Foco[/gold] neste combate.",
            "rus" => "Вы не можете получать [gold]фокус[/gold] в этом бою.",
            "spa" => "No puedes ganar [gold]Concentración[/gold] durante este combate.",
            "tha" => "ใน combat นี้ คุณไม่สามารถได้รับ [gold]โฟกัส[/gold] ได้",
            "tur" => "Bu savaşta [gold]Odak[/gold] elde edemezsin.",
            "zhs" => "本次战斗中不能再获得[gold]集中[/gold]。",
            _ => "You cannot gain [gold]Focus[/gold] this combat."
        };
    }
}

[HarmonyPatch(typeof(LocTable), nameof(LocTable.GetRawText))]
public static class CardDescriptionPatch
{
    [HarmonyPrefix]
    public static bool Prefix(string key, ref string __result)
    {
        if (CustomLocalizationOverrides.TryGet(key, out __result))
            return false;

        bool zhs = LocManager.Instance?.Language == "zhs";
        switch (key)
        {
            case "SPITE.description" when ModConfiguration.IsEnabled(CardIds.Spite):
                __result = zhs
                    ? "造成{Damage:diff()}点伤害。\n如果你在本回合失去过生命值，则抽{Cards:diff()}张牌。"
                    : "Deal {Damage:diff()} damage.\nIf you lost HP this turn, draw {Cards:diff()} {Cards:plural:card|cards}.";
                return false;
            case "HAILSTORM.description" when ModConfiguration.IsEnabled(CardIds.Hailstorm):
                __result = zhs
                    ? "每当你激发[gold]冰霜[/gold]充能球时，对所有敌人造成{HailstormPower:diff()}点伤害。"
                    : "Whenever you Evoke a [gold]Frost[/gold] Orb, deal {HailstormPower:diff()} damage to ALL enemies.";
                return false;
            case "HIDDEN_GEM.description" when ModConfiguration.IsEnabled(CardIds.HiddenGem):
                __result = zhs
                    ? "你[gold]抽牌堆[/gold]中的一张随机牌获得{Replay:diff()}层[gold]重放[/gold]。"
                    : "A random card in your [gold]Draw Pile[/gold] gains [gold]Replay[/gold] {Replay:diff()}.";
                return false;
            case "SEANCE.description" when ModConfiguration.IsEnabled(CardIds.Seance):
                __result = zhs
                    ? "将你[gold]抽牌堆[/gold]中的一张牌变化为[gold]{IfUpgraded:show:灵魂+|灵魂}[/gold]。"
                    : "Transform a card in your [gold]Draw Pile[/gold] into [gold]{IfUpgraded:show:Soul+|Soul}[/gold].";
                return false;
            case "GLOW.description" when ModConfiguration.IsEnabled(CardIds.Glow):
                __result = zhs
                    ? "获得{Stars:starIcons()}。\n抽{Cards:diff()}张牌。"
                    : "Gain {Stars:starIcons()}.\nDraw {Cards:diff()} {Cards:plural:card|cards}.";
                return false;
            case "BORROWED_TIME.description" when ModConfiguration.IsEnabled(CardIds.BorrowedTime):
                __result = zhs
                    ? "给予自身{DoomPower:diff()}层[gold]灾厄[/gold]。\n获得{Energy:energyIcons()}。"
                    : "Apply {DoomPower:diff()} [gold]Doom[/gold] to yourself.\nGain {Energy:energyIcons()}.";
                return false;
            default:
                return true;
        }
    }
}

[HarmonyPatch(typeof(LocTable), nameof(LocTable.HasEntry))]
public static class LocTableHasEntryPatch
{
    [HarmonyPostfix]
    public static void Postfix(string key, ref bool __result)
    {
        if (!__result && CustomLocalizationOverrides.ShouldOverride(key) && CustomLocalizationOverrides.HasKey(key))
            __result = true;
    }
}

public sealed class NoDexterityGainPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m)
            return false;

        if (canonicalPower is not DexterityPower && canonicalPower is not TemporaryDexterityPower)
            return false;

        modifiedAmount = 0m;
        return true;
    }

    public override Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        Flash();
        return Task.CompletedTask;
    }
}

public static class PowerIconPatch
{
    private static readonly Dictionary<string, Texture2D> _cache = new();

    public static bool IconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        string? assetName = GetAssetName(__instance);
        if (assetName == null)
            return true;

        Texture2D? texture = LoadTexture(assetName);
        if (texture == null)
            return true;

        __result = texture;
        return false;
    }

    private static string? GetAssetName(PowerModel power)
    {
        return power switch
        {
            NoDexterityGainPower => "no_dexterity_gain_power.png",
            _ => null
        };
    }

    private static Texture2D? LoadTexture(string assetName)
    {
        if (_cache.TryGetValue(assetName, out Texture2D? cached))
            return cached;

        string path = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location) ?? string.Empty, "assets", assetName);
        if (!File.Exists(path))
        {
            Log.Warn($"[CustomCardBalance] Missing custom power icon asset: {path}");
            return null;
        }

        try
        {
            Image? image = Image.LoadFromFile(path);
            if (image == null)
                return null;

            Texture2D texture = ImageTexture.CreateFromImage(image);
            _cache[assetName] = texture;
            return texture;
        }
        catch (Exception ex)
        {
            Log.Error($"[CustomCardBalance] Failed to load custom power icon {path}.\n{ex}");
            return null;
        }
    }
}

[HarmonyPatch(typeof(ModManager), nameof(ModManager.GetGameplayRelevantModNameList))]
public static class GameplayRelevantModNameListPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref List<string>? __result)
    {
        if (__result == null)
            return;

        for (int i = 0; i < __result.Count; i++)
        {
            if (__result[i].StartsWith("CustomCardBalance-", StringComparison.Ordinal))
                __result[i] = $"{__result[i]}-{ModConfiguration.GetMultiplayerCompatibilityToken()}";
        }
    }
}
