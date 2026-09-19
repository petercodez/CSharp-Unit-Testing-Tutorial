using Library;
using Moq;
using Xunit;

namespace Tests;

/// <summary>
/// Example 12 — Async / Await Tests
///
/// xUnit natively supports async test methods — just return Task:
///   - async Task test methods are awaited automatically by xUnit
///   - Assert.ThrowsAsync&lt;T&gt; for async exceptions
///   - Moq can set up async methods with ReturnsAsync / ThrowsAsync
/// </summary>
public class AsyncTests
{
    private readonly Mock<IWeatherApiClient> _apiMock = new();
    private readonly WeatherService _service;

    public AsyncTests()
    {
        _service = new WeatherService(_apiMock.Object);
    }

    [Fact]
    public async Task GetWeatherSummary_ValidCity_ReturnsSummaryString()
    {
        double expectedTemperature = 18.5;
        
        _apiMock.Setup(a => a.GetWeatherAsync("London"))
                .ReturnsAsync(new WeatherData("London", expectedTemperature, "Cloudy"));

        string summary = await _service.GetWeatherSummaryAsync("London");

        Assert.Contains("London", summary);
    }

    [Fact]
    public async Task GetWeatherSummary_ApiReturnsNull_ReturnsFallbackMessage()
    {
        _apiMock.Setup(a => a.GetWeatherAsync("Unknown"))
                .ReturnsAsync((WeatherData?)null);

        string summary = await _service.GetWeatherSummaryAsync("Unknown");

        Assert.Contains("No weather data", summary);
        Assert.Contains("Unknown", summary);
    }

    [Fact]
    public async Task GetWeatherSummary_EmptyCity_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetWeatherSummaryAsync(""));
    }

    [Fact]
    public async Task IsFreezingAsync_BelowZero_ReturnsTrue()
    {
        _apiMock.Setup(a => a.GetWeatherAsync("Antarctica"))
                .ReturnsAsync(new WeatherData("Antarctica", -15, "Blizzard"));

        bool result = await _service.IsFreezingAsync("Antarctica");
        Assert.True(result);
    }

    [Fact]
    public async Task IsFreezingAsync_AboveZero_ReturnsFalse()
    {
        _apiMock.Setup(a => a.GetWeatherAsync("Dubai"))
                .ReturnsAsync(new WeatherData("Dubai", 42, "Sunny"));

        bool result = await _service.IsFreezingAsync("Dubai");
        Assert.False(result);
    }

    [Fact]
    public async Task GetHottestForecastDay_ReturnsDayWithHighestTemp()
    {
        var forecast = new List<WeatherData>
        {
            new("Paris", 20.0, "Sunny"),
            new("Paris", 35.0, "Hot"),
            new("Paris", 15.0, "Cloudy"),
        };
        _apiMock.Setup(a => a.GetForecastAsync("Paris", 3))
                .ReturnsAsync(forecast);

        var hottest = await _service.GetHottestForecastDayAsync("Paris", 3);

        Assert.NotNull(hottest);
        Assert.Equal(35.0, hottest.TemperatureCelsius);
    }
}
