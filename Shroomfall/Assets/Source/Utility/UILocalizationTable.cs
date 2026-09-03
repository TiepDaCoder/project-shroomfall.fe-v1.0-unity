using Assets.Source.Core;
using Assets.Source.Service;
using Contract;
using System.Collections.Generic;

namespace Assets.Source.Utility
{
    public static class UILocalizationTable
    {
        private static Dictionary<string, Dictionary<string, string>> table = new();

        #region Methods
        public static string Get(
            string key)
        {
            if (table.TryGetValue(key, out var localized))
            {
                var settingService = ServiceProvider.Get<SettingService>();

                if (settingService != null && localized.TryGetValue(settingService.GetCurrentLocale(), out var value))
                    return value;

                if (localized.TryGetValue(Constraint.DEFAULT_LOCALE, out var fallback))
                    return fallback;
            }

            return key;
        }
        #endregion
    }
}
