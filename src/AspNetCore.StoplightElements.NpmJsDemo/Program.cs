/*
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
 */

using Basilisque.AspNetCore.StoplightElements;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi("npmjs-demo", options =>
{
    options.ShouldInclude = description =>
        string.Equals(description.GroupName, "npmjs-demo", StringComparison.OrdinalIgnoreCase);

    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Stoplight npmjs Demo API";
        document.Info.Version = "v1";
        document.Info.Description = "Demonstrates Stoplight asset download via npmjs.org tarball settings.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
}

app.MapGet("/api/ping", () => Results.Ok(new { Message = "pong", Utc = DateTime.UtcNow }))
    .WithGroupName("npmjs-demo")
    .WithName("Ping")
    .WithSummary("Simple ping endpoint for OpenAPI demo");

if (app.Environment.IsDevelopment())
{
    app.MapStoplightElements(options =>
    {
        options.RoutePrefix = "/api-docs";
        options.Documents.Clear();
        options.Documents.Add(new StoplightElementsDocumentOptions
        {
            Name = "npmjs demo",
            DocumentTitle = "Stoplight npmjs Demo Docs",
            ApiDescriptionUrl = "/openapi/npmjs-demo.json"
        });
    });
}

app.Run();
