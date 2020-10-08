namespace Lexicala.NET.Response.Entries
{
    public struct PronunciationObject
    {
        public Pronunciation Pronunciation;
        public Pronunciation[] PronunciationArray;

        public static implicit operator PronunciationObject(Pronunciation pronunciation) => new PronunciationObject { Pronunciation = pronunciation };
        public static implicit operator PronunciationObject(Pronunciation[] pronunciationArray) => new PronunciationObject { PronunciationArray = pronunciationArray };
    }
}