<!--
   Copyright 2026 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
-->
# Basilisque - ASP.Net - Stoplight Elements

[![NuGet Basilisque.AspNetCore.StoplightElements](https://img.shields.io/badge/NuGet_Basilisque.AspNetCore.StoplightElements-latest-%23004880.svg?logo=nuget)](https://www.nuget.org/packages/Basilisque.AspNetCore.StoplightElements)  
[![License](https://img.shields.io/badge/License-Apache%20License%202.0-%23D22128.svg?logo=apache&logoColor=%23D22128)](LICENSE.txt)  

## Description
This package provides a lightweight integration for [Stoplight Elements](https://stoplight.io/open-source/elements) API documentation in ASP.NET Core applications.  
It automatically manages frontend static assets (`JS` / `CSS`) via MSBuild embedded resources, provides full offline support and offers flexible configuration options.

## Features

- **Seamless ASP.NET Core Integration:** Map interactive API documentation using a single endpoint method (`MapStoplightElements`).
- **Zero Frontend Dependencies:** All required JavaScript and CSS assets are automatically fetched at build time and embedded directly into the assembly.
- **Offline & Production Ready:** No external CDN requests at runtime. Assets are cached locally in `obj/` to ensure deterministic builds and offline availability.
- **Multi-API Support:** Easily host multiple documentation pages (e.g., v1, v2, internal, public, AsyncAPI) within the same application.
- **Flexible Options Model:** Strongly typed configuration combined with a generic attribute engine for future-proof extensibility.
- **Asset-Only Mode:** Disable the standalone HTML endpoint to host `<elements-api>` directly inside your custom site like Blazor, Razor Pages, or MVC layouts.

## Installation
Install the package via the .NET CLI:
```bash
dotnet add package Basilisque.AspNetCore.StoplightElements
```
Or via the Package Manager Console:
```powershell
Install-Package Basilisque.AspNetCore.StoplightElements
```
## Quick Start

### 1. Register OpenAPI Support

First, ensure your ASP.NET Core application generates an OpenAPI description document (e.g., using `Microsoft.AspNetCore.OpenApi` or `Swashbuckle`).

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI services
builder.Services.AddOpenApi();

var app = builder.Build();

// Map the OpenAPI endpoint
app.MapOpenApi();

// Map Stoplight Elements UI at /api-docs (customizable)
app.MapStoplightElements();

app.Run();
```

Navigate to `http://localhost:<port>/api-docs` to view your interactive API documentation.

## Controlling the Stoplight Elements Asset Version

By default, the package uses `latest` as the version for fetching Stoplight Elements assets from unpkg at build time.

### Pinning Versions for Production Builds

To guarantee reproducible, deterministic production builds, you can pin a specific Stoplight Elements version in your project file (`.csproj`):

```xml
<PropertyGroup>
  <!-- Option 1: Pin to exact version (Recommended for deterministic production builds) -->
  <StoplightElementsVersion>9.0.0</StoplightElementsVersion>

  <!-- Option 2: Pin to Major.Minor (Automatically receives patch updates on clean builds) -->
  <!-- <StoplightElementsVersion>9.0</StoplightElementsVersion> -->

  <!-- Option 3: Pin to Major version only (Receives minor and patch updates) -->
  <!-- <StoplightElementsVersion>9</StoplightElementsVersion> -->
</PropertyGroup>
```

### Build Caching & Offline Builds

- Assets are downloaded once during the build step and cached in `$(IntermediateOutputPath)stoplight-assets/` (the `obj/` folder).
- Subsequent builds reuse the cached assets without making network calls.
- If you change `<StoplightElementsVersion>`, the MSBuild target automatically downloads the new asset version on the next build.

## Usage & Configuration

### Basic Configuration

You can customize the endpoint route, page title, layout, and Stoplight features using `StoplightElementsOptions`:

```csharp
app.MapStoplightElements(options =>
{
    options.DocumentTitle = "My Enterprise API Documentation";
    options.ApiDescriptionUrl = "/openapi/v1.json"; // Path to your OpenAPI JSON/YAML
    options.Layout = "sidebar";                     // "sidebar" or "stacked"
    options.Router = "history";                     // "history", "hash", or "memory"
    options.HideTryIt = false;                      // Show/hide interactive API console
    options.HideSchemas = false;                    // Show/hide schema definitions
    options.HideExport = false;                     // Show/hide export specification button
});
```

### Multiple API Documentation Endpoints

To host multiple distinct API documentation pages within the same project, call `MapStoplightElements` with different route prefixes:

```csharp
// REST API v1
app.MapStoplightElements(options =>
{
    options.RoutePrefix = "api-docs/v1";
    options.DocumentTitle = "Customer API v1";
    options.ApiDescriptionUrl = "/openapi/v1.json";
});

// REST API v2
app.MapStoplightElements(options =>
{
    options.RoutePrefix = "api-docs/v2";
    options.DocumentTitle = "Customer API v2";
    options.ApiDescriptionUrl = "/openapi/v2.json";
});

// Event-driven Architecture (AsyncAPI)
app.MapStoplightElements(options =>
{
    options.RoutePrefix = "api-docs/events";
    options.DocumentTitle = "Event Bus Specification";
    options.ApiDescriptionUrl = "/asyncapi/v1.json";
});
```
This provides the respective Stoplight Elements instance under the provided route prefix. Please note that this does not provide the respective API description URLs.

### Advanced HTML Attribute Customization

If Stoplight introduces new HTML attributes or you need to supply specialized configuration options (such as custom proxies or CORS policies), use `AddAttribute`:

```csharp
app.MapStoplightElements(options =>
{
    options.AddAttribute("tryItCredentialsPolicy", "include")
           .AddAttribute("corsProxy", "https://proxy.example.com")
           .AddAttribute("logo", "https://example.com/logo.png");
});
```

### Embedding in Custom Web Pages (Asset-Only Mode - Stoplight Elements API component without the main HTML page of this project)

If your application already has a custom layout (e.g., in Blazor, Razor Pages, or React) and you only want to serve the static frontend assets from the library:

```csharp
// Map static assets under /assets/stoplight, but disable the HTML page endpoint
app.MapStoplightElements(options =>
{
    options.RoutePrefix = "assets/stoplight";
    options.MapHtmlEndpoint = false;
});
```

In your custom HTML page or Razor layout, include the assets and use `<elements-api>` directly:

```html
<link rel="stylesheet" href="/assets/stoplight/styles.min.css" />
<script src="/assets/stoplight/web-components.min.js" defer></script>

<div class="my-custom-layout">
    <header>
        <h1>Custom Application Dashboard</h1>
    </header>
    <main>
        <elements-api 
            apiDescriptionUrl="/openapi/v1.json" 
            router="hash" 
            layout="sidebar" />
    </main>
</div>
```

## License
The Basilisque framework (including this repository) is licensed under the [Apache License, Version 2.0](LICENSE.txt).

## Third-Party Licenses & Attribution
This library bundles third-party open-source web components created by Stoplight, Inc. into the target application:

- **Stoplight Elements** ([@stoplight/elements](https://github.com/stoplightio/elements))  
  Copyright (c) Stoplight, Inc.  
  Licensed under the [Apache License, Version 2.0](https://www.apache.org/licenses/LICENSE-2.0).

The required license text (`web-components.min.js.LICENSE.txt`) is automatically embedded and served alongside the frontend web component assets under your configured route prefix (e.g., `/api-docs/LICENSE.txt`).
