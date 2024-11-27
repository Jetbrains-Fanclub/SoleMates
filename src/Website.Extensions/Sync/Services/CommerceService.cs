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
                var store = _commerceApi.GetStore("soleMates");
                _commerceApi.SetProductStock(store.Id, sizeNode.Key.ToString(), sizeModel.Stock);
                uow.Complete();
            });
        } catch (Exception ex) {
            _logger.LogError("Error occured while setting product stock. Exception: {0} StackTrace: {1}", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public void UpdateStoreProductPrice(IContent seriesNode, SeriesModel seriesModel) {
        try {
            _commerceApi.Uow.Execute((uow) => {
                var store = _commerceApi.GetStore("soleMates");
                var prices = new KeyValuePair<Guid, decimal>(store.BaseCurrencyId!.Value, seriesModel.Price);
                seriesNode.SetValue("price", JsonConvert.SerializeObject(prices));
                uow.Complete();
            });
        } catch (Exception ex) {
            _logger.LogError("Error occurred while updating series price. Exception: {0} StackTrace: {1}", ex.Message, ex.StackTrace);
            throw;
        }
    }
}
