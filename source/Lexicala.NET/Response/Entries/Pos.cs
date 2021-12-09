namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct Pos
    {
        public string PartOfSpeech;
        public string[] PartOfSpeechArray;

        public static implicit operator Pos(string pos) => new Pos { PartOfSpeech = pos };
        public static implicit operator Pos(string[] posArray) => new Pos { PartOfSpeechArray = posArray };

    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}