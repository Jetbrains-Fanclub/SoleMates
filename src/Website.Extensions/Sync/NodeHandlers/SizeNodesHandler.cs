using Microsoft.Extensions.Logging;
using SoleMates.Website.Extensions.Sync.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Sync.NodeHandlers;
public class SizeNodesHandler {
  private readonly IContentService _contentService;
  private readonly ILogger<SizeNodesHandler> _logger;

  public SizeNodesHandler(IContentService contentService, ILogger<SizeNodesHandler> logger) {
    _contentService = contentService;
    _logger = logger;
  }

  /// <summary> Returns all <see cref="IContent"/> 'Size Nodes' in the current Umbraco database Scope. Searches for <see cref="IContent"/> 'Size Nodes' under 'site > products > series'. </summary>
  /// <returns> An <see cref="IEnumerable{IContent}"/> collection of the found <see cref="IContent"/> 'Size Nodes'. Might be empty, in which case that is logged. </returns>
  public IEnumerable<IContent> GetSizeNodes(IContent seriesNode) {
    IEnumerable<IContent> sizeNodes = _contentService.GetPagedChildren(seriesNode.Id, 0, int.MaxValue, out _);

    if (!sizeNodes.Any()) {
      _logger.LogWarning($"SizeNodesHandler.GetSizeNodes() - Returned an empty collection for 'Series' node {seriesNode.Name}");
    }

    return sizeNodes;
  }

  /// <summary> Tries to find the <see cref="IContent"/> 'Size' node, based on the passed int, and the passed parent 'Series' node. </summary>
  /// <returns> The <see cref="IContent"/> 'Size' node. Might be <see langword="null"/>, in which case that is logged. </returns>
  public IContent? TryGetSizeNodeBySize(int size, IContent seriesNode) {
    IEnumerable<IContent> sizeNodes = GetSizeNodes(seriesNode);
    IContent? matchedSize = sizeNodes
      .Where((node) => node.Name == $"Size {size}")
      .FirstOrDefault();

    if (matchedSize is null) {
      _logger.LogWarning($"SizeNodesHandler.TryGetSizeNodeBySize() - Returned a null value, for size {size}");
    }

    return matchedSize;
  }

  /// <summary> Creates and saves a new <see cref="IContent"/> 'Size' node, under the passed parent 'Series' node. </summary>
  public void CreateSizeNode(SizeModel model, IContent seriesNode) {
    string sizeNodeName = $"Size {model.Size}";
    IContent sizeNode = _contentService.Create(sizeNodeName, seriesNode, "productSize");
    sizeNode.SetValue("sku", model.SKU);
    sizeNode.SetValue("stock", model.Stock);
    _contentService.Save(sizeNode);
  }

  /// <summary> Compares the property int 'stock' of the passed <see cref="IContent"/> 'Size' node, to the passed <see cref="SizeModel"/>. <br/>
  /// If the values are not the same, the 'Size' nodes' 'stock' property is updated to the models' value, which was just fetched from the ERP. </summary>
  public void UpdateStockIfHasChanged(IContent sizeNode, SizeModel newSizeModel) {
    int? currentStock = sizeNode.GetValue<int>("stock");

    if (currentStock is null) {
      _logger.LogWarning($"SizeNodesHandler.UpdateStockIfHasChanged() - Could not find property int 'stock' for the 'Size' node of {newSizeModel.SKU}");
      return;
    }

    if (currentStock != newSizeModel.Stock) {
      sizeNode.SetValue("stock", newSizeModel.Stock);
      _contentService.Save(sizeNode);
    }
  }
}
