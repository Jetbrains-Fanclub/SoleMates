using Microsoft.Extensions.Logging;
using SoleMates.Website.Extensions.Sync.Models;
using SoleMates.Website.Extensions.Sync.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Sync.NodeHandlers;
public class SeriesNodesHandler {
    private readonly BaseNodesHandler _baseNodesHandler;
    private readonly IContentService _contentService;
    private readonly ILogger<SeriesNodesHandler> _logger;

    public SeriesNodesHandler(BaseNodesHandler baseNodesHandler, IContentService contentService, ILogger<SeriesNodesHandler> logger) {
        _baseNodesHandler = baseNodesHandler;
        _contentService = contentService;
        _logger = logger;
    }

    /// <summary> Returns all <see cref="IContent"/> 'Series Nodes' in the current Umbraco database Scope. <br/> 
    /// Searches for <see cref="IContent"/> 'Series Nodes' under 'site > products'. </summary>
    /// <returns> An <see cref="IEnumerable{IContent}"/> collection of the found <see cref="IContent"/> 'Series Nodes'. Might be empty, in which case that is logged. </returns>
    public IEnumerable<IContent> GetSeriesNodes() {
        IContent productsNode = _baseNodesHandler.TryGetProductsNode();
        IEnumerable<IContent> seriesNodes = _contentService.GetPagedChildren(productsNode.Id, 0, int.MaxValue, out _);

        if (!seriesNodes.Any()) {
            _logger.LogWarning("SeriesNodesHandler.GetSeriesNodes() - Returned an empty collection.");
        }

        return seriesNodes;
    }

    /// <summary> Tries to find the <see cref="IContent"/> 'Series' node, based on the passed int. </summary>
    /// <returns> The <see cref="IContent"/> 'Series' node. Might be <see langword="null"/>, in which case that is logged. </returns>
    public IContent? TryGetSeriesNodeById(int id) {
        IContent? matchedSeries = GetSeriesNodes()
            .FirstOrDefault((node) => node.GetValue<int>("seriesId") == id);

        if (matchedSeries is null) {
            _logger.LogWarning($"SeriesNodesHandler.TryGetSeriesNodeById() - Returned a null value, for ID {id}");
        }

        return matchedSeries;
    }

    /// <summary> Returns all existing <see cref="IContent"/> 'Series Nodes' ID properties as a <see cref="HashSet{int}"/>, for the current Umbraco database Scope. </summary>
    /// <returns> A <see cref="HashSet{int}"/> of the found <see cref="IContent"/> 'Series Nodes' IDs. Might be empty, in which case that is logged. </returns>
    public HashSet<int> GetSeriesNodesIds() {
        var ids = GetSeriesNodes()
          .Select((node) => node.GetValue<int>("seriesId"))
          .ToHashSet();

        if (ids.Count is 0) {
            _logger.LogWarning("SeriesNodesHandler.GetSeriesNodesIds() - Returned an empty collection of 'Series IDs'.");
        }

        return ids;
    }

    /// <summary> Creates and saves a new <see cref="IContent"/> 'Series Node'. </summary>
    /// <returns> The created <see cref="IContent"/> 'Series Node'. </returns>
    public IContent CreateSeriesNode(SeriesModel model) {
        IContent productsNode = _baseNodesHandler.TryGetProductsNode();

        string nodeName = $"{model.Brand} - {model.Name}";
        IContent seriesNode = _contentService.Create(nodeName, productsNode, "productItem");
        SetSeriesNodeProperties(model, seriesNode);

        _contentService.Save(seriesNode);
        return seriesNode;
    }

    /// <summary> Updates the properties of an <see cref="IContent"/> 'Series Node'. </summary>
    public void UpdateSeriesNode(SeriesModel updateModel, IContent seriesNode) {
        SetSeriesNodeProperties(updateModel, seriesNode);
        _contentService.Save(seriesNode);
    }

    private static void SetSeriesNodeProperties(SeriesModel model, IContent seriesNode) {
        seriesNode.SetValue("brand", model.Brand);
        seriesNode.SetValue("price", model.Price);
        seriesNode.SetValue("series", model.Name);
        seriesNode.SetValue("seriesId", model.ID);
        seriesNode.SetValue("hash", HashingService.GetFormattedHash(model));
    }
}
