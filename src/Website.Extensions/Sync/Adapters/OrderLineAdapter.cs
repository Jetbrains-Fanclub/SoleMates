using SoleMates.Website.Extensions.Sync.Models;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;

namespace SoleMates.Website.Extensions.Sync.Adapters;
public class OrderLineAdapter {
  private readonly IUmbracoCommerceApi _commerceApi;

  public OrderLineAdapter(IUmbracoCommerceApi umbracoCommerceApi) {
    _commerceApi = umbracoCommerceApi;
  }


  public SizeModel ConvertLineToSize(OrderLineReadOnly line, Guid storeId) {

    string productReferece = line.ProductReference;

    return new SizeModel(
      SKU: line.Sku,
      Size: -1, //not used by the ERP to update the stock.
      Stock: (int)(_commerceApi.GetProductStock(storeId, productReferece))
    );
  }
}
