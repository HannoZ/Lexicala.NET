# Change log 
## Lexicala.NET
1.1.1. - Fixed a bug in the LexicalaSearchParser that caused the Pos property of a SearchResultEntry to display System.String[] instead of the actual value

1.1 - The DependencyRegistration helper has been moved to a new project/package: Lexicala.NET.MicrosoftDependencyInjection. Also an integration package for Autofac is now available. The `LexicalaConfig` class has been extended with helper methods to setup the LexicalaClient. The Microsoft and Autofac integration packages make use of those helper methods. 

1.0 - The Client and Parser project have been merged into a single project and Nuget package. 
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
