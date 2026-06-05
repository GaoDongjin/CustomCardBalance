using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace CustomCardBalance;

public enum CardCategory
{
    All,
    Ironclad,
    Silent,
    Defect,
    Regent,
    Necrobinder,
    Other
}

public sealed record CardBalanceEntry(
    string Id,
    CardCategory Category,
    string Name,
    string Adjustment,
    string BaseEffect,
    string UpgradedEffect);

[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
public static class MainMenuSettingsPanelPatch
{
    [HarmonyPostfix]
    public static void Postfix(NMainMenu __instance)
    {
        if (__instance.GetNodeOrNull<CardBalanceSettingsPanel>("CardBalanceSettingsPanel") != null)
            return;

        var stopwatch = Stopwatch.StartNew();
        var panel = new CardBalanceSettingsPanel { Name = "CardBalanceSettingsPanel" };
        __instance.AddChild(panel);
        panel.Initialize(__instance);
        Log.Info($"[CustomCardBalance] Settings panel shell registered in {stopwatch.ElapsedMilliseconds}ms.");
    }
}

[HarmonyPatch(typeof(NGame), nameof(NGame._Input))]
public static class MainMenuSettingsHotkeyPatch
{
    [HarmonyPrefix]
    public static void Prefix(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey { Keycode: Key.F1, Pressed: true, Echo: false })
            return;

        NMainMenu? mainMenu = NGame.Instance?.MainMenu;
        CardBalanceSettingsPanel? panel = mainMenu?.GetNodeOrNull<CardBalanceSettingsPanel>("CardBalanceSettingsPanel");
        if (panel?.TryToggleFromMainMenuHotkey() != true)
            return;

        panel.GetViewport().SetInputAsHandled();
    }
}

[HarmonyPatch(typeof(NSettingsScreen), nameof(NSettingsScreen._Ready))]
public static class SettingsScreenSettingsPanelPatch
{
    private const string SettingsRowName = "CustomCardBalanceSettings";

    [HarmonyPostfix]
    public static void Postfix(NSettingsScreen __instance)
    {
        NMainMenu? mainMenu = NGame.Instance?.MainMenu;
        CardBalanceSettingsPanel? panel = mainMenu?.GetNodeOrNull<CardBalanceSettingsPanel>("CardBalanceSettingsPanel");
        VBoxContainer? content = __instance.GetNodeOrNull<NSettingsPanel>("%GeneralSettings")?.Content;
        if (panel == null || content == null || content.GetNodeOrNull(SettingsRowName) != null)
            return;

        ColorRect? templateDivider = content.GetNodeOrNull<ColorRect>("ModdingDivider");
        MarginContainer? templateRow = content.GetNodeOrNull<MarginContainer>("Modding");
        if (templateDivider == null || templateRow == null)
            return;

        var divider = (ColorRect)templateDivider.Duplicate();
        divider.Name = $"{SettingsRowName}Divider";
        divider.Visible = true;

        var row = (MarginContainer)templateRow.Duplicate();
        row.Name = SettingsRowName;
        row.Visible = true;

        int insertIndex = templateRow.GetIndex() + 1;
        content.AddChild(divider);
        content.MoveChild(divider, insertIndex);
        content.AddChild(row);
        content.MoveChild(row, insertIndex + 1);

        row.GetNode<RichTextLabel>("Label").Text = "Custom Card Balance";
        NClickableControl openButton = row.GetNode<NClickableControl>("ModdingButton");
        openButton.Name = "OpenButton";
        row.GetNode<Label>("OpenButton/Label").Text = "打开设置";
        openButton.Connect(
            NClickableControl.SignalName.Released,
            Callable.From<NClickableControl>(_ => panel.OpenFromSettingsMenu()));
    }
}

public partial class CardBalanceSettingsPanel : Control
{
    private const float CardNameWidth = 152f;
    private const float AdjustmentWidth = 258f;
    private const float EffectWidth = 450f;
    private const float ToggleWidth = 98f;
    private const float HeaderRowHeight = 42f;
    private const float EntryRowHeight = 72f;
    private const float CardNameLeftPadding = 6f;

    private static readonly CardCategory[] _categoryOrder =
    {
        CardCategory.Ironclad,
        CardCategory.Silent,
        CardCategory.Defect,
        CardCategory.Regent,
        CardCategory.Necrobinder,
        CardCategory.Other
    };

    private static readonly IReadOnlyList<CardBalanceEntry> _entries = new[]
    {
        new CardBalanceEntry(CardIds.ForgottenRitual, CardCategory.Ironclad, "被遗忘的仪式", "升级后移除消耗，但不增加能量", "如果你在本回合消耗过卡牌，则获得3点能量。消耗。", "如果你在本回合消耗过卡牌，则获得3点能量。"),
        new CardBalanceEntry(CardIds.Spite, CardCategory.Ironclad, "怨恨", "伤害改为抽牌", "造成6点伤害。如果你在本回合失去过生命值，则抽1张牌。", "造成9点伤害。如果你在本回合失去过生命值，则抽1张牌。"),
        new CardBalanceEntry(CardIds.Acrobatics, CardCategory.Silent, "杂技", "稀有度：罕见 → 普通", "抽3张牌。丢弃1张牌。", "抽4张牌。丢弃1张牌。"),
        new CardBalanceEntry(CardIds.Untouchable, CardCategory.Silent, "触不可及", "格挡：6/8 → 7/10", "奇巧。获得7点格挡。", "奇巧。获得10点格挡。"),
        new CardBalanceEntry(CardIds.Anticipate, CardCategory.Silent, "预判", "敏捷：2/3 → 3/4", "在本回合获得3点敏捷。", "在本回合获得4点敏捷。"),
        new CardBalanceEntry(CardIds.Speedster, CardCategory.Silent, "速行者", "升级后额外伤害+1", "每当你在回合中抽到牌时，对所有敌人造成2点伤害。", "每当你在回合中抽到牌时，对所有敌人造成3点伤害。"),
        new CardBalanceEntry(CardIds.WraithForm, CardCategory.Silent, "幽魂形态", "升级后获得保留；负面效果削弱", "获得2层无实体。本次战斗中不能再获得敏捷。", "保留。获得3层无实体。本次战斗中不能再获得敏捷。"),
        new CardBalanceEntry(CardIds.Voltaic, CardCategory.Defect, "电流相生", "耗能：3 → 2", "生成等量于你在这场战斗中生成过的闪电充能球数量的闪电充能球。消耗。", "生成等量于你在这场战斗中生成过的闪电充能球数量的闪电充能球。"),
        new CardBalanceEntry(CardIds.Hotfix, CardCategory.Defect, "热修复", "移除消耗；升级后集中+1", "在本回合获得2点集中。消耗。", "在本回合获得3点集中。"),
        new CardBalanceEntry(CardIds.Defragment, CardCategory.Defect, "碎片整理", "集中+1", "获得2点集中。", "获得3点集中。"),
        new CardBalanceEntry(CardIds.Coolant, CardCategory.Defect, "冷却剂", "格挡值+1", "在你的回合开始时，你每有一种不同的充能球，就获得3点格挡。", "在你的回合开始时，你每有一种不同的充能球，就获得4点格挡。"),
        new CardBalanceEntry(CardIds.BiasedCognition, CardCategory.Defect, "偏差认知", "负面效果削弱", "获得4点集中。本次战斗中不能再获得集中。", "获得5点集中。本次战斗中不能再获得集中。"),
        new CardBalanceEntry(CardIds.Hailstorm, CardCategory.Defect, "冰雹风暴", "激发充能球", "每当你激发冰霜充能球时，对所有敌人造成2点伤害。", "固有。每当你激发冰霜充能球时，对所有敌人造成2点伤害。"),
        new CardBalanceEntry(CardIds.Rainbow, CardCategory.Defect, "彩虹", "耗能：2 → 1", "生成1个闪电充能球。生成1个冰霜充能球。生成1个黑暗充能球。消耗。", "生成1个闪电充能球。生成1个冰霜充能球。生成1个黑暗充能球。"),
        new CardBalanceEntry(CardIds.Glow, CardCategory.Regent, "辉光", "抽2张牌", "获得1点星。抽2张牌。", "获得2点星。抽2张牌。"),
        new CardBalanceEntry(CardIds.Alignment, CardCategory.Regent, "星位序列", "星消耗：3 → 2", "获得2点能量。", "获得3点能量。"),
        new CardBalanceEntry(CardIds.VoidForm, CardCategory.Regent, "虚空形态", "移除虚无", "结束你的回合。你可以免费打出每回合的前2张牌。", "结束你的回合。你可以免费打出每回合的前3张牌。"),
        new CardBalanceEntry(CardIds.TheSealedThrone, CardCategory.Regent, "封印王座", "升级后添加固有", "你每打出一张牌，获得1点星。", "固有。你每打出一张牌，获得1点星。"),
        new CardBalanceEntry(CardIds.BansheesCry, CardCategory.Necrobinder, "女妖之嚎", "耗能：9 → 6；升级改为伤害+6", "对所有敌人造成33点伤害。本场战斗中每打出过一张虚无牌，此牌的耗能就减少2。", "对所有敌人造成39点伤害。本场战斗中每打出过一张虚无牌，此牌的耗能就减少2。"),
        new CardBalanceEntry(CardIds.Dirge, CardCategory.Necrobinder, "挽歌", "移除消耗", "召唤3X次。将X张灵魂添加到你的抽牌堆中。", "召唤4X次。将X张灵魂+添加到你的抽牌堆中。"),
        new CardBalanceEntry(CardIds.Seance, CardCategory.Necrobinder, "降灵", "耗能统一为0；升级后生成灵魂+", "虚无。将你抽牌堆中的一张牌变化为灵魂。", "虚无。将你抽牌堆中的一张牌变化为灵魂+。"),
        new CardBalanceEntry(CardIds.BorrowedTime, CardCategory.Necrobinder, "预借时间", "不额外增加耗能", "给予自身3层灾厄。获得1点能量。", "给予自身3层灾厄。获得2点能量。"),
        new CardBalanceEntry(CardIds.Debilitate, CardCategory.Necrobinder, "摧残", "降低伤害，增加效果持续时间", "造成7点伤害。在接下来的3回合内，该敌人身上的易伤与虚弱效率翻倍。", "造成9点伤害。在接下来的4回合内，该敌人身上的易伤与虚弱效率翻倍。"),
        new CardBalanceEntry(CardIds.Defy, CardCategory.Necrobinder, "违逆", "升级改为格挡+1、虚弱+1", "虚无。获得6点格挡。给予1层虚弱。", "虚无。获得7点格挡。给予2层虚弱。"),
        new CardBalanceEntry(CardIds.Production, CardCategory.Other, "生产制造", "升级后移除消耗", "获得2点能量。消耗。", "获得2点能量。"),
        new CardBalanceEntry(CardIds.HiddenGem, CardCategory.Other, "未掘宝石", "随机牌获取效果", "你抽牌堆中的一张随机牌获得2层重放。", "你抽牌堆中的一张随机牌获得3层重放。")
    };

