namespace Lexicala.NET.Response.Search
{
    /// <summary>
    /// Represents a polymorphic headword value that can be either a single headword or an array.
    /// </summary>
    public struct HeadwordObject
    {
        /// <summary>
        /// A single headword value.
        /// </summary>
        public Headword Headword;

        /// <summary>
        /// A list of headword values.
        /// </summary>
        public Headword[] HeadwordElementArray;

        /// <summary>
        /// Converts a <see cref="Headword"/> to a <see cref="HeadwordObject"/>.
        /// </summary>
        /// <param name="headword">The source headword.</param>
        public static implicit operator HeadwordObject(Headword headword) => new HeadwordObject { Headword = headword };

        /// <summary>
        /// Converts an array of <see cref="Headword"/> values to a <see cref="HeadwordObject"/>.
        /// </summary>
        /// <param name="headwordElementArray">The source headword array.</param>
        public static implicit operator HeadwordObject(Headword[] headwordElementArray) => new HeadwordObject { HeadwordElementArray = headwordElementArray };
    }
}
