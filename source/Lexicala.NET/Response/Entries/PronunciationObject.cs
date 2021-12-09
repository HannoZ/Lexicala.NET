namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct PronunciationObject
    {
        public Pronunciation Pronunciation;
        public Pronunciation[] PronunciationArray;

        public static implicit operator PronunciationObject(Pronunciation pronunciation) => new PronunciationObject { Pronunciation = pronunciation };
        public static implicit operator PronunciationObject(Pronunciation[] pronunciationArray) => new PronunciationObject { PronunciationArray = pronunciationArray };
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}