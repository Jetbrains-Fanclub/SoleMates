using SoleMates.Website.Extensions.Fetch.Models;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Fetch.Adapters;
public class UmbracoAdapter {
  private readonly IContentService _contentService;
  private readonly IAdapter<SeriesModel, string> _restAdapter;

  public UmbracoAdapter(IContentService contentService, IAdapter<SeriesModel, string> restAdapter) {
    _contentService = contentService;
    _restAdapter = restAdapter;
  }

  public async Task CreateNodesFromSource() {
    List<SeriesModel> shoeSeries = await _restAdapter.FetchFromSource("http://IP_ADDRESS_HERE:8080/api/series/getallseries");
    IContent? siteNode = _contentService.GetRootContent()
      .Where((node) => node.ContentType.Alias == "site")
      .FirstOrDefault();

    if (siteNode == null) {
      throw new Exception("'Site' node not found.");
    }

    IContent? productsNode = _contentService.GetPagedChildren(siteNode.Id, 0, int.MaxValue, out _)
      .Where((node) => node.ContentType.Alias == "products")
      .FirstOrDefault();

    if (productsNode == null) {
      throw new Exception("'Products' node not found.");
    }

    IEnumerable<IContent> existingSeries = _contentService.GetPagedChildren(productsNode.Id, 0, int.MaxValue, out _);
    var existingSeriesIds = existingSeries
      .Select((node) => node.GetValue<int>("seriesId"))
      .ToHashSet();

    foreach (SeriesModel entry in shoeSeries) {
      if (!existingSeriesIds.Contains(entry.ID)) {
        IContent seriesNode = _contentService.Create($"{entry.Brand} - {entry.Name}", productsNode, "productItem");
        SetSeriesNodeValues(entry, seriesNode);

        _contentService.Save(seriesNode);

        foreach (SizeModel size in entry.Sizes) {
          IContent sizeNode = _contentService.Create($"Size {size.Size}", seriesNode, "productSize");
          sizeNode.SetValue("sku", size.SKU);
          sizeNode.SetValue("stock", size.Stock);
          _contentService.Save(sizeNode);
        }
      } else {
        IContent? matchedNode = existingSeries
          .Where((node) => node.GetValue<int>("seriesId") == entry.ID)
          .FirstOrDefault();

        if (matchedNode == null) {
          continue;
        }

        string? lastHash = matchedNode.GetValue<string>("hash");
        string currentHash = GetFormattedHash(entry);

        if (string.IsNullOrWhiteSpace(lastHash)) {
          throw new Exception("LAST HASH NOT FOUND!");
        }

        if (!string.Equals(lastHash, currentHash)) {
          SetSeriesNodeValues(entry, matchedNode);
          _contentService.Save(matchedNode);
        }

        IContent? currentSeries = existingSeries
          .Where((node) => node.GetValue<int>("seriesId") == entry.ID)
          .FirstOrDefault();

        IEnumerable<IContent> sizeNodes = _contentService.GetPagedChildren(currentSeries.Id, 0, int.MaxValue, out _);

        foreach (SizeModel size in entry.Sizes) {
          IContent? currentSizeNode = sizeNodes
            .Where((node) => node.Name == $"Size {size.Size}")
            .FirstOrDefault();

          if (currentSizeNode == null) {
            continue;
          }

          int? currentSizeStock = currentSizeNode.GetValue<int>("stock");
          if (currentSizeStock is not null && currentSizeStock != size.Stock) {
            currentSizeNode.SetValue("stock", size.Stock);
            _contentService.Save(currentSizeNode);
          }
        }
      }
    }

    _contentService.SaveAndPublishBranch(productsNode, true, []);
  }


  private void SetSeriesNodeValues(SeriesModel model, IContent seriesNode) {
    seriesNode.SetValue("brand", model.Brand);
    seriesNode.SetValue("price", model.Price);
    seriesNode.SetValue("series", model.Name);
    seriesNode.SetValue("seriesId", model.ID);
    seriesNode.SetValue("hash", GetFormattedHash(model));
  }

  private string GetFormattedHash(SeriesModel model) {
    string concatination = $"{model.Brand}{model.Name}{model.Price}";
    byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(concatination));
    string unformattedHash = BitConverter.ToString(hashBytes);         //Unformatted looks like this:
    string formattedHash = unformattedHash.Replace("-", "").ToLower(); //5E-B6-3B-BB-E0-1E-EE-D0-93-CB-22-BB-8F-5A-CD-C3

    return formattedHash;
  }

}
