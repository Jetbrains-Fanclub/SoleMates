using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Sync.NodeHandlers;

/// <summary> This class is responsible for handling the base <see cref="IContent"/> nodes, 'site' and 'products'. </summary>
public class BaseNodesHandler {
  private readonly IContentService _contentService;
  private readonly ILogger<BaseNodesHandler> _logger;

  public BaseNodesHandler(IContentService contentService, ILogger<BaseNodesHandler> logger) {
    _contentService = contentService;
    _logger = logger;
  }

  /// <summary> Tries to get the <see cref="IContent"/> 'Products' node, which is the parent node of all <see cref="IContent"/> 'series' nodes. <br/>
  /// Throws a <see cref="NullReferenceException"/> if no nodes with the aliases 'site' and 'products' are found.<br/>
  /// The exception is handled by HangFire, and will be shown in the dashboard under "retries". </summary>
  /// <exception cref="NullReferenceException"></exception>
  public IContent TryGetProductsNode() {
    IContent? siteNode = _contentService.GetRootContent()
      .Where((node) => node.ContentType.Alias == "site")
      .FirstOrDefault();

    if (siteNode is null) {
      _logger.LogError("BaseNodesHandler.TryGetProductsNode() - No node of documenttype 'site' was found. Make sure one is created under 'Content'.");
      throw new NullReferenceException("SeriesNodesHandler.TryGetProductsNode() - No node of documenttype 'site' was found. Make sure one is created under 'Content'.");
    }

    IContent? productsNode = _contentService.GetPagedChildren(siteNode.Id, 0, int.MaxValue, out _)
      .Where((node) => node.ContentType.Alias == "products")
      .FirstOrDefault();

    if (productsNode is null) {
      _logger.LogError("BaseNodesHandler.TryGetProductsNode() - No node of documenttype 'products' was found. Make sure one is created under 'Content'.");
      throw new NullReferenceException("BaseNodesHandler.TryGetProductsNode() - No node of documenttype 'products' was found. Make sure one is created under 'Content'.");
    }

    return productsNode;
  }
}
