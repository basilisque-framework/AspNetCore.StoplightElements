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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Basilisque.AspNetCore.StoplightElements;

/// <summary>
/// Provides extension methods for mapping Stoplight Elements documentation endpoints in ASP.NET Core applications.
/// </summary>
public static class StoplightElementsExtensions
{
    private const string AssetPrefix = "StoplightAssets.";

    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        /// Maps the Stoplight Elements documentation page and its embedded assets to the specified route prefix.
        /// </summary>
        /// <param name="configure">An optional delegate to configure <see cref="StoplightElementsOptions"/>.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
        public IEndpointRouteBuilder MapStoplightElements(Action<StoplightElementsOptions>? configure = null)
        {
            var options = new StoplightElementsOptions();

            configure?.Invoke(options);

            if (options.Documents.Count == 0)
                options.Documents.Add(new StoplightElementsDocumentOptions());

            // Assembly precedence: options.TargetAssembly -> CallingAssembly
            var targetAssembly = options.TargetAssembly ?? Assembly.GetCallingAssembly();

            var prefix = options.RoutePrefix.TrimEnd('/');

            // Discover all embedded resources matching the StoplightAssets. prefix
            var resourceNames = targetAssembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(AssetPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var fullResourceName in resourceNames)
            {
                var fileName = fullResourceName.Substring(AssetPrefix.Length);
                var route = $"{prefix}/{fileName}";
                var contentType = getContentType(fileName);

                endpoints.MapGet(route, async context =>
                {
                    await serveEmbeddedAsset(context, targetAssembly, fullResourceName, contentType, options.MaxAgeSeconds);
                }).ExcludeFromDescription();

                // Alias mapping for LICENSE.txt if web-components.min.js.LICENSE.txt is present
                if (fileName.Equals("web-components.min.js.LICENSE.txt", StringComparison.OrdinalIgnoreCase))
                {
                    endpoints.MapGet($"{prefix}/LICENSE.txt", async context =>
                    {
                        await serveEmbeddedAsset(context, targetAssembly, fullResourceName, "text/plain; charset=utf-8", options.MaxAgeSeconds);
                    }).ExcludeFromDescription();
                }
            }

            // Endpoint: Serve documentation HTML shell
            if (options.MapHtmlEndpoint)
            {
                endpoints.MapGet(prefix, () =>
                {
                    string html;
                    if (options.CustomHtmlTemplateHandler is null)
                        html = StoplightElementsDefaultHtmlRenderer.GetDefaultHtml(options, prefix);
                    else
                        html = options.CustomHtmlTemplateHandler(options, prefix, () => StoplightElementsDefaultHtmlRenderer.GetDefaultHtml(options, prefix));

                    return Results.Content(html, "text/html; charset=utf-8");
                }).ExcludeFromDescription();
            }

            return endpoints;
        }
    }

    private static async Task serveEmbeddedAsset(
        HttpContext context,
        Assembly assembly,
        string fullResourceName,
        string contentType,
        int maxAgeSeconds)
    {
        // Compute deterministic ETag based on assembly version and resource name
        var assemblyVersion = assembly.GetName().Version?.ToString() ?? "1.0.0";
        var etag = $"\"{assemblyVersion}_{fullResourceName.GetHashCode():X}\"";

        // Check ETag for HTTP 304 Not Modified
        if (context.Request.Headers.IfNoneMatch == etag)
        {
            context.Response.StatusCode = StatusCodes.Status304NotModified;
            return;
        }

        await using var stream = assembly.GetManifestResourceStream(fullResourceName);

        if (stream != null)
        {
            context.Response.Headers.ETag = etag;
            context.Response.Headers.CacheControl = $"public, max-age={maxAgeSeconds}, immutable";
            context.Response.ContentType = contentType;
            context.Response.ContentLength = stream.Length;

            await stream.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }

    private static string getContentType(string fileName) => fileName switch
    {
        var f when f.EndsWith(".js", StringComparison.OrdinalIgnoreCase) => "application/javascript; charset=utf-8",
        var f when f.EndsWith(".css", StringComparison.OrdinalIgnoreCase) => "text/css; charset=utf-8",
        var f when f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) => "application/json; charset=utf-8",
        var f when f.EndsWith(".html", StringComparison.OrdinalIgnoreCase) => "text/html; charset=utf-8",
        var f when f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) => "text/plain; charset=utf-8",
        _ => "application/octet-stream"
    };
}