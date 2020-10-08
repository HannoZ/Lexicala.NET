namespace Lexicala.NET.Response.Search
{
    public struct HeadwordObject
    {
        public Headword Headword;
        public Headword[] HeadwordElementArray;

        public static implicit operator HeadwordObject(Headword headword) => new HeadwordObject { Headword = headword };
        public static implicit operator HeadwordObject(Headword[] headwordElementArray) => new HeadwordObject { HeadwordElementArray = headwordElementArray };
    }
}