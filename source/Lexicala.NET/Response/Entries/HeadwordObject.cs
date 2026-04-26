namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a flexible JSON shape for headword data.
    /// </summary>
    /// <remarks>
    /// Lexicala may return <c>headword</c> as either a single object or an array.
    /// </remarks>
    public struct HeadwordObject
    {
        /// <summary>
        /// Gets or sets a single headword object.
        /// </summary>
        public Headword Headword;

        /// <summary>
        /// Gets or sets multiple headword objects.
        /// </summary>
        public Headword[] HeadwordElementArray;

        /// <summary>
        /// Converts a single <see cref="Headword"/> into the wrapper.
        /// </summary>
        public static implicit operator HeadwordObject(Headword headword) => new HeadwordObject { Headword = headword };

        /// <summary>
        /// Converts an array of <see cref="Headword"/> values into the wrapper.
        /// </summary>
        public static implicit operator HeadwordObject(Headword[] headwordElementArray) => new HeadwordObject { HeadwordElementArray = headwordElementArray };
    }
}
