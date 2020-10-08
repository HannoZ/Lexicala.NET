namespace Lexicala.NET.Response.Entries
{
    public struct Pos
    {
        public string PartOfSpeech;
        public string[] PartOfSpeechArray;

        public static implicit operator Pos(string pos) => new Pos { PartOfSpeech = pos };
        public static implicit operator Pos(string[] posArray) => new Pos { PartOfSpeechArray = posArray };

    }
}