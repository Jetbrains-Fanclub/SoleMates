using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoleMates.Website.Extensions.Sync.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Core.Api;

namespace SoleMates.Website.Extensions.Sync.Services;

public class CommerceService {
    private readonly IUmbracoCommerceApi _commerceApi;
    private readonly ILogger<CommerceService> _logger;

    public CommerceService(IUmbracoCommerceApi commerceApi, ILogger<CommerceService> logger) {
        _commerceApi = commerceApi;
        _logger = logger;


    }

    public void UpdateStoreProductStock(IContent sizeNode, SizeModel sizeModel) {
        try {
            _commerceApi.Uow.Execute((uow) => {
                var store = _commerceApi.GetStores()
                    .First();
                _commerceApi.SetProductStock(store.Id, sizeNode.Key.ToString(), sizeModel.Stock);

                uow.Complete();
            });
        } catch {
            _logger.LogError("CommerceService.TryGetCommerceStore() - No store was found. Make sure one is created in the backoffice.");
            throw new NullReferenceException("CommerceService.TryGetCommerceStore() - No store was found. Make sure one is created in the backoffice.");
        }
    }

    public void UpdateStoreProductPrice(IContent seriesNode, SeriesModel seriesModel) {
        try {
            _commerceApi.Uow.Execute((uow) => {
                var store = _commerceApi.GetStores()
                    .First();
                var defaultCurrency = CommerceApi.Instance.GetDefaultCurrency(store.Id);
                var guid = defaultCurrency.Id;

                var prices = new KeyValuePair<Guid, decimal>(guid, seriesModel.Price);
                seriesNode.SetValue("price", JsonConvert.SerializeObject(prices));

                uow.Complete();
            });
        } catch {
            _logger.LogError("CommerceService.TryGetCommerceStore() - No store was found. Make sure one is created in the backoffice.");
            throw new NullReferenceException("CommerceService.TryGetCommerceStore() - No store was found. Make sure one is created in the backoffice.");
        }


    }
}
