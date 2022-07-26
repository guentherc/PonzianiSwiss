using System;
using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace PonzianiSwiss.Resources
{
    public class LocalizedStrings
    {
        private LocalizedStrings()
        {

        }

        public static LocalizedStrings Instance { get; } = new LocalizedStrings();

        public void SetCulture(string cultureCode)
        {
            var newCulture = new CultureInfo(cultureCode);
            LocalizeDictionary.Instance.Culture = newCulture;
        }

        public string Get(string key, params object?[] parameter)
        {
            string template = (string)LocalizeDictionary.Instance.GetLocalizedObject("PonzianiSwiss", "PonzianiSwiss.Resources.Strings", key, LocalizeDictionary.Instance.Culture);
            return String.Format(template, parameter);
        }

        public string this[string key]
        {
            get
            {
                var result = LocalizeDictionary.Instance.GetLocalizedObject("PonzianiSwiss", "PonzianiSwiss.Resources.Strings", key, LocalizeDictionary.Instance.Culture);
                return result as string ?? string.Empty;
            }
        }
    }
}
