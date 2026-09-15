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

namespace Basilisque.AspNetCore.StoplightElements.Demo.Endpoints;

internal static class WeatherEndpoints
{
    private static readonly string[] Summaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ];

    public static void MapWeatherEndpoints(WebApplication app)
    {
        var weatherGroup = app.MapGroup("/api/weather").WithTags("Weather");

        weatherGroup.MapGet("/forecast", () =>
            Enumerable.Range(1, 5).Select(index =>
                new WeatherForecastDto(
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                    Random.Shared.Next(-10, 36),
                    Summaries[Random.Shared.Next(Summaries.Length)])).ToArray())
            .WithGroupName("weather")
            .WithName("GetWeatherForecast")
            .WithSummary("Retrieve a 5-day weather forecast");
    }

    private sealed record WeatherForecastDto(DateOnly Date, int TemperatureC, string Summary);
}
