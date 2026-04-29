---
name: repository-information
description: "Use when referring to repository structure, project layout, or overall information about the Lexicala.NET repo."
applyTo: "**/*"
---

This repository contains the Lexicala.NET client library and related projects.

Official documentation: https://api.lexicala.com/documentation/

Key repository information:

- Solution: `source/Lexicala.NET.slnx`
- Projects:
  - `source/Lexicala.NET/` — main library project with the Lexicala client, configuration, parsing, request, and response types
  - `source/Demo/Lexicala.NET.Demo.Api/` — ASP.NET Core host with Swagger UI for manually exercising the implemented Lexicala endpoints
  - `source/Lexicala.NET.Tests/` — unit tests and parser tests with embedded JSON fixtures under `Resources/`
- Target frameworks:
  - `source/Lexicala.NET/` — `net8.0` and `net10.0`
  - `source/Demo/Lexicala.NET.Demo.Api/` — `net10.0`
  - `source/Lexicala.NET.Tests/` — `net10.0`
- Purpose: provides a .NET SDK for interacting with the Lexicala API, including request models, response models, parser/search abstractions, dependency registration, and a Swagger-enabled local test host.
- Repository is organized into:
  - `Parsing/` for search parser implementation and DTO models
  - `Request/` for request model definitions
  - `Response/` for response model definitions and metadata
  - `.github/workflows/` for CI, PR validation, and package publishing workflows

Current API surface highlights:

- Supported client endpoints include `/test`, `/languages`, `/search`, `/search-entries`, `/search-rdf`, `/search-definitions`, `/fluky-search`, `/entries`, `/senses`, and advanced search variants.
- The `/me` endpoint has been removed and should not be described as supported.

When answering questions about this repository, reference this file for the canonical project layout and repository-level details.
