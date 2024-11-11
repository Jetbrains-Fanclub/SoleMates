using SoleMates.Website.Extensions.Sync.Models;
using SoleMates.Website.Extensions.Sync.NodeHandlers;
using SoleMates.Website.Extensions.Sync.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Sync.Adapters;
public class UmbracoAdapter {
  private readonly IContentService _contentService;
  private readonly IAdapter<SeriesModel, string> _restAdapter;
  private readonly HashingService _hashingService;
  private readonly SeriesNodesHandler _seriesNodesHandler;
  private readonly SizeNodesHandler _sizeNodesHandler;
  private readonly BaseNodesHandler _baseNodesHandler;

  public UmbracoAdapter(
    IContentService contentService,
    IAdapter<SeriesModel, string> restAdapter,
    HashingService hashingService,
    SeriesNodesHandler seriesNodesHandler,
    SizeNodesHandler sizeNodesHandler,
    BaseNodesHandler baseNodesHandler
  ) {
    _contentService = contentService;
    _restAdapter = restAdapter;
    _hashingService = hashingService;
    _seriesNodesHandler = seriesNodesHandler;
    _sizeNodesHandler = sizeNodesHandler;
    _baseNodesHandler = baseNodesHandler;
  }

  //TODO: Better naming - This should be like 'Extended' ping, and then we will have a smaller fetch that only gets the stock changes, similar to 'Normal' ping.
  public async Task SyncEverythingFromRest() {                           //TODO: Environment Variable.
    List<SeriesModel> shoeSeries = await _restAdapter.FetchFromSource("http://37.27.179.21:8080/api/series/getallseries");

    HashSet<int> existingSeriesIds = _seriesNodesHandler.GetSeriesNodesIds();

    foreach (SeriesModel entry in shoeSeries) {
      //If we do not have a series node with a matching ID, we create a new SeriesNode and its children SizeNodes.
      if (!existingSeriesIds.Contains(entry.ID)) {
        IContent newSeriesNode = _seriesNodesHandler.CreateSeriesNode(entry);

        foreach (SizeModel size in entry.Sizes) {
          _sizeNodesHandler.CreateSizeNode(size, newSeriesNode);
        }
      } else {
        //Else we check if the series properties have changed, by comparing the hashes.
        IContent seriesNode = _seriesNodesHandler.TryGetSeriesNodeById(entry.ID);
        bool areSameHashes = _hashingService.TryCompareHashes(seriesNode, entry);

        if (!areSameHashes) {
          _seriesNodesHandler.UpdateSeriesNode(entry, seriesNode);
        }

        //Then we check if the stock has changed, and update the stock if it has.
        IEnumerable<IContent> sizeNodes = _sizeNodesHandler.GetSizeNodes(seriesNode);

        foreach (SizeModel size in entry.Sizes) {
          IContent? currentSizeNode = _sizeNodesHandler.TryGetSizeNodeBySize(size.Size, seriesNode);

          if (currentSizeNode == null) {
            continue;
          }

          _sizeNodesHandler.UpdateStockIfHasChanged(currentSizeNode, size);
        }
      }
    }

    IContent productsNode = _baseNodesHandler.TryGetProductsNode();
    _contentService.SaveAndPublishBranch(productsNode, true, []);
  }

  public async Task SyncStockFromRest() {
    List<SeriesModel> shoeSeries = await _restAdapter.FetchFromSource("http://37.27.179.21:8080/api/series/getallseries");
    IEnumerable<IContent> seriesNodes = _seriesNodesHandler.GetSeriesNodes();

    foreach (IContent seriesNode in seriesNodes) {
      IEnumerable<IContent> sizeNodes = _sizeNodesHandler.GetSizeNodes(seriesNode);
      SeriesModel? series = shoeSeries
        .Where((series) => series.ID == seriesNode.GetValue<int>("seriesId"))
        .FirstOrDefault();

      if (series == null) {
        //TODO: Logging
        continue;
      }

      foreach (IContent sizeNode in sizeNodes) {
        SizeModel? size = series.Sizes
          .Where((size) => size.SKU == sizeNode.GetValue<string>("sku"))
          .FirstOrDefault();

        if (size == null) {
          //TODO: LOGGING
          continue;
        }

        _sizeNodesHandler.UpdateStockIfHasChanged(sizeNode, size);
      }
    }
    IContent productsNode = _baseNodesHandler.TryGetProductsNode();
    _contentService.SaveAndPublishBranch(productsNode, true, []);
  }
}
