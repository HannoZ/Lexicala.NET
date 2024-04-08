![Main package](https://github.com/HannoZ/Lexicala.NET/workflows/Build%20Test%20Package/badge.svg)

# Lexicala.NET
A .NET Client for the Lexicala dictionary api. The Lexical dictionary api is hosted by RapidApi, you need to register an account at RapidApi and obtain an api key in order to use this api. 

## About the repository
The repository contains the .NET implementation for (most parts of) the Lexicala Api. It also contains parser logic that implements and uses the ILexicalaClient to execute a search request and parse the results into a model that is easier to use than the raw data from the api (at least for me it is ;-) ). For full documentation on the api visit the [Lexicala documentation page](https://api.lexicala.com/documentation).

All api methods, except for `/senses` are implemented, but not all methods are thoroughly tested. I have started to build this library for a hobby project where I only need translation from Spanish to some other languages, but I have tested some searches on English words because they can have a much more extensive response.

A basic search query can be executed by specifying the search text and source language. The response object is as complete as possible by trying out many different search queries, but it could be that some properties are still missing.
Advanced search queries can also be executed.

Implemented api methods:
- `/test`
- `/users/me`
- `/languages`
- `/search` (two implementations, basic and advanced)
- `/entries`

Entries is the most interesting part of the api because it contains the detailed information on a search result ('sense'). As with the search response, I've tried to have the response as complete as possible. 


## Basic usage
Lexicala.NET is available on Nuget.
An extension method is available to register the ILexicalaClient and other dependencies in a .NET Core startup class:
`services.RegisterLexicala(Configuration)`
This method depends on a Lexicala section in your appsettings.json file:
```json
{
"Lexicala": {
    "ApiKey": "my-key"
  }
}
```
Now you can either inject and use the ILexicalaClient directly, or use the ILexicalaSearchParser. 

## Code examples
````c#
// get available languages in the Global dictionary
var response = await lexicalaClient.LanguagesAsync();
languages = response.Resources.Global;

// execute a basic search using the Lexicala client for the word árbol in spanish
var searchTerm = "árbol";
var srcLang = "en";
var searchResponse = await lexicalaClient.BasicSearchAsync(searchTerm, srcLang);
foreach (var result in searchResponse.Results)
{
    // get the entry details
    var entry = await lexicalaClient.GetEntryAsync(result.Id);
    foreach (var sense in entry.Senses)
    {
        // the sense contains all the information
    }
}

// use the LexicalaSearchParser to search for árbol in spanish
string searchTerm = "árbol";
string srcLang = "es";

var resultModel = await lexicalaSearchParser.SearchAsync(searchTerm, srcLang);
var summary_en = resultModel.Summary("en"); // "tree, shaft, post, mast"

foreach(var result in resultModel.Results)
{
    // do something with result
    string definition = result.Senses.First().Definition; // "planta de tronco leñoso y elevado"
}
````

## TODO
- improve exception handling
- implement sense api 
