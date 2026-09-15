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

using System.Reflection;

namespace Basilisque.AspNetCore.StoplightElements;

/// <summary>
/// Defines a delegate for customizing the HTML template used to render the Stoplight Elements documentation page.
/// </summary>
/// <param name="options">The <see cref="StoplightElementsOptions"/> instance being used.</param>
/// <param name="routePrefix">The route prefix under which the documentation is being served.</param>
/// <param name="getDefaultHtml">A function that returns the default HTML content.</param>
/// <returns>The customized HTML content as a string.</returns>
public delegate string CustomHtmlTemplateHandler(StoplightElementsOptions options, string routePrefix, Func<string> getDefaultHtml);

/// <summary>
/// Configuration options for configuring Stoplight Elements API documentation UI in ASP.NET Core.
/// </summary>
public class StoplightElementsOptions
{
    /// <summary>
    /// Gets or sets the route prefix under which the Stoplight Elements UI and static assets will be served.
    /// Default value is <c>"/api-docs"</c>.
    /// </summary>
    public string RoutePrefix { get; set; } = "/api-docs";

    /// <summary>
    /// Gets the collection of documentation entries rendered by this endpoint.
    /// The first document is used as default in the generated HTML.
    /// </summary>
    public IList<StoplightElementsDocumentOptions> Documents { get; } = [];

    /// <summary>
    /// Gets or sets the duration in seconds for which static assets should be cached by the browser via the <c>Cache-Control</c> header.
    /// Default value is <c>31536000</c> (1 year).
    /// </summary>
    public int MaxAgeSeconds { get; set; } = 31536000;

    /// <summary>
    /// Gets or sets an optional target <see cref="Assembly"/> containing the embedded Stoplight assets.
    /// If <c>null</c>, the calling assembly of <c>MapStoplightElements</c> is used.
    /// </summary>
    public Assembly? TargetAssembly { get; set; }

    /// <summary>
    /// Gets or sets a delegate that allows customizing the HTML template used to render the Stoplight Elements documentation page.
    /// </summary>
    public CustomHtmlTemplateHandler? CustomHtmlTemplateHandler { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the standalone HTML documentation page endpoint should be mapped.
    /// <para>
    /// Set to <c>false</c> if you only want to serve the static frontend assets (<c>JS</c>/<c>CSS</c>)
    /// to embed the <c>&lt;elements-api&gt;</c> Web Component directly into your own custom pages or Razor layouts.
    /// </para>
    /// <para>Defaults to <c>true</c>.</para>
    /// </summary>
    public bool MapHtmlEndpoint { get; set; } = true;

}