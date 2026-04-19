namespace Lexicala.NET
{
    /// <summary>
    /// Contains API endpoint constants for the Lexicala service.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Test endpoint path.
        /// </summary>
        internal const string Test = "/test";

        /// <summary>
        /// Basic search endpoint path.
        /// </summary>
        internal const string Search = "/search";

        /// <summary>
        /// Search entries endpoint path.
        /// </summary>
        internal const string SearchEntries = "/search-entries";

        /// <summary>
        /// Entries endpoint path.
        /// </summary>
        internal const string Entries = "/entries";

        /// <summary>
        /// Search RDF endpoint path.
        /// </summary>
        internal const string SearchRdf = "/search-rdf";

        /// <summary>
        /// RDF endpoint path.
        /// </summary>
        internal const string Rdf = "/rdf";

        /// <summary>
        /// Languages endpoint path.
        /// </summary>
        internal const string Languages = "/languages";

        /// <summary>
        /// Senses endpoint path.
        /// </summary>
        internal const string Senses = "/senses";

        /// <summary>
        /// Search definitions endpoint path.
        /// </summary>
        internal const string SearchDefinitions = "/search-definitions";

        /// <summary>
        /// Fluky search endpoint path (random word discovery).
        /// </summary>
        internal const string FlukySearch = "/fluky-search";
    }
}