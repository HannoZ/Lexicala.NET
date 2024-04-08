namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct TranslationObject
    {
        public Translation Translation;
        public Translation[] Translations;

        public static implicit operator TranslationObject(Translation translation) => new() { Translation = translation };
        public static implicit operator TranslationObject(Translation[] translationObjectArray) => new() { Translations = translationObjectArray };
    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}