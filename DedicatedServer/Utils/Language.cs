using DedicatedServer.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DedicatedServer.Utils
{
    internal static class Language
    {
        private static readonly Dictionary<string, LocalizedContentManager.LanguageCode> LanguageCodes = new()
        {
            ["en"] = LocalizedContentManager.LanguageCode.en, // English,
            ["ja"] = LocalizedContentManager.LanguageCode.ja, // Japanese,
            ["ru"] = LocalizedContentManager.LanguageCode.ru, // Russian,
            ["zh"] = LocalizedContentManager.LanguageCode.zh, // Chinese,
            ["pt"] = LocalizedContentManager.LanguageCode.pt, // Portuguese,
            ["es"] = LocalizedContentManager.LanguageCode.es, // Spanish,
            ["de"] = LocalizedContentManager.LanguageCode.de, // German,
            ["th"] = LocalizedContentManager.LanguageCode.th, // Thai,
            ["fr"] = LocalizedContentManager.LanguageCode.fr, // French,
            ["ko"] = LocalizedContentManager.LanguageCode.ko, // Korean,
            ["it"] = LocalizedContentManager.LanguageCode.it, // Italian,
            ["tr"] = LocalizedContentManager.LanguageCode.tr, // Turkish
            ["hu"] = LocalizedContentManager.LanguageCode.hu, // Hungarian,
            ["mod"] = LocalizedContentManager.LanguageCode.mod, // A custom language added by a mod.
        };

        public static LocalizedContentManager.LanguageCode GetLanguage()
            => LocalizedContentManager.CurrentLanguageCode;

        /// <summary>
        /// This functions needs to be called in the <see cref="StardewValley.Menus.TitleMenu"/>
        /// </summary>
        public static void ChangeLanguage(IMonitor monitor, ModConfig config)
        {
            if (null == config.Language)
            {
                monitor?.Log($"The language has not been changed", LogLevel.Info);
                return;
            }

            if (Game1.activeClickableMenu is not TitleMenu)
            {
                throw new Exception("This functions needs to be called in StardewValley.Menus.TitleMenu");
            }

            if (LanguageCodes.TryGetValue(config.Language, out var languageCode))
            {
                if (LocalizedContentManager.LanguageCode.mod == languageCode)
                {
                    monitor?.Log("\"mod\" was selected as the language; no changes will be made to the language.");
                }
                else
                {
                    LocalizedContentManager.CurrentLanguageCode = languageCode;

                    CultureInfo culture = new CultureInfo(config.Language);
                    monitor?.Log($"The language has been changed to {culture.EnglishName}", LogLevel.Info);
                }
            }
            else
            {
                monitor?.Log($"The correct language was not found ({config.Language}).", LogLevel.Info);
            }
        }
    }
}
