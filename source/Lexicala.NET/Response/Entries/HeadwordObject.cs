namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct HeadwordObject
    {
        public Headword Headword;
        public Headword[] HeadwordElementArray;

        public static implicit operator HeadwordObject(Headword headword) => new HeadwordObject { Headword = headword };
        public static implicit operator HeadwordObject(Headword[] headwordElementArray) => new HeadwordObject { HeadwordElementArray = headwordElementArray };
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}