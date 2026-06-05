using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace CustomCardBalance;

public static class CardIds
{
    public const string Dominate = "dominate";
    public const string ExpectAFight = "expect_a_fight";
    public const string ForgottenRitual = "forgotten_ritual";
    public const string Acrobatics = "acrobatics";
    public const string Untouchable = "untouchable";
    public const string Anticipate = "anticipate";
    public const string Speedster = "speedster";
    public const string Murder = "murder";
    public const string WraithForm = "wraith_form";
    public const string Voltaic = "voltaic";
    public const string Rainbow = "rainbow";
    public const string Glow = "glow";
    public const string Alignment = "alignment";
    public const string VoidForm = "void_form";
    public const string TheSealedThrone = "the_sealed_throne";
    public const string BansheesCry = "banshees_cry";
    public const string Dirge = "dirge";
    public const string Seance = "seance";
    public const string BorrowedTime = "borrowed_time";
    public const string Defy = "defy";
    public const string Production = "production";

    public static readonly string[] All =
    {
        Dominate,
        ExpectAFight,
        ForgottenRitual,
        Acrobatics,
        Untouchable,
        Anticipate,
        Speedster,
        Murder,
        WraithForm,
        Voltaic,
        Rainbow,
        Glow,
        Alignment,
        VoidForm,
        TheSealedThrone,
        BansheesCry,
        Dirge,
        Seance,
        BorrowedTime,
        Defy,
        Production
    };
}

public sealed class CardBalanceSave
{
    public Dictionary<string, bool> Cards { get; set; } = new();
}

public static class ModConfiguration
{
    private static readonly Dictionary<string, bool> _enabledCards = CreateDefaults();

    public static IReadOnlyDictionary<string, bool> EnabledCards => _enabledCards;

    public static string SettingsPath =>
        Path.Combine(OS.GetUserDataDir(), "CustomCardBalance", "settings.json");

    public static string LegacySettingsPath =>
        Path.Combine(OS.GetUserDataDir(), "RevertCardsMod", "settings.json");

    public static bool IsEnabled(string cardId)
    {
        return !_enabledCards.TryGetValue(cardId, out bool enabled) || enabled;
    }

    public static Dictionary<string, bool> CreateDraft()
    {
        return new Dictionary<string, bool>(_enabledCards);
    }

    public static Dictionary<string, bool> CreateDefaults()
    {
        var defaults = new Dictionary<string, bool>();
        foreach (string cardId in CardIds.All)
            defaults[cardId] = true;
        return defaults;
    }

    public static void Load()
    {
        _enabledCards.Clear();
        foreach ((string cardId, bool enabled) in CreateDefaults())
            _enabledCards[cardId] = enabled;

        try
        {
            MigrateLegacySettingsIfNeeded();
            if (!File.Exists(SettingsPath))
            {
                Log.Info($"[CustomCardBalance] No settings file found. Using defaults: {SettingsPath}");
                return;
            }

            CardBalanceSave? save = JsonSerializer.Deserialize<CardBalanceSave>(File.ReadAllText(SettingsPath));
            if (save?.Cards == null)
                return;

            foreach ((string cardId, bool enabled) in save.Cards)
            {
                if (_enabledCards.ContainsKey(cardId))
                    _enabledCards[cardId] = enabled;
            }
            Log.Info($"[CustomCardBalance] Loaded card settings from {SettingsPath}");
        }
        catch (Exception ex)
        {
            Log.Error($"[CustomCardBalance] Failed to load settings. Using defaults.\n{ex}");
        }
    }

    public static void Save(IReadOnlyDictionary<string, bool> cards)
    {
        var normalized = CreateDefaults();
        foreach ((string cardId, bool enabled) in cards)
        {
            if (normalized.ContainsKey(cardId))
                normalized[cardId] = enabled;
        }

        string? directory = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new CardBalanceSave { Cards = normalized }, options));
        Log.Info($"[CustomCardBalance] Saved card settings to {SettingsPath}");
    }

    private static void MigrateLegacySettingsIfNeeded()
    {
        if (File.Exists(SettingsPath) || !File.Exists(LegacySettingsPath))
            return;

        string? directory = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.Copy(LegacySettingsPath, SettingsPath);
        Log.Info($"[CustomCardBalance] Migrated legacy settings from {LegacySettingsPath} to {SettingsPath}");
    }
}
