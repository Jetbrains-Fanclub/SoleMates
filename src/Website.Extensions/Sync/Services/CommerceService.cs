using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoleMates.Website.Extensions.Sync.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;

namespace SoleMates.Website.Extensions.Sync.Services;

public class CommerceService {
    private readonly IUmbracoCommerceApi _commerceApi;
    private readonly ILogger<CommerceService> _logger;
    private readonly StoreReadOnly _store; //cache the store, so we don't have to search for it on each request.
    private readonly Guid _defaultCurrencyGuid;

    public CommerceService(IUmbracoCommerceApi commerceApi, ILogger<CommerceService> logger) {
        _commerceApi = commerceApi;
        _logger = logger;

        try {
            _store = _commerceApi.GetStores()
               .First(); //We only have one store, so we can just get all and pick the first.
            _defaultCurrencyGuid = _commerceApi.GetDefaultCurrency(_store.Id).Id;
        } catch {
            _logger.LogError("CommerceService.TryGetCommerceStore() - No store was found. Make sure one is created in the backoffice.");
            throw new NullReferenceException("CommerceService.TryGetCommerceStore() - No store was found. Make sure one is created in the backoffice.");
        }

    }

    public void UpdateStoreProductStock(IContent sizeNode, SizeModel sizeModel) {
        _commerceApi.SetProductStock(_store.Id, sizeNode.Key.ToString(), sizeModel.Stock);
    }

    public void UpdateStoreProductPrice(IContent seriesNode, SeriesModel seriesModel) {
        var prices = new KeyValuePair<Guid, decimal>(_defaultCurrencyGuid, seriesModel.Price);
        seriesNode.SetValue("price", JsonConvert.SerializeObject(prices));
    }
}
