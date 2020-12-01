![Main package](https://github.com/HannoZ/Lexicala.NET/workflows/Build%20Test%20Package/badge.svg)
![MicrosoftDependencyInjection](https://github.com/HannoZ/Lexicala.NET/workflows/Build%20and%20Push%20to%20Nuget%20-%20MicrosoftDependencyInjection/badge.svg)
![Autofac](https://github.com/HannoZ/Lexicala.NET/workflows/Build%20and%20Push%20to%20Nuget%20-%20Autofac/badge.svg)

# Lexicala.NET
A .NET Client for the Lexicala dictionary api. Target framework is .NET Standard so it should work with .NET 4.7.2 and higher and .NET Core. Version 1.3 has dependencies on  .NET 5 packages, earlier versions use .NET Core 3.1 packages. 

## About the repository
The repository contains the .NET implementation for (part of) the Lexicala Api. It also contains parser logic that implements and uses the ILexicalaClient to execute a search request and parse the results into a model that is easier to use than the raw data from the api (at least for me it is ;-) ). For full documentation on the api visit the [Lexicala documentation page](https://api.lexicala.com/documentation).

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
For .NET Core-based applications you can use the [Lexicala.NET.MicrosoftDependencyInjection](https://www.nuget.org/packages/Lexicala.NET.MicrosoftDependencyInjection/) Nuget package. This package contains extension methods to register the ILexicalaClient and other dependencies in a .NET Core startup class:
`services.RegisterLexicala(Configuration)`
This overload depends on a Lexicala section in your appsettings.json file:
```json
{
"Lexicala": {
    "Username": "HannoZ",
    "Password": "pa$$"
  }
}
```
A similar package exists for [Autofac](https://www.nuget.org/packages/Lexicala.NET.Autofac/);
Now you can either inject and use the ILexicalaClient directly, or use the ILexicalaSearchParser. 

## TODO
- improve exception handling
- implement sense api 
