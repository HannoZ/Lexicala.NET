namespace Lexicala.NET.Response.Entries
{
    public struct LanguageObject
    {
        public Language Language;
        public Language[] CommonLanguageObjectArray;

        public static implicit operator LanguageObject(Language language) => new LanguageObject { Language = language };
        public static implicit operator LanguageObject(Language[] languageObjectArray) => new LanguageObject { CommonLanguageObjectArray = languageObjectArray };
    }
}