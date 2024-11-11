using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SoleMates.Website.Extensions.Fetch.NodeHandlers;

/// <summary>
/// This class is responsible for handling the base <see cref="IContent"/> nodes, 'site' and 'products'.
/// </summary>
public class BaseNodesHandler {
  private readonly IContentService _contentService;

  public BaseNodesHandler(IContentService contentService) {
    _contentService = contentService;
  }
  /// <summary>
  /// Tries to get the <see cref="IContent"/> 'Products' node, which is the parent node of all <see cref="IContent"/> 'series' nodes. <br/>
  /// Throws a <see cref="NullReferenceException"/> if no nodes with the aliases 'site' and 'products' are found.<br/>
  /// The exception is handled by HangFire, and will be shown in the dashboard under "retries".
  /// </summary>
  /// <exception cref="NullReferenceException"></exception>
  public IContent TryGetProductsNode() {
    IContent? siteNode = _contentService.GetRootContent()
      .Where((node) => node.ContentType.Alias == "site")
      .FirstOrDefault();

    if (siteNode is null) {
      //TODO: Logging here - Prolly make it a error.
      throw new NullReferenceException("SeriesNodesHandler.TryGetProductsNode(): siteNode is null.");
    }

    IContent? productsNode = _contentService.GetPagedChildren(siteNode.Id, 0, int.MaxValue, out _)
      .Where((node) => node.ContentType.Alias == "products")
      .FirstOrDefault();

    if (productsNode is null) {
      //TODO: Logging here - Prolly make it a error.
      throw new NullReferenceException("SeriesNodesHandler.TryGetProductsNode(): productsNode is null.");
    }

    return productsNode;
  }
}
