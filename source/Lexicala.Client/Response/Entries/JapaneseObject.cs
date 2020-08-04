namespace Lexicala.NET.Client.Response.Entries
{
    public struct JapaneseObject
    {
        public Japanese Ja;
        public Japanese[] JaArray;

        public static implicit operator JapaneseObject(Japanese ja) => new JapaneseObject { Ja = ja };
        public static implicit operator JapaneseObject(Japanese[] jaArray) => new JapaneseObject { JaArray = jaArray };
    }
}