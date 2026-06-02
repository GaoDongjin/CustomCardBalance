using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CustomCardBalance;

[HarmonyPatch(typeof(Untouchable), "get_CanonicalVars")]
public static class UntouchableVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Untouchable))
            __result = new DynamicVar[] { new BlockVar(9m, ValueProp.Move) };
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

[HarmonyPatch(typeof(Speedster), "OnUpgrade")]
public static class SpeedsterUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Speedster __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Speedster))
            return true;

        __instance.DynamicVars["SpeedsterPower"].UpgradeValueBy(1m);
        __instance.AddKeyword(CardKeyword.Innate);
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

        __instance.DynamicVars.Dexterity.UpgradeValueBy(2m);
        return false;
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

[HarmonyPatch(typeof(LocTable), nameof(LocTable.GetRawText))]
public static class CardDescriptionPatch
{
    [HarmonyPrefix]
    public static bool Prefix(string key, ref string __result)
    {
        switch (key)
        {
            case "SEANCE.description" when ModConfiguration.IsEnabled(CardIds.Seance):
                __result = LocManager.Instance?.Language == "zhs"
                    ? "将你[gold]抽牌堆[/gold]中的一张牌变化为[gold]{IfUpgraded:show:灵魂+|灵魂}[/gold]。"
                    : "Transform a card in your [gold]Draw Pile[/gold] into [gold]{IfUpgraded:show:Soul+|Soul}[/gold].";
                return false;
            case "WRAITH_FORM.description" when ModConfiguration.IsEnabled(CardIds.WraithForm):
                __result = LocManager.Instance?.Language == "zhs"
                    ? "获得{IntangiblePower:diff()}层[gold]无实体[/gold]。"
                    : "Gain {IntangiblePower:diff()} [gold]Intangible[/gold].";
                return false;
            case "EXPECT_A_FIGHT.description" when ModConfiguration.IsEnabled(CardIds.ExpectAFight):
                __result = LocManager.Instance?.Language == "zhs"
                    ? "你的[gold]手牌[/gold]中每有一张攻击牌，就会获得{energyPrefix:energyIcons(1)}。{InCombat:\n（获得{CalculatedEnergy:energyIcons()}。）|}"
                    : "Gain {energyPrefix:energyIcons(1)} for each Attack in your [gold]Hand[/gold].{InCombat:\n(Gain {CalculatedEnergy:energyIcons()}.)|}";
                return false;
            case "GLOW.description" when ModConfiguration.IsEnabled(CardIds.Glow):
                __result = LocManager.Instance?.Language == "zhs"
                    ? "获得{Stars:starIcons()}。\n抽{Cards:diff()}张牌。"
                    : "Gain {Stars:starIcons()}.\nDraw {Cards:diff()} {Cards:plural:card|cards}.";
                return false;
            case "BORROWED_TIME.description" when ModConfiguration.IsEnabled(CardIds.BorrowedTime):
                __result = LocManager.Instance?.Language == "zhs"
                    ? "给予自身{DoomPower:diff()}层[gold]灾厄[/gold]。\n获得{Energy:energyIcons()}。"
                    : "Apply {DoomPower:diff()} [gold]Doom[/gold] to yourself.\nGain {Energy:energyIcons()}.";
                return false;
            default:
                return true;
        }
    }
}

[HarmonyPatch(typeof(Dominate), "OnUpgrade")]
public static class DominateUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Dominate __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Dominate))
            return true;

        __instance.RemoveKeyword(CardKeyword.Exhaust);
        return false;
    }
}

[HarmonyPatch(typeof(ExpectAFight), "OnPlay")]
public static class ExpectAFightPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ExpectAFight __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.ExpectAFight))
            return true;

        __result = PlayAdjustedEffect(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PlayAdjustedEffect(ExpectAFight card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await PlayerCmd.GainEnergy(((CalculatedVar)card.DynamicVars["CalculatedEnergy"]).Calculate(cardPlay.Target), card.Owner);
    }
}

[HarmonyPatch(typeof(ForgottenRitual), "get_CanonicalKeywords")]
public static class ForgottenRitualKeywordsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<CardKeyword> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.ForgottenRitual))
            __result = System.Array.Empty<CardKeyword>();
    }
}

[HarmonyPatch(typeof(ForgottenRitual), "get_ExtraHoverTips")]
public static class ForgottenRitualHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        if (!ModConfiguration.IsEnabled(CardIds.ForgottenRitual))
            return;

        string exhaustTipId = HoverTipFactory.FromKeyword(CardKeyword.Exhaust).Id;
        __result = __result.Where(tip => tip.Id != exhaustTipId).ToArray();
    }
}

[HarmonyPatch(typeof(Murder), "OnUpgrade")]
public static class MurderUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Murder __instance)
    {
        if (!ModConfiguration.IsEnabled(CardIds.Murder))
            return true;

        __instance.EnergyCost.UpgradeBy(-1);
        __instance.DynamicVars["ExtraDamage"].UpgradeValueBy(1m);
        return false;
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
            __result = System.Array.Empty<CardKeyword>();
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
            __result = new IHoverTip[] { HoverTipFactory.FromPower<IntangiblePower>() };
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
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["IntangiblePower"].BaseValue,
            card.Owner.Creature,
            card);
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

[HarmonyPatch(typeof(Dirge), "get_CanonicalKeywords")]
public static class DirgeKeywordsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<CardKeyword> __result)
    {
        if (ModConfiguration.IsEnabled(CardIds.Dirge))
            __result = System.Array.Empty<CardKeyword>();
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
