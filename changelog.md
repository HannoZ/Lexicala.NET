# Change log 
## Lexicala.NET
1.0 The Client and Parser project have been merged into a single project and Nuget package. 
- `Lexicala.NET.Client` namespaces are changed to `Lexicala.NET`
- `Lexicala.NET.Parser` namespaces are changed to `Lexicala.NET.Parsing`

**DEPRECATED PACKAGES**
## Lexicala.NET.Client
1.0.0 - Initial version: basic `/search` and `/entries` api implementation

1.1.0 - Added new api implementations: `/test`, `/me` and `/languages`

1.2.0 - `/search` is now fully implemented

1.3.0 - Improved entry response classes to handle deserialization of more responses correctly: Fixed some deserialization issues with Pos/Pronunciation elements; Simplified the structure for languages in translations, translations are now a dictionary of languagecode + translation

## Lexicala.NET.Parser
1.0.0 - Initial version: Parse search results into more convenient structure. Contains registration class to register the LexicalaSearchParser and LexicalaClient in a .NET core app. 

1.1.0 - Parser now verifies that source language is valid, using the  `/languages` endpoint. Languages are cached in memory cache

1.2.0 - Added `GetEntryAsync` method to parse a single entry. Updated parsing of entries to match changes in the Lexicala.NET.Client entry response classes.
