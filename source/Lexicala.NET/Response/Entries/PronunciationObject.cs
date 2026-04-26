namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a flexible JSON shape for pronunciation values.
    /// </summary>
    /// <remarks>
    /// Lexicala may return pronunciation as either a single object or an array.
    /// </remarks>
    public struct PronunciationObject
    {
        /// <summary>
        /// Gets or sets a single pronunciation object.
        /// </summary>
        public Pronunciation Pronunciation;

        /// <summary>
        /// Gets or sets multiple pronunciation objects.
        /// </summary>
        public Pronunciation[] PronunciationArray;

        /// <summary>
        /// Converts a single <see cref="Pronunciation"/> into the wrapper.
        /// </summary>
        public static implicit operator PronunciationObject(Pronunciation pronunciation) => new PronunciationObject { Pronunciation = pronunciation };

        /// <summary>
        /// Converts multiple <see cref="Pronunciation"/> values into the wrapper.
        /// </summary>
        public static implicit operator PronunciationObject(Pronunciation[] pronunciationArray) => new PronunciationObject { PronunciationArray = pronunciationArray };
    }
}
