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

using System.Text;
using System.Text.Json;

namespace Basilisque.AspNetCore.StoplightElements;

internal static class StoplightElementsDefaultHtmlRenderer
{
    internal static string GetDefaultHtml(StoplightElementsOptions options, string routePrefix)
    {
        var primaryDocument = options.Documents[0];

        if (options.Documents.Count == 1)
            return renderSingleDocumentHtml(primaryDocument, routePrefix);

        return renderMultiDocumentHtml(options.Documents, routePrefix, primaryDocument);
    }

    private static string renderSingleDocumentHtml(StoplightElementsDocumentOptions document, string routePrefix)
    {
        var result = getCommonDocumentHtml(document, routePrefix);

        result = result.Replace("<--ADDITIONAL_STYLES-->", @"
      elements-api {
        display: block;
        height: 100vh;
        width: 100vw;
      }
");

        result = result.Replace("<--ADDITIONAL_BODY_BEFORE-->", "");

        result = result.Replace("<--ADDITIONAL_BODY_AFTER-->", "");

        return result;
    }

    private static string renderMultiDocumentHtml(IEnumerable<StoplightElementsDocumentOptions> documents, string routePrefix, StoplightElementsDocumentOptions defaultDocument)
    {
        var selectorOptions = buildSelectorOptions(documents);

        var documentPayload = documents.Select(document => new
        {
            documentTitle = document.DocumentTitle,
            attributes = document.GetHtmlAttributeMap()
        });

        var documentsJson = JsonSerializer.Serialize(documentPayload);

        var result = getCommonDocumentHtml(defaultDocument, routePrefix);

        result = result.Replace("<--ADDITIONAL_STYLES-->", @"
      .stoplight-doc-switcher {
        display: flex;
        align-items: center;
        gap: .5rem;
        padding: .75rem 1rem;
        border-bottom: 1px solid rgba(148, 163, 184, .3);
        background: rgba(248, 250, 252, .96);
      }

      .stoplight-doc-switcher label {
        font-size: .875rem;
        color: #334155;
        font-weight: 600;
      }

      .stoplight-doc-switcher select {
        border: 1px solid #cbd5e1;
        border-radius: .375rem;
        background: #ffffff;
        color: #0f172a;
        padding: .375rem .5rem;
        min-width: 16rem;
        font-size: .875rem;
      }

      @media (prefers-color-scheme: dark) {
        .stoplight-doc-switcher {
          background: rgba(15, 23, 42, .96);
          border-bottom-color: rgba(71, 85, 105, .8);
        }

        .stoplight-doc-switcher label {
          color: #e2e8f0;
        }

        .stoplight-doc-switcher select {
          background: #0f172a;
          color: #e2e8f0;
          border-color: #334155;
        }
      }

      elements-api {
        display: block;
        height: calc(100vh - 57px);
        width: 100vw;
      }
");

        result = result.Replace("<--ADDITIONAL_BODY_BEFORE-->", $$"""
    <div class="stoplight-doc-switcher">
      <select id="stoplight-document-select">{{selectorOptions}}</select>
    </div>
    """);

        result = result.Replace("<--ADDITIONAL_BODY_AFTER-->", $$"""
    <script>
      (() => {
        const documents = {{documentsJson}};
        const selector = document.getElementById("stoplight-document-select");
        let apiElement = document.getElementById("stoplight-elements-api");

        if (!selector || !apiElement || documents.length === 0) {
          return;
        }

        const createApiElement = (selected) => {
          const nextApiElement = document.createElement("elements-api");
          nextApiElement.id = "stoplight-elements-api";

          for (const [attributeName, attributeValue] of Object.entries(selected.attributes)) {
            nextApiElement.setAttribute(attributeName, attributeValue);
          }

          return nextApiElement;
        };

        const renderDocument = (index) => {
          const selected = documents[index];

          if (!selected) {
            return;
          }

          document.title = selected.documentTitle;

          const nextApiElement = createApiElement(selected);
          apiElement.replaceWith(nextApiElement);
          apiElement = nextApiElement;
        };

        selector.addEventListener("change", () => {
          const index = Number.parseInt(selector.value, 10);
          renderDocument(index);
        });
      })();
    </script>
    """);

        return result;
    }

    private static string getCommonDocumentHtml(StoplightElementsDocumentOptions document, string routePrefix)
    {
        var attributes = document.RenderHtmlAttributes();

        return $$"""
        <!doctype html>
        <html lang="en">
          <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
            <title>{{document.DocumentTitle}}</title>

            <link rel="stylesheet" href="{{routePrefix}}/styles.min.css" />
            <script src="{{routePrefix}}/web-components.min.js" defer></script>

            <style>
              html, body {
                height: 100%;
                width: 100%;
                margin: 0;
                padding: 0;
                overflow: hidden;
                font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
                background-color: #ffffff;
              }

              @media (prefers-color-scheme: dark) {
                html, body {
                  background-color: #0f172a;
                }
              }
              <--ADDITIONAL_STYLES-->
            </style>
          </head>
          <body>
            <--ADDITIONAL_BODY_BEFORE-->
            <elements-api id="stoplight-elements-api" {{attributes}} />
            <--ADDITIONAL_BODY_AFTER-->
          </body>
        </html>
        """;
    }

    private static string buildSelectorOptions(IEnumerable<StoplightElementsDocumentOptions> documents)
    {
        var sb = new StringBuilder();
        var index = 0;

        foreach (var document in documents)
        {
            var displayName = document.Name;

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = document.DocumentTitle;

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = document.ApiDescriptionUrl;

            sb.Append($"<option value=\"{index}\">{System.Net.WebUtility.HtmlEncode(displayName)}</option>");
            index++;
        }

        return sb.ToString();
    }
}
