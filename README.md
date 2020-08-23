# Lexicala.NET
A .NET Client for the Lexicala dictionary api. Tested only with .NET Core 3.x 

## About the repository
The repository contains two main projects: Lexicala.NET.Client and Lexicala.NET.Parser. The Client project contains a .NET implementation for (part of) the Lexicala Api. The Parser project is an implementation that uses the ILexicalaClient to execute a search request and parse the results into a model that is easier to use than the raw data from the api (at least for me it is ;-) ). For full documentation on the api visit the [Lexicala documentation page](https://api.lexicala.com/documentation).

Alle api methods, except for `/senses` are implemented, but not all methods are thoroughly tested. I have started to build this library for a hobby project where I only need translation from Spanish to some other languages, but I have tested some searches on English words because they can have a much more extensive response.

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
The Lexicala.NET.Client and Lexicala.NET.Parser projects are available on Nuget.

The Lexicala.NET.Parser project contains extension methods to register the ILexicalaClient in a .NET Core startup class:
`services.RegisterLexicala(Configuration)`
This overload depends on a Lexicala section in your appsettings.json file:
```json
{
"Lexicala": {
    "Username": "hannoz82",
    "Password": "pa$$"
  }
}
```
Now you can either inject and use the ILexicalaClient directly, or use the ILexicalaSearchParser. 

## TODO
- improve exception handling
- implement sense api 
