namespace Lexicala.NET.Parsing.Dto
{
    /// <summary>
    /// Represents a translation. 
    /// </summary>
    public class Translation
    {
        /// <summary>
        /// Gets or sets the text (ie. the translation of a word / sentence)
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Gets or sets the translation language code.
        /// </summary>
        public string Language { get; set; }
    }
}
