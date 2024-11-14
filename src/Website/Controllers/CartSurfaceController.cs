using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Common.Validation;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;

namespace SoleMates.Website.Controllers;

public class CartDto {
  public string ProductReference { get; set; } = "";
}

public class CartSurfaceController : SurfaceController {
  private readonly IUmbracoCommerceApi _commerceApi;

  public CartSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IUmbracoCommerceApi commerceApi)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider) {
    _commerceApi = commerceApi;
  }

  [HttpPost]
  public IActionResult AddToBasket(CartDto cart) {
    return _commerceApi.Uow.Execute(uow => {
      var store = CurrentPage.Value<StoreReadOnly>("store", fallback: Fallback.ToAncestors);

      if (store == null) {
        return RedirectToCurrentUmbracoPage();
      }

      try {
        var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
            .AsWritable(uow)
            .AddProduct(cart.ProductReference, 1);

        _commerceApi.SaveOrder(order);

        uow.Complete();

        TempData["SuccessFeedback"] = "Product added to cart";
        return RedirectToCurrentUmbracoPage();
      } catch (ValidationException ve) {
        throw new ValidationException(ve.Errors);
      } catch (Exception ex) {
        return RedirectToCurrentUmbracoPage();
        //logger.Error(ex, "An error occurred.");
      }
    });
  }
}
