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
    private StoreReadOnly? _store;

    public CommerceService(IUmbracoCommerceApi commerceApi, ILogger<CommerceService> logger) {
        _commerceApi = commerceApi;
        _logger = logger;

        _commerceApi.Uow.Execute(true, (uow) => {
            _store = _commerceApi.GetStore("soleMates")!;
        });
    }

    public void UpdateStoreProductStock(IContent sizeNode, SizeModel sizeModel) {
        try {
            _commerceApi.SetProductStock(_store!.Id, sizeNode.Key.ToString(), sizeModel.Stock);
        } catch (Exception ex) {
            _logger.LogError("Error occured while setting product stock. Exception: {0} StackTrace: {1}", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public void UpdateStoreProductPrice(IContent seriesNode, SeriesModel seriesModel) {
        try {
            var price = new KeyValuePair<Guid, decimal>(_store!.BaseCurrencyId!.Value, seriesModel.Price);
            seriesNode.SetValue("price", JsonConvert.SerializeObject(price));
        } catch (Exception ex) {
            _logger.LogError("Error occurred while updating series price. Exception: {0} StackTrace: {1}", ex.Message, ex.StackTrace);
            throw;
        }
    }
}
