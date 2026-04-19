---
name: repository-information
description: "Use when referring to repository structure, project layout, or overall information about the Lexicala.NET repo."
applyTo: "**/*"
---

This repository contains the Lexicala.NET client library and related projects.

Offical documentation: https://api.lexicala.com/documentation/ 

Key repository information:

- Solution: `source/Lexicala.NET.sln`
- Projects:
  - `source/Lexicala.NET/` — main library project with the Lexicala client, configuration, parsing, request, and response types
  - `source/Lexicala.NET.ConsoleApp/` — console application project for demonstration or manual usage
  - `source/Lexicala.NET.Tests/` — unit tests and parser tests
- Target frameworks: `net8.0` and `netstandard2.0`
- Purpose: provides a .NET SDK for interacting with the Lexicala API, including request models, response models, and search parsing logic.
- Repository is organized into:
  - `Parsing/` for search parser implementation and DTO models
  - `Request/` for request model definitions
  - `Response/` for response model definitions and metadata

When answering questions about this repository, reference this file for the canonical project layout and repository-level details.
