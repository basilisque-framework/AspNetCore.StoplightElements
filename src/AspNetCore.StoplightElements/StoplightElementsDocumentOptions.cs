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

namespace Basilisque.AspNetCore.StoplightElements;

/// <summary>
/// Represents per-document configuration for rendering one <c>&lt;elements-api&gt;</c> instance.
/// </summary>
public class StoplightElementsDocumentOptions
{
    /// <summary>
    /// Gets or sets the name shown in the selector when multiple documents are configured.
    /// If not set, <see cref="DocumentTitle"/> is used.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the title displayed in the browser tab and the document selector when multiple documents are configured.
    /// Default value is <c>"API Documentation"</c>.
    /// </summary>
    public string DocumentTitle { get; set; } = "API Documentation";

    /// <summary>
    /// Gets or sets the relative or absolute URL to the OpenAPI or AsyncAPI specification document (JSON or YAML).
    /// Default value is <c>"/openapi/v1.json"</c>.
    /// </summary>
    public string ApiDescriptionUrl { get; set; } = "/openapi/v1.json";

    /// <summary>
    /// Gets or sets the visual layout of the documentation UI.
    /// Supported options are <c>"sidebar"</c> and <c>"stacked"</c>. Default value is <c>"sidebar"</c>.
    /// </summary>
    public string Layout { get; set; } = "sidebar";

    /// <summary>
    /// Gets or sets the routing strategy used by the Stoplight Elements web component.
    /// Supported options are <c>"hash"</c>, <c>"history"</c>, and <c>"memory"</c>. Default value is <c>"history"</c>.
    /// </summary>
    public string Router { get; set; } = "history";

    /// <summary>
    /// Gets or sets a value indicating whether the "Try It" feature should be hidden in the documentation UI.
    /// </summary>
    public bool HideTryIt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the "Schemas" section should be hidden in the documentation UI.
    /// </summary>
    public bool HideSchemas { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to filter out any content which has been marked as internal with 'x-internal'.
    /// </summary>
    public bool HideInternal { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the "Export" feature should be hidden in the documentation UI.
    /// </summary>
    public bool HideExport { get; set; }

    /// <summary>
    /// Gets or sets a dictionary of additional attributes to be added to the <c>&lt;elements-api&gt;</c> web component.
    /// </summary>
    public IDictionary<string, object?> AdditionalAttributes { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds an additional attribute to the <c>&lt;elements-api&gt;</c> web component.
    /// </summary>
    public StoplightElementsDocumentOptions AddAttribute(string name, object? value)
    {
        AdditionalAttributes[name] = value;
        return this;
    }

    /// <summary>
    /// Renders all set options neatly as HTML attributes.
    /// </summary>
    public string RenderHtmlAttributes()
    {
        var sb = new System.Text.StringBuilder();

        foreach (var (key, val) in GetHtmlAttributeMap())
        {
            var encoded = System.Net.WebUtility.HtmlEncode(val);
            sb.Append($" {key}=\"{encoded}\"");
        }

        return sb.ToString().TrimStart();
    }

    internal IDictionary<string, string> GetHtmlAttributeMap()
    {
        var attributes = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["apiDescriptionUrl"] = ApiDescriptionUrl,
            ["router"] = Router,
            ["layout"] = Layout,
            ["hideTryIt"] = HideTryIt ? true : null,
            ["hideSchemas"] = HideSchemas ? true : null,
            ["hideInternal"] = HideInternal ? true : null,
            ["hideExport"] = HideExport ? true : null,
        };

        foreach (var (key, value) in AdditionalAttributes)
        {
            attributes[key] = value;
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, val) in attributes)
        {
            if (val is null) continue;

            // bool values: only render if true (prevents the hideTryIt="false" issue that hides the try feature just because the attribute is present)
            if (val is bool boolVal)
            {
                if (boolVal)
                    result[key] = "true";

                continue;
            }

            result[key] = val.ToString() ?? "";
        }

        return result;
    }
}
