namespace Lexicala.NET.Client.Request
{
    public class AdvancedSearchRequest
    {
        public string ETag { get; set; }
        /// <summary>
        /// Specify which resource to look in. (See <see cref="Sources"/>).
        /// </summary>
        public string Source { get; set; } = Sources.Global;
        /// <summary>
        /// Specify a headword.
        /// </summary>
        public string SearchText { get; set; }
        /// <summary>
        /// Specify which source language to look in.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Setting this to <c>true</c> looks for inflected forms by applying the stemmer.
        /// </summary>
        public bool Analyzed { get; set; }
        /// <summary>
        ///  Find single sense entries only.
        /// </summary>
        public bool Monosemous { get; set; }
        /// <summary>
        /// Setting this to <c>true</c> looks for all inflected forms (as well as headwords) contained both in KD data and in the external morphological lists.
        /// </summary>
        public bool Morph { get; set; }
        /// <summary>
        /// Find multiple sense entries only.
        /// </summary>
        public bool Polysemous { get; set; }

        /// <summary>
        /// Specify grammatical gender ( = masculine, feminine, ...).
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Specify grammatical number  ( = singular, plural, ...).
        /// </summary>
        public string Number{ get; set; }
        /// <summary>
        /// Specify part of speech ( = noun, verb, ...).
        /// </summary>
        public string Pos { get; set; }
        /// <summary>
        /// Specify subcategorization ( = masculine, feminine, ...).
        /// </summary>
        public string Subcategorization { get; set; }

        /// <summary>
        /// Specify the page number out of available_n_pages, in order to navigate between pages.
        /// The default value is 1.
        /// </summary>
        public int Page { get; set; } = 1;
        /// <summary>
        /// Specify how many results appear per page.
        /// </summary>
        public int PageLength { get; set; } = 10;
        /// <summary>
        /// Specify the number of randomly-sampled results to return.
        /// </summary>
        public int Sample { get; set; }
    }
}
