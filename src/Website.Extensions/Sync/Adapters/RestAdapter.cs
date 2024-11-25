using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoleMates.Website.Extensions.Exceptions;
using SoleMates.Website.Extensions.Sync.Models;

namespace SoleMates.Website.Extensions.Sync.Adapters;
/// <summary> The <see cref="RestAdapter"/> class is responsible for fetching data from the REST API and converting the <see cref="JsonResult"/> <br/>
/// to a collection of <see cref="SeriesModel"/>. This collection is used by the <see cref="UmbracoAdapter"/> to populate Umbraco Nodes. </summary>
public class RestAdapter : ISourceAdapter<SeriesModel, string, SizeModel> {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RestAdapter> _logger;
    private readonly string? _username;
    private readonly string? _password;
    private readonly JsonSerializerOptions _serializerOptions;

    public RestAdapter(IHttpClientFactory httpClientFactory, ILogger<RestAdapter> logger) {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _serializerOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _username = Environment.GetEnvironmentVariable("API_USER");
        _password = Environment.GetEnvironmentVariable("API_PASS");

        if (string.IsNullOrEmpty(_username)) {
            _logger.LogError("RestAdapter.RestAdapter() - Could not find environment variable '$API_USER'.");
            throw new EnvironmentVariableMissingException("RestAdapter", "API_USER");
        }

        if (string.IsNullOrEmpty(_password)) {
            _logger.LogError("RestAdapter.RestAdapter() - Could not find environment variable '$API_PASS'.");
            throw new EnvironmentVariableMissingException("RestAdapter", "API_PASS");
        }
    }

    /// <summary> Fetches the ERP REST API. If something goes wrong an exception is thrown. This does not crash the server,
    /// since it gets handled by Hangfire. </summary>
    public async Task<List<SeriesModel>> FetchFromSource(string source) {
        HttpClient httpClient = _httpClientFactory.CreateClient();
        string formattedUrl = GetFormattedUrl(source);
        httpClient.BaseAddress = new Uri(formattedUrl);
        httpClient.DefaultRequestHeaders.Add("username", _username);
        httpClient.DefaultRequestHeaders.Add("password", _password);
        httpClient.Timeout = TimeSpan.FromMinutes(1);

        HttpResponseMessage response = await httpClient.GetAsync(source);

        List<SeriesModel>? seriesList = JsonSerializer.Deserialize<List<SeriesModel>>(response.Content.ReadAsStream(), _serializerOptions);

        if (seriesList is null) {
            _logger.LogError("RestAdapter.FetchFromSource() - Could not successfully fetch from the REST API.");
            throw new JsonException("RestAdapter.FetchFromSource() - Could not successfully fetch from the REST API.");
        }

        return seriesList;
    }

    // The HttpClient.BaseAddress Uri has to have some specific criteria met, otherwise it will throw an exception.
    // It must begin with "https://" or "http://" and end with a "/".
    // https://learn.microsoft.com/en-us/dotnet/api/system.uri?view=net-8.0
    private static string GetFormattedUrl(string url) {
        if (!url.StartsWith("https://") && !url.StartsWith("http://")) {
            url = "https://" + url;
        }
        if (!url.EndsWith('/')) {
            url += "/";
        }

        return url;
    }

    public async Task UpdateSource(string source, List<SizeModel> updateModels) {
        HttpClient httpClient = _httpClientFactory.CreateClient();
        string formattedUrl = GetFormattedUrl(source);
        httpClient.BaseAddress = new Uri(formattedUrl);
        httpClient.DefaultRequestHeaders.Add("username", _username);
        httpClient.DefaultRequestHeaders.Add("password", _password);
        httpClient.Timeout = TimeSpan.FromMinutes(1);

        string json = JsonSerializer.Serialize(updateModels);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync("", content);
        //TODO: Response handling based on if the request was successful or not.
    }
}
