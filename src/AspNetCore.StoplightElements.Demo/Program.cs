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

using Basilisque.AspNetCore.StoplightElements.Demo.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Basilisque.AspNetCore.StoplightElements.Demo;

/// <summary>
/// The main program class for the demo application that demonstrates the integration of Stoplight Elements with ASP.NET Core.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point for the demo application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add OpenAPI specification support (built-in .NET OpenAPI generation)
        builder.Services.AddOpenApi("todo", options =>
        {
            options.ShouldInclude = description => string.Equals(description.GroupName, "todo", StringComparison.OrdinalIgnoreCase);

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Basilisque Demo Todo API";
                document.Info.Version = "v1";
                document.Info.Description = "Todo API document generated from the demo endpoints.";
                return Task.CompletedTask;
            });
        });

        builder.Services.AddOpenApi("weather", options =>
        {
            options.ShouldInclude = description => string.Equals(description.GroupName, "weather", StringComparison.OrdinalIgnoreCase);

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Basilisque Demo Weather API";
                document.Info.Version = "v1";
                document.Info.Description = "Weather API document generated from the demo endpoints.";
                return Task.CompletedTask;
            });
        });

        var app = builder.Build();

        // Enable OpenAPI JSON endpoints in development (served at /openapi/{documentName}.json)
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi("/openapi/{documentName}.json");
        }

        // Map the Todo API sample endpoints
        TodoEndpoints.MapTodoEndpoints(app);
        WeatherEndpoints.MapWeatherEndpoints(app);

        // Map Stoplight Elements UI endpoint
        if (app.Environment.IsDevelopment())
        {
            app.MapStoplightElements(options =>
            {
                options.Documents.Add(new StoplightElementsDocumentOptions
                {
                    Name = "Todo API",
                    DocumentTitle = "Basilisque Demo Todo API Docs",
                    ApiDescriptionUrl = "/openapi/todo.json"
                });

                options.Documents.Add(new StoplightElementsDocumentOptions
                {
                    Name = "Weather API",
                    DocumentTitle = "Basilisque Demo Weather API Docs",
                    ApiDescriptionUrl = "/openapi/weather.json"
                });

                // example of customizing the main HTML template
                options.CustomHtmlTemplateHandler = (options, routePrefix, getDefaultHtml) =>
                {
                    // get the default HTML template if needed
                    var htmlTemplate = getDefaultHtml();

                    // Customize the HTML template here if needed
                    //...

                    // return the customized HTML template or a completely new one
                    return htmlTemplate;
                };
            });
        }

        app.Run();
    }
}
