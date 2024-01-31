﻿using ECommons.DalamudServices;

namespace RotationSolver.Localization;

internal static class LocalizationManager
{
    private static Dictionary<string, string> _rightLang = [];

    private static readonly Dictionary<string, Dictionary<string, string>> _translations = [];

    public static string Local(this string key, string @default)
    {
        if (_rightLang.TryGetValue(key, out var value)) return value;

#if DEBUG
        _rightLang[key] = @default;
#endif
        return @default;
    }


    public static void InIt()
    {
        SetLanguage(Svc.PluginInterface.UiLanguage);
        Svc.PluginInterface.LanguageChanged += OnLanguageChange;
#if DEBUG
        ExportLocalization();
#endif
    }

    private static async void SetLanguage(string lang)
    {
        if (_translations.TryGetValue(lang, out var value))
        {
            _rightLang = value;
        }
        else
        {
            try
            {
                var url = $"https://raw.githubusercontent.com/{Service.USERNAME}/{Service.REPO}/main/RotationSolver/Localization/{lang}.json";
                using var client = new HttpClient();
                _rightLang = _translations[lang] = JsonConvert.DeserializeObject<Dictionary<string, string>>(await client.GetStringAsync(url))!;
            }
            catch (HttpRequestException ex) when (ex?.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Svc.Log.Information(ex, $"No language {lang}");
                _rightLang = [];
            }
            catch (Exception ex)
            {
                Svc.Log.Warning(ex, $"Failed to download the language {lang}");
                _rightLang = [];
            }
        }

        RotationSolverPlugin.ChangeUITranslation();
    }

#if DEBUG
    private static void ExportLocalization()
    {
        //TODO: related path.
        var directory = @"E:\OneDrive - stu.zafu.edu.cn\PartTime\FFXIV\RotationSolver\RotationSolver\Localization";
        if (!Directory.Exists(directory)) return;

        if (Svc.PluginInterface.UiLanguage != "en") return;

        //Default values.
        var path = Path.Combine(directory, "Localization.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(_rightLang, Formatting.Indented));

        Svc.Log.Info("Exported the json file");
    }
#endif

    public static void Dispose()
    {
        Svc.PluginInterface.LanguageChanged -= OnLanguageChange;
    }

    private static void OnLanguageChange(string languageCode)
    {
        try
        {
            Svc.Log.Information($"Loading Localization for {languageCode}");
            SetLanguage(languageCode);
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Unable to Load Localization");
        }
    }
}
