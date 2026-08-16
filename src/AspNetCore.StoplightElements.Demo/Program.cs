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
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Basilisque Demo API";
                document.Info.Version = "v1.1";
                document.Info.Description = "Example API to demonstrate the Stoplight Elements integration.";
                return Task.CompletedTask;
            });
        });

        var app = builder.Build();

        // Enable OpenAPI JSON endpoint in development (served at /openapi/v1.json)
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // Map the Todo API sample endpoints
        TodoEndpoints.MapTodoEndpoints(app);

        // Map Stoplight Elements UI endpoint
        app.MapStoplightElements(options =>
        {
            options.DocumentTitle = "Basilisque Demo API Docs";


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

        app.Run();
    }
}
