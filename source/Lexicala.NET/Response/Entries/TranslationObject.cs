namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a flexible JSON shape for translation values.
    /// </summary>
    /// <remarks>
    /// Lexicala translation buckets can contain either a single translation object
    /// or an array of translations.
    /// </remarks>
    public struct TranslationObject
    {
        /// <summary>
        /// Gets or sets a single translation object.
        /// </summary>
        public Translation Translation;

        /// <summary>
        /// Gets or sets multiple translation objects.
        /// </summary>
        public Translation[] Translations;

        /// <summary>
        /// Converts a single <see cref="Translation"/> into the wrapper.
        /// </summary>
        public static implicit operator TranslationObject(Translation translation) => new() { Translation = translation };

        /// <summary>
        /// Converts multiple <see cref="Translation"/> values into the wrapper.
        /// </summary>
        public static implicit operator TranslationObject(Translation[] translationObjectArray) => new() { Translations = translationObjectArray };
    }
}
