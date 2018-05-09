using Lotd.FileFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class LocalizedText
    {
        public string Universal { get; set; }

        private Language lastLanguageSet = Language.Unknown;

        private string english;
        public string English
        {
            get { return english; }
            set
            {
                english = value;
                lastLanguageSet = Language.English;
            }
        }

        private string french;
        public string French
        {
            get { return french; }
            set
            {
                french = value;
                lastLanguageSet = Language.French;
            }
        }

        private string german;
        public string German
        {
            get { return german; }
            set
            {
                german = value;
                lastLanguageSet = Language.German;
            }
        }

        private string italian;
        public string Italian
        {
            get { return italian; }
            set
            {
                italian = value;
                lastLanguageSet = Language.Italian;
            }
        }

        private string spanish;
        public string Spanish
        {
            get { return spanish; }
            set
            {
                spanish = value;
                lastLanguageSet = Language.Spanish;
            }
        }

        public void SetText(Language language, string value)
        {
            switch (language)
            {
                case Language.English: English = value; break;
                case Language.French: French = value; break;
                case Language.German: German = value; break;
                case Language.Italian: Italian = value; break;
                case Language.Spanish: Spanish = value; break;
                default: Universal = value; break;
            }
        }

        public string GetText(Language language)
        {
            switch (language)
            {
                case Language.English: return English;
                case Language.French: return French;
                case Language.German: return German;
                case Language.Italian: return Italian;
                case Language.Spanish: return Spanish;
                default: return Universal;
            }
        }

        public override string ToString()
        {
            return English != null ? English : GetText(lastLanguageSet);
        }
    }

    public class LocalizedOffset
    {
        public long Universal { get; set; }

        public long English { get; set; }
        public long French { get; set; }
        public long German { get; set; }
        public long Italian { get; set; }
        public long Spanish { get; set; }

        public LocalizedOffset()
        {
            Universal = -1;
        }

        public void SetValue(Language language, long value)
        {
            switch (language)
            {
                case Language.English: English = value; break;
                case Language.French: French = value; break;
                case Language.German: German = value; break;
                case Language.Italian: Italian = value; break;
                case Language.Spanish: Spanish = value; break;
                default: Universal = value; break;
            }
        }

        public long GetValue(Language language)
        {
            switch (language)
            {
                case Language.English: return English;
                case Language.French: return French;
                case Language.German: return German;
                case Language.Italian: return Italian;
                case Language.Spanish: return Spanish;
                default: return Universal;
            }
        }
    }

    public enum Language
    {
        Unknown,

        English,
        French,
        German,
        Italian,
        Spanish
    }
}