    private readonly Dictionary<CardCategory, Button> _tabButtons = new();
    private readonly Dictionary<string, CheckButton> _toggleButtons = new();
    private Dictionary<string, bool> _draft = new();
    private NMainMenu? _mainMenu;
    private VBoxContainer? _tableRows;
    private CardCategory _selectedCategory = CardCategory.All;
    private bool _uiBuilt;

    public void Initialize(NMainMenu mainMenu)
    {
        _mainMenu = mainMenu;
        _draft = ModConfiguration.CreateDraft();
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false;
    }

    public bool TryToggleFromMainMenuHotkey()
    {
        if (_mainMenu == null || NGame.Instance?.MainMenu != _mainMenu)
            return false;
        if (!Visible && _mainMenu.SubmenuStack.SubmenusOpen)
            return false;

        SetPanelVisible(!Visible);
        return true;
    }

    public void OpenFromSettingsMenu()
    {
        if (_mainMenu == null || NGame.Instance?.MainMenu != _mainMenu || Visible)
            return;

        EnsureUiBuilt();
        SetPanelVisible(true);
    }

    private void EnsureUiBuilt()
    {
        if (_uiBuilt)
            return;

        var stopwatch = Stopwatch.StartNew();
        BuildUi();
        _uiBuilt = true;
        RenderRows();
        Log.Info($"[CustomCardBalance] Settings UI built on demand in {stopwatch.ElapsedMilliseconds}ms.");
    }

