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

using System.Net;
using System.Net.Http;

namespace Basilisque.AspNetCore.StoplightElements.Unit.Tests;

public class StoplightElementsTests
{
    [Test]
    public async Task Get_Docs_Index_Returns_Html_Page()
    {
        using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api-docs");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("text/html");

        var html = await response.Content.ReadAsStringAsync();
        await Assert.That(html).Contains("<elements-api");
        await Assert.That(html).Contains("web-components.min.js");
    }

    [Test]
    public async Task Get_Asset_Returns_Content_And_ETag_Header()
    {
        using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api-docs/web-components.min.js");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.ETag).IsNotNull();
        await Assert.That(response.Headers.CacheControl?.MaxAge).IsNotNull();

        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsNotEmpty();
    }

    [Test]
    public async Task Get_Asset_With_Matching_IfNoneMatch_Returns_304_NotModified()
    {
        using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // 1. Erster Aufruf: ETag abrufen
        var initialResponse = await client.GetAsync("/api-docs/web-components.min.js");
        var etag = initialResponse.Headers.ETag;

        await Assert.That(etag).IsNotNull();

        // 2. Zweiter Aufruf mit If-None-Match Header
        var request = new HttpRequestMessage(HttpMethod.Get, "/api-docs/web-components.min.js");
        request.Headers.IfNoneMatch.Add(etag!);

        var secondResponse = await client.SendAsync(request);

        await Assert.That(secondResponse.StatusCode).IsEqualTo(HttpStatusCode.NotModified);

        var body = await secondResponse.Content.ReadAsStringAsync();
        await Assert.That(body).IsEmpty();
    }

    [Test]
    public async Task Custom_Options_Are_Respected()
    {
        using var factory = new TestWebApplicationFactory(options =>
        {
            options.RoutePrefix = "/custom-api-docs";
            options.DocumentTitle = "My Custom Docs";
            options.ApiDescriptionUrl = "/v2/openapi.json";
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/custom-api-docs");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        await Assert.That(html).Contains("<title>My Custom Docs</title>");
        await Assert.That(html).Contains("apiDescriptionUrl=\"/v2/openapi.json\"");
    }

    [Test]
    public async Task Non_Existent_Asset_Returns_404()
    {
        using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api-docs/non-existent-file.js");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}