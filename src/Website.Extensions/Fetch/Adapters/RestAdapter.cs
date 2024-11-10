using SoleMates.Website.Extensions.Fetch.Models;
using System.Text.Json;

namespace SoleMates.Website.Extensions.Fetch.Adapters;
public class RestAdapter : IAdapter<SeriesModel, string> {
  private readonly IHttpClientFactory _httpClientFactory;

  public RestAdapter(IHttpClientFactory httpClientFactory) {
    _httpClientFactory = httpClientFactory;
  }

  public async Task<List<SeriesModel>> FetchFromSource(string source) {
    HttpClient httpClient = _httpClientFactory.CreateClient();
    string formattedUrl = GetFormattedUrl(source);
    httpClient.BaseAddress = new Uri(formattedUrl);
    httpClient.DefaultRequestHeaders.Add("username", "Hovedopgave"); //TODO: Environment Variables
    httpClient.DefaultRequestHeaders.Add("password", "TtXr@oA7M.PaNxCmsvL2WBMw");  //TODO: Environment Variables
    httpClient.Timeout = TimeSpan.FromMinutes(1);

    HttpResponseMessage response = await httpClient.GetAsync(source);

    JsonSerializerOptions options = new() {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    var seriesList = new List<SeriesModel>();

    try {
      seriesList = JsonSerializer.Deserialize<List<SeriesModel>>(response.Content.ReadAsStream(), options);
    } catch (Exception ex) { } //TODO: Exception handling - Prolly some logging stuff

    if (seriesList is null) {
      seriesList = [];
      //TODO: DO SOME LOGGING HERE
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
      url = url + "/";
    }

    return url;
  }
}