    private void BuildUi()
    {
        var page = new PanelContainer();
        page.AnchorLeft = 0.055f;
        page.AnchorTop = 0.075f;
        page.AnchorRight = 0.945f;
        page.AnchorBottom = 0.925f;
        page.AddThemeStyleboxOverride("panel", Box("201D20F5", "A58B50", 2, 24));
        AddChild(page);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 34);
        margin.AddThemeConstantOverride("margin_top", 26);
        margin.AddThemeConstantOverride("margin_right", 34);
        margin.AddThemeConstantOverride("margin_bottom", 22);
        page.AddChild(margin);

        var content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 12);
        margin.AddChild(content);

        var header = new HBoxContainer();
        header.AddThemeConstantOverride("separation", 12);
        content.AddChild(header);
        header.AddChild(Label("Custom Card Balance 设置", 40, StsColors.cream));
        header.AddChild(Spacer());
        header.AddChild(Label("F1 打开 / 关闭", 24, StsColors.gold));

        var subtitle = new HBoxContainer();
        content.AddChild(subtitle);
        subtitle.AddChild(Label("选择要启用调整的卡牌。关闭开关时，保持当前游戏版本的原版效果。", 20, StsColors.lightGray));
        subtitle.AddChild(Spacer());
        var restart = Label("保存后游戏会自动退出，重启后生效", 20, StsColors.gold);
        restart.AddThemeStyleboxOverride("normal", Box("332714CC", "A67F2C", 1, 14));
        subtitle.AddChild(restart);

        var tabs = new HBoxContainer();
        tabs.AddThemeConstantOverride("separation", 12);
        content.AddChild(tabs);
        AddTab(tabs, CardCategory.All, "全部");
        AddTab(tabs, CardCategory.Ironclad, "铁甲战士");
        AddTab(tabs, CardCategory.Silent, "静默猎手");
        AddTab(tabs, CardCategory.Defect, "故障机器人");
        AddTab(tabs, CardCategory.Regent, "储君");
        AddTab(tabs, CardCategory.Necrobinder, "亡灵契约师");
        AddTab(tabs, CardCategory.Other, "其他");

        var separator = new ColorRect
        {
            Color = new Color("913E3E"),
            CustomMinimumSize = new Vector2(0f, 4f)
        };
        content.AddChild(separator);

        var tablePanel = new PanelContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        tablePanel.AddThemeStyleboxOverride("panel", Box("11131BE8", "6B5A38", 1, 18));
        content.AddChild(tablePanel);

        var tableMargin = new MarginContainer();
        tableMargin.AddThemeConstantOverride("margin_left", 18);
        tableMargin.AddThemeConstantOverride("margin_top", 16);
        tableMargin.AddThemeConstantOverride("margin_right", 18);
        tableMargin.AddThemeConstantOverride("margin_bottom", 14);
        tablePanel.AddChild(tableMargin);

        var tableContent = new VBoxContainer();
        tableContent.AddThemeConstantOverride("separation", 5);
        tableMargin.AddChild(tableContent);
        tableContent.AddChild(BuildTableHeader());

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        tableContent.AddChild(scroll);
        _tableRows = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _tableRows.AddThemeConstantOverride("separation", 4);
        scroll.AddChild(_tableRows);

        var footer = new HBoxContainer();
        footer.Alignment = BoxContainer.AlignmentMode.End;
        footer.AddThemeConstantOverride("separation", 20);
        content.AddChild(footer);
        footer.AddChild(ActionButton("恢复默认", ResetDefaults, false));
        footer.AddChild(ActionButton("取消", Cancel, false));
        footer.AddChild(ActionButton("保存并退出", SaveAndExit, true));
    }

    private Control BuildTableHeader()
    {
        var header = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        header.AddThemeConstantOverride("separation", 8);
        header.AddChild(PaddedCell("卡牌名称", CardNameWidth, StsColors.gold, 21, HeaderRowHeight, CardNameLeftPadding));
        header.AddChild(Cell("调整内容", AdjustmentWidth, StsColors.gold, 21, HeaderRowHeight));
        header.AddChild(Cell("升级前效果", EffectWidth, StsColors.gold, 21, HeaderRowHeight));
        header.AddChild(Cell("升级后效果", EffectWidth, StsColors.gold, 21, HeaderRowHeight));
        header.AddChild(Spacer());
        header.AddChild(Cell("开关", ToggleWidth, StsColors.gold, 21, HeaderRowHeight));
        return header;
    }

    private void AddTab(HBoxContainer tabs, CardCategory category, string text)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(category == CardCategory.All ? 118f : 148f, 58f)
        };
        button.AddThemeFontSizeOverride("font_size", 22);
        button.Pressed += () => SelectCategory(category);
        tabs.AddChild(button);
        _tabButtons[category] = button;
        UpdateTabStyle(category);
    }

    private void SelectCategory(CardCategory category)
    {
        _selectedCategory = category;
        foreach (CardCategory item in _tabButtons.Keys)
            UpdateTabStyle(item);
        RenderRows();
    }

    private void UpdateTabStyle(CardCategory category)
    {
        Button button = _tabButtons[category];
        bool active = category == _selectedCategory;
        button.AddThemeStyleboxOverride("normal", Box(active ? "475A54" : "4A484B", active ? "D8BD72" : "858585", active ? 2 : 1, 12));
        button.AddThemeStyleboxOverride("hover", Box("5B6763", "EFC851", 2, 12));
        button.AddThemeStyleboxOverride("pressed", Box("303E3A", "EFC851", 2, 12));
        button.AddThemeColorOverride("font_color", active ? StsColors.cream : StsColors.lightGray);
    }

    private void RenderRows()
    {
        if (_tableRows == null)
            return;

        foreach (Node child in _tableRows.GetChildren())
            child.QueueFree();
        _toggleButtons.Clear();

        if (_selectedCategory == CardCategory.All)
        {
            foreach (CardCategory category in _categoryOrder)
            {
                List<CardBalanceEntry> rows = _entries.Where(entry => entry.Category == category).ToList();
                if (rows.Count == 0)
                    continue;
                _tableRows.AddChild(BuildCategoryHeader(category, rows.Count));
                foreach (CardBalanceEntry entry in rows)
                    _tableRows.AddChild(BuildEntryRow(entry));
            }
            return;
        }

        List<CardBalanceEntry> filtered = _entries.Where(entry => entry.Category == _selectedCategory).ToList();
        if (filtered.Count == 0)
        {
            var empty = Label("这个角色暂时没有调整的卡牌", 22, StsColors.lightGray);
            empty.HorizontalAlignment = HorizontalAlignment.Center;
            empty.CustomMinimumSize = new Vector2(0f, 120f);
            empty.VerticalAlignment = VerticalAlignment.Center;
            _tableRows.AddChild(empty);
            return;
        }

        _tableRows.AddChild(BuildCategoryHeader(_selectedCategory, filtered.Count));
        foreach (CardBalanceEntry entry in filtered)
            _tableRows.AddChild(BuildEntryRow(entry));
    }

    private Control BuildCategoryHeader(CardCategory category, int count)
    {
        var row = new HBoxContainer { CustomMinimumSize = new Vector2(0f, 34f) };
        string text = $"{CategoryName(category)}  {count} 张";
        row.AddChild(Label(text, 22, CategoryColor(category)));
        return row;
    }

    private Control BuildEntryRow(CardBalanceEntry entry)
    {
        var panel = new PanelContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        panel.AddThemeStyleboxOverride("panel", Box("151720E8", "2D3038", 1, 8));
        var row = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 8);
        panel.AddChild(row);
        row.AddChild(PaddedCell(entry.Name, CardNameWidth, CategoryColor(entry.Category), 20, EntryRowHeight, CardNameLeftPadding));
        row.AddChild(Cell(entry.Adjustment, AdjustmentWidth, StsColors.cream, 17, EntryRowHeight));
        row.AddChild(Cell(entry.BaseEffect, EffectWidth, StsColors.cream, 16, EntryRowHeight, true));
        row.AddChild(Cell(entry.UpgradedEffect, EffectWidth, StsColors.cream, 16, EntryRowHeight, true));
        row.AddChild(Spacer());

        var toggle = new CheckButton
        {
            ButtonPressed = _draft[entry.Id],
            Text = _draft[entry.Id] ? "启用" : "关闭",
            CustomMinimumSize = new Vector2(ToggleWidth, EntryRowHeight)
        };
        toggle.AddThemeFontSizeOverride("font_size", 19);
        toggle.Toggled += enabled =>
        {
            _draft[entry.Id] = enabled;
            toggle.Text = enabled ? "启用" : "关闭";
            toggle.AddThemeColorOverride("font_color", enabled ? StsColors.cream : StsColors.lightGray);
        };
        toggle.AddThemeColorOverride("font_color", toggle.ButtonPressed ? StsColors.cream : StsColors.lightGray);
        row.AddChild(toggle);
        _toggleButtons[entry.Id] = toggle;
        return panel;
    }

    private static Label Cell(string text, float width, Color color, int fontSize, float height, bool wrap = false)
    {
        var label = Label(text, fontSize, color);
        label.CustomMinimumSize = new Vector2(width, height);
        label.AutowrapMode = wrap ? TextServer.AutowrapMode.WordSmart : TextServer.AutowrapMode.Off;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.ClipText = !wrap;
        return label;
    }

    private static Control PaddedCell(string text, float width, Color color, int fontSize, float height, float leftPadding)
    {
        var margin = new MarginContainer
        {
            CustomMinimumSize = new Vector2(width, height)
        };
        margin.AddThemeConstantOverride("margin_left", (int)leftPadding);
        margin.AddChild(Cell(text, Math.Max(0f, width - leftPadding), color, fontSize, height));
        return margin;
    }

    private static Label Label(string text, int fontSize, Color color)
    {
        var label = new Label { Text = text };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private static Control Spacer()
    {
        return new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill };
    }

    private static Button ActionButton(string text, Action action, bool primary)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(primary ? 230f : 176f, 56f)
        };
        button.AddThemeFontSizeOverride("font_size", 24);
        button.AddThemeColorOverride("font_color", StsColors.cream);
        button.AddThemeStyleboxOverride("normal", Box(primary ? "24515C" : "24444C", primary ? "EFC851" : "4A8794", primary ? 3 : 2, 12));
        button.AddThemeStyleboxOverride("hover", Box("2D6571", "EFC851", 3, 12));
        button.AddThemeStyleboxOverride("pressed", Box("193C44", "EFC851", 2, 12));
        button.Pressed += action;
        return button;
    }

    private static StyleBoxFlat Box(string background, string border, int borderWidth, int cornerRadius)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(background),
            BorderColor = new Color(border),
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = cornerRadius,
            CornerRadiusTopRight = cornerRadius,
            CornerRadiusBottomLeft = cornerRadius,
            CornerRadiusBottomRight = cornerRadius
        };
    }

    private static string CategoryName(CardCategory category)
    {
        return category switch
        {
            CardCategory.Ironclad => "铁甲战士",
            CardCategory.Silent => "静默猎手",
            CardCategory.Defect => "故障机器人",
            CardCategory.Regent => "储君",
            CardCategory.Necrobinder => "亡灵契约师",
            CardCategory.Other => "其他",
            _ => "全部"
        };
    }

    private static Color CategoryColor(CardCategory category)
    {
        return category switch
        {
            CardCategory.Ironclad => StsColors.red,
            CardCategory.Silent => StsColors.green,
            CardCategory.Defect => StsColors.blue,
            CardCategory.Regent => StsColors.orange,
            CardCategory.Necrobinder => StsColors.purple,
            _ => StsColors.lightGray
        };
    }

    private void ResetDefaults()
    {
        _draft = ModConfiguration.CreateDefaults();
        RenderRows();
    }

    private void Cancel()
    {
        _draft = ModConfiguration.CreateDraft();
        SetPanelVisible(false);
        RenderRows();
    }

    private void SaveAndExit()
    {
        ModConfiguration.Save(_draft);
        NGame.Instance?.Quit();
    }

    private void SetPanelVisible(bool visible)
    {
        if (visible)
            EnsureUiBuilt();

        Visible = visible;
        MouseFilter = visible ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
        if (visible)
        {
            _draft = ModConfiguration.CreateDraft();
            RenderRows();
            _mainMenu?.EnableBackstop();
            MoveToFront();
        }
        else if (_mainMenu?.SubmenuStack.SubmenusOpen == false)
        {
            _mainMenu.DisableBackstop();
        }
    }
}
