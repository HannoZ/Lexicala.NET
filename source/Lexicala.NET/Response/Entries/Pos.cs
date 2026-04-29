namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a flexible JSON shape for part-of-speech values.
    /// </summary>
    /// <remarks>
    /// Lexicala may return a single POS string or an array of strings.
    /// </remarks>
    public struct Pos
    {
        /// <summary>
        /// Gets or sets a single part-of-speech value.
        /// </summary>
        public string PartOfSpeech;

        /// <summary>
        /// Gets or sets multiple part-of-speech values.
        /// </summary>
        public string[] PartOfSpeechArray;

        /// <summary>
        /// Converts a single part-of-speech value into the wrapper.
        /// </summary>
        public static implicit operator Pos(string pos) => new Pos { PartOfSpeech = pos };

        /// <summary>
        /// Converts multiple part-of-speech values into the wrapper.
        /// </summary>
        public static implicit operator Pos(string[] posArray) => new Pos { PartOfSpeechArray = posArray };

    }
}
