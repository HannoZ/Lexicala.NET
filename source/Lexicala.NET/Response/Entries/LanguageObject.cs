namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct LanguageObject
    {
        public Language Language;
        public Language[] Languages;

        public static implicit operator LanguageObject(Language language) => new LanguageObject { Language = language };
        public static implicit operator LanguageObject(Language[] languageObjectArray) => new LanguageObject { Languages = languageObjectArray };
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}