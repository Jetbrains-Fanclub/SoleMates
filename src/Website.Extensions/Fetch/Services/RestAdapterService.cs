using Microsoft.Extensions.Logging;
using System.Text.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Website.Extensions.Fetch.Models;

namespace Website.Extensions.Fetch.Services;
public class RestAdapterService : IAdapterService<SeriesModel, string> {
  private readonly IContentService _contentService;
  private readonly ILogger<RestAdapterService> _loggerService;
  private readonly NodeInitializerService _nodeInitializerService;
  private readonly IHttpClientFactory _httpClientFactory;
  private IContent? _lastSeries = null;
  private List<IContent> _sizeChildren = [];

  public RestAdapterService(IContentService contentService, ILogger<RestAdapterService> loggerService,
    NodeInitializerService nodeInitializerService, IHttpClientFactory httpClientFactory) {
    _contentService = contentService;
    _loggerService = loggerService;
    _nodeInitializerService = nodeInitializerService;
    _httpClientFactory = httpClientFactory;
  }

  public async Task CreateNodesFromSource() {
    List<SeriesModel> shoeSeries = await FetchFromSource("http://37.27.179.21:8080/api/shoe/getallshoes"); //TODO: Environment Variables
    foreach (SeriesModel entry in shoeSeries) {
      IContent seriesNode = _nodeInitializerService.InitializeSeries(entry.Name);
      seriesNode.SetValue("brand", entry.Brand);
      seriesNode.SetValue("price", entry.Price);
      _loggerService.LogInformation($"ITERATING Series {seriesNode}");

      foreach (SizeModel size in entry.Sizes) {
        IContent sizeNode = _contentService.Create($"Size {size.Size}", seriesNode, "productSize");
        sizeNode.SetValue("sku", size.SKU);
        sizeNode.SetValue("stock", size.Stock);
      }
    }
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
