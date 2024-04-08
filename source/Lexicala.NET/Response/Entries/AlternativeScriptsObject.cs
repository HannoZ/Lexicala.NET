namespace Lexicala.NET.Response.Entries
{
    public struct AlternativeScriptsObject   
    {
        public AlternativeScripts AlternativeScripts;
        public AlternativeScripts[] AlternativeScriptsArray;

        public static implicit operator AlternativeScriptsObject(AlternativeScripts alternativeScripts) => new() { AlternativeScripts = alternativeScripts };
        public static implicit operator AlternativeScriptsObject(AlternativeScripts[] alternativeScriptsArray) => new() { AlternativeScriptsArray = alternativeScriptsArray };
    }
}