namespace Lexicala.NET.Client.Response.Entries
{
    public struct CommonLanguageObject
    {
        public CommonLanguage CommonLanguage;
        public CommonLanguage[] CommonLanguageObjectArray;

        public static implicit operator CommonLanguageObject(CommonLanguage commonLanguage) => new CommonLanguageObject { CommonLanguage = commonLanguage };
        public static implicit operator CommonLanguageObject(CommonLanguage[] languageObjectArray) => new CommonLanguageObject { CommonLanguageObjectArray = languageObjectArray };
    }
}