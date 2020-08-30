# Change log (Nuget packages) 

## Lexicala.NET.Client
1.0.0 - Initial version: basic `/search` and `/entries` api implementation

1.1.0 - Added new api implementations: `/test`, `/me` and `/languages`

1.2.0 - `/search` is now fully implemented

## Lexicala.NET.Parser
1.0.0 - Initial version: Parse search results into more convenient structure. Contains registration class to register the LexicalaSearchParser and LexicalaClient in a .NET core app. 

1.1.0 - Parser now verifies that source language is valid, using the  `/languages` endpoint. Languages are cached in memory cache
