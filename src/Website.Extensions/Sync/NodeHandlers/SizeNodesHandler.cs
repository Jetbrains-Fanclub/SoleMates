using SoleMates.Website.Extensions.Sync.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Sync.NodeHandlers;
public class SizeNodesHandler {
  private readonly IContentService _contentService;

  public SizeNodesHandler(IContentService contentService) {
    _contentService = contentService;
  }

  /// <summary>
  /// Returns all <see cref="IContent"/> 'Size Nodes' in the current Umbraco database Scope. Searches for <see cref="IContent"/> 'Size Nodes' under 'site > products > series'.
  /// </summary>
  /// <returns>
  /// An <see cref="IEnumerable{IContent}"/> collection of the found <see cref="IContent"/> 'Size Nodes'. Might be empty, in which case that is logged.
  /// </returns>
  public IEnumerable<IContent> GetSizeNodes(IContent seriesNode) {
    IEnumerable<IContent> sizeNodes = _contentService.GetPagedChildren(seriesNode.Id, 0, int.MaxValue, out _);

    if (!sizeNodes.Any()) {
      //TODO: Logging here - Prolly make it a warning.
      //TODO: Might not be a bad idea to make it nullable then let UmbracoAdapter handle that, and just stick to logging here.
    }

    return sizeNodes;
  }

  public IContent TryGetSizeNodeBySize(int size, IContent seriesNode) {
    IEnumerable<IContent> sizeNodes = GetSizeNodes(seriesNode);
    IContent? matchedSize = sizeNodes
      .Where((node) => node.Name == $"Size {size}")
      .FirstOrDefault();

    if (matchedSize is null) {
      //TODO: Logging here -prolly make it a warning.
      //TODO: Null handling here as well. Maybe just throw.
    }

    return matchedSize;
  }

  /// <summary>
  /// Creates and saves a new <see cref="IContent"/> 'Size Node'.
  /// </summary>
  public void CreateSizeNode(SizeModel model, IContent seriesNode) {
    string sizeNodeName = $"Size {model.Size}";
    IContent sizeNode = _contentService.Create(sizeNodeName, seriesNode, "productSize");
    sizeNode.SetValue("sku", model.SKU);
    sizeNode.SetValue("stock", model.Stock);
    _contentService.Save(sizeNode);
  }

  public void UpdateStockIfHasChanged(IContent sizeNode, SizeModel newSizeModel) {
    int? sizeStock = sizeNode.GetValue<int>("stock");
    if (sizeStock is not null && sizeStock != newSizeModel.Stock) { //TODO: Better naming here. They sound super similar.
      sizeNode.SetValue("stock", newSizeModel.Stock); //TODO: Maybe split the checks and log if sizeStock is null.
      _contentService.Save(sizeNode);
    }
  }
}
