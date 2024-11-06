using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Website.Extensions.Fetch.Services;
public class NodeInitializerService {
  private readonly IContentService _contentService;

  public NodeInitializerService(IContentService contentService) {
    _contentService = contentService;
  }

  public IContent InitializeSeries(string seriesName) {
    IContent productsNode = GetProductsNode();
    if (productsNode is null) {
      throw new Exception("ERROR: Products node is null.");
    }

    IContent seriesNode = CreateIfNotExists(productsNode, seriesName, "productItem");

    return seriesNode;
  }

  private IContent GetProductsNode() {
    IContent? homepageNode = _contentService.GetRootContent()
        .FirstOrDefault(node => node.ContentType.Alias == "homepage");

    if (homepageNode is null) {
      throw new Exception("ERROR: Homepage node not found.");
    }

    IContent? productsNode = CreateIfNotExists(homepageNode, "Products", "products");
    if (productsNode is null) {
      throw new Exception("ERROR: Could not create or retrieve 'Products' node.");
    }

    return productsNode;
  }

  private IContent CreateIfNotExists(IContent parentNode, string nodeName, string contentTypeAlias) {
    IContent? targetNode = _contentService.GetPagedChildren(parentNode.Id, 0, int.MaxValue, out _)
        .FirstOrDefault(node => node.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));

    if (targetNode is not null) {
      return targetNode;
    }

    IContent newNode = _contentService.Create(nodeName, parentNode, contentTypeAlias);
    PublishResult publishResult = _contentService.SaveAndPublish(newNode, []);

    bool success = publishResult.Success;
    if (success) {
      return newNode;
    } else {
      return null; //TODO: FIX THIS LOGIC, MAYBE DO SOME LOGGING INSTEAD
    }
  }
}
