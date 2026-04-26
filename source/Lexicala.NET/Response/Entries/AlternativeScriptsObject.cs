namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a flexible JSON shape for alternative scripts.
    /// </summary>
    /// <remarks>
    /// Lexicala can return either a single object or an array for this field.
    /// This union-like struct preserves both possibilities for custom converters.
    /// </remarks>
    public struct AlternativeScriptsObject
    {
        /// <summary>
        /// Gets or sets a single alternative script object.
        /// </summary>
        public AlternativeScripts AlternativeScripts;

        /// <summary>
        /// Gets or sets multiple alternative script objects.
        /// </summary>
        public AlternativeScripts[] AlternativeScriptsArray;

        /// <summary>
        /// Converts a single <see cref="AlternativeScripts"/> value into the wrapper.
        /// </summary>
        public static implicit operator AlternativeScriptsObject(AlternativeScripts alternativeScripts) => new() { AlternativeScripts = alternativeScripts };

        /// <summary>
        /// Converts an array of <see cref="AlternativeScripts"/> values into the wrapper.
        /// </summary>
        public static implicit operator AlternativeScriptsObject(AlternativeScripts[] alternativeScriptsArray) => new() { AlternativeScriptsArray = alternativeScriptsArray };
    }
}
