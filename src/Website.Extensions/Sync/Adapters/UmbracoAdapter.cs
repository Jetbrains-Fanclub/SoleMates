using SoleMates.Website.Extensions.Sync.Models;
using SoleMates.Website.Extensions.Sync.NodeHandlers;
using SoleMates.Website.Extensions.Sync.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Sync.Adapters;
public class UmbracoAdapter {
    private readonly IContentService _contentService;
    private readonly ISourceAdapter<SeriesModel, string, SizeModel> _restAdapter;
    private readonly HashingService _hashingService;
    private readonly SeriesNodesHandler _seriesNodesHandler;
    private readonly SizeNodesHandler _sizeNodesHandler;
    private readonly BaseNodesHandler _baseNodesHandler;

    public UmbracoAdapter(
      IContentService contentService,
      ISourceAdapter<SeriesModel, string, SizeModel> restAdapter,
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

    /// <summary> Fetches the REST API and creates any new nodes that didn't exist before. <br/>
    /// Updates the current stock amount afterwards. </summary>
    public async Task SyncEverythingFromRest() {
        List<SeriesModel> shoeSeries = await _restAdapter.FetchFromSource("http://IP_ADDRESS_HERE:8080/api/series/getallseries");

        HashSet<int> existingSeriesIds = _seriesNodesHandler.GetSeriesNodesIds();

        foreach (SeriesModel entry in shoeSeries) {
            //If we do not have a series node with a matching ID, we create a new SeriesNode and its children SizeNodes.
            if (!existingSeriesIds.Contains(entry.ID)) {
                IContent newSeriesNode = _seriesNodesHandler.CreateSeriesNode(entry);

                foreach (SizeModel size in entry.Sizes) {
                    _sizeNodesHandler.CreateSizeNode(size, newSeriesNode);
                }

                continue;
            }

            //Else we check if the series properties have changed, by comparing the hashes.
            IContent? seriesNode = _seriesNodesHandler.TryGetSeriesNodeById(entry.ID);
            if (seriesNode is null) {
                continue;
            }

            if (!_hashingService.TryCompareHashes(seriesNode, entry)) {
                _seriesNodesHandler.UpdateSeriesNode(entry, seriesNode);
            }

            //Then we check if the stock has changed, and update the stock if it has.
            IEnumerable<IContent> sizeNodes = _sizeNodesHandler.GetSizeNodes(seriesNode);

            foreach (SizeModel size in entry.Sizes) {
                IContent? currentSizeNode = _sizeNodesHandler.TryGetSizeNodeBySize(size.Size, seriesNode);
                if (currentSizeNode is null) {
                    continue;
                }

                _sizeNodesHandler.UpdateStockIfHasChanged(currentSizeNode, size);
            }
        }

        IContent productsNode = _baseNodesHandler.TryGetProductsNode();
        _contentService.SaveAndPublishBranch(productsNode, true, [""]);
    }

    /// <summary> Fetches the REST API and updates the stock amount if it has changed for the node. </summary>
    public async Task SyncStockFromRest() {
        List<SeriesModel> shoeSeries = await _restAdapter.FetchFromSource("http://IP_ADDRESS_HERE:8080/api/series/getallseries");
        IEnumerable<IContent> seriesNodes = _seriesNodesHandler.GetSeriesNodes();

        foreach (IContent seriesNode in seriesNodes) {
            IEnumerable<IContent> sizeNodes = _sizeNodesHandler.GetSizeNodes(seriesNode);
            SeriesModel? series = shoeSeries
                .FirstOrDefault((series) => series.ID == seriesNode.GetValue<int>("seriesId"));

            if (series is null) {
                continue;
            }

            foreach (IContent sizeNode in sizeNodes) {
                SizeModel? size = series.Sizes
                    .FirstOrDefault((size) => size.SKU == sizeNode.GetValue<string>("sku"));

                if (size is null) {
                    continue;
                }

                _sizeNodesHandler.UpdateStockIfHasChanged(sizeNode, size);
            }
        }
        IContent productsNode = _baseNodesHandler.TryGetProductsNode();
        _contentService.SaveAndPublishBranch(productsNode, true, [""]);
    }
}
