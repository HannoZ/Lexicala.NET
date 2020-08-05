# Lexicala.NET
A .NET Client for the Lexicala dictionary api. Tested only with .NET Core 3.x 

## About the repository
The repository contains two main projects: Lexicala.NET.Client and Lexicala.NET.Parser. The Client project contains a .NET implementation for (part of) the Lexicala Api. The Parser project is an implementation that uses the ILexicalaClient to execute a search request and parse the results into a model that is easier to use than the raw data from the api (at least for me it is ;-) ). For full documentation on the api visit the [Lexicala documentation page](https://api.lexicala.com/documentation).

Not all parts of the api are (fully) implemented. I have begun to write this library for a personal project of mine where I'm using api's to get automated translations from Spanish to Dutch (my native language) or English when Dutch translation is not available.

**NOT** implemented: 
- `/test`
- `/users/me`

(easier to just use Postman for those two)
- `/senses` (because it seems to be almost equal to `/entries` only with less useful information)

*Partially* implemented: 
- `/search`

A basic search query can be executed by specifying the search text and source language. The response object is as complete as possible by tyring out many different search queries, but probably some properties are still missing.

*Fully* implemented:
- `/entries`

Entries is the most interesting part of the api because it contains the detailed information on a search result ('sense'). As with the search response, I've tried to have the response as complete as possible. 

## Basic usage
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
