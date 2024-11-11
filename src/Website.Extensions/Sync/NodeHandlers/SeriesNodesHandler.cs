using SoleMates.Website.Extensions.Fetch.Models;
using SoleMates.Website.Extensions.Fetch.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Fetch.NodeHandlers;
public class SeriesNodesHandler {
  private readonly HashingService _hashingService;
  private readonly BaseNodesHandler _baseNodesHandler;
  private readonly IContentService _contentService;

  public SeriesNodesHandler(HashingService hashingService, BaseNodesHandler baseNodesHandler, IContentService contentService) {
    _hashingService = hashingService;
    _baseNodesHandler = baseNodesHandler;
    _contentService = contentService;
  }

  /// <summary>
  /// Returns all <see cref="IContent"/> 'Series Nodes' in the current Umbraco database Scope. Searches for <see cref="IContent"/> 'Series Nodes' under 'site > products'.
  /// </summary>
  /// <returns>
  /// An <see cref="IEnumerable{IContent}"/> collection of the found <see cref="IContent"/> 'Series Nodes'. Might be empty, in which case that is logged.
  /// </returns>
  public IEnumerable<IContent> GetSeriesNodes() {
    IContent productsNode = _baseNodesHandler.TryGetProductsNode();
    IEnumerable<IContent> seriesNodes = _contentService.GetPagedChildren(productsNode.Id, 0, int.MaxValue, out _);

    if (!seriesNodes.Any()) {
      //TODO: Logging here - Prolly make it a warning.
    }

    return seriesNodes;
  }

  public IContent TryGetSeriesNodeById(int id) {
    IContent? matchedSeries = GetSeriesNodes()
      .Where((node) => node.GetValue<int>("seriesId") == id)
      .FirstOrDefault();

    if (matchedSeries is null) {
      //TODO: Logging here -prolly make it a warning.
      //TODO: Null handling here as well. Maybe just throw.
    }

    return matchedSeries;
  }

  /// <summary>
  /// Returns all existing <see cref="IContent"/> 'Series Nodes' ID properties as a <see cref="HashSet{int}"/>, for the current Umbraco database Scope.
  /// </summary>
  /// <returns>
  /// A <see cref="HashSet{int}"/> of the found <see cref="IContent"/> 'Series Nodes' IDs. Might be empty, in which case that is logged.
  /// </returns>
  public HashSet<int> GetSeriesNodesIds() {
    var ids = GetSeriesNodes()
      .Select((node) => node.GetValue<int>("seriesId"))
      .ToHashSet();

    if (ids.Count == 0) {
      //TODO: Logging here - Prolly make it a warning.
    }

    return ids;
  }

  /// <summary>
  /// Creates and saves a new <see cref="IContent"/> 'Series Node'.
  /// </summary>
  /// <returns>
  /// The created <see cref="IContent"/> 'Series Node'.
  /// </returns>
  public IContent CreateSeriesNode(SeriesModel model) {
    IContent productsNode = _baseNodesHandler.TryGetProductsNode();

    string seriesNodeName = $"{model.Brand} - {model.Name}";
    IContent seriesNode = _contentService.Create(seriesNodeName, productsNode, "productItem");
    SetSeriesNodeProperties(model, seriesNode);

    _contentService.Save(seriesNode);
    return seriesNode;
  }

  public void UpdateSeriesNode(SeriesModel updateModel, IContent seriesNode) {
    SetSeriesNodeProperties(updateModel, seriesNode);
    _contentService.Save(seriesNode);
  }

  private void SetSeriesNodeProperties(SeriesModel model, IContent seriesNode) {
    seriesNode.SetValue("brand", model.Brand);
    seriesNode.SetValue("price", model.Price);
    seriesNode.SetValue("series", model.Name);
    seriesNode.SetValue("seriesId", model.ID);
    seriesNode.SetValue("hash", _hashingService.GetFormattedHash(model));
  }
}
