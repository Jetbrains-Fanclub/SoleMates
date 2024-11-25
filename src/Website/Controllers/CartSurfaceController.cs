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

namespace SoleMates.Website.Controllers;

public class CartDto {
  public Guid OrderLineId { get; set; }
  public string ProductReference { get; set; } = "";
  public string ProductVariantReference { get; set; } = "";
  public OrderLineQuantityDto[] OrderLines { get; set; } = [];
}

public class OrderLineQuantityDto {
  public Guid Id { get; set; }
  public decimal Quantity { get; set; }
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
      var store = CurrentPage?.Value<StoreReadOnly>("store", fallback: Fallback.ToAncestors);

      if (store == null) {
        return RedirectToCurrentUmbracoPage();
      }

      try {
        var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
            .AsWritable(uow)
            .AddProduct(cart.ProductVariantReference, 1);

        if (!_commerceApi.TryReduceProductStock(store.Id, cart.ProductVariantReference, 1)) {
          TempData["Feedback"] = "Could not reduce stock amount";
          return RedirectToCurrentUmbracoPage();
        }

        _commerceApi.SaveOrder(order);

        uow.Complete();

        TempData["Feedback"] = "Product added to cart";
        return RedirectToCurrentUmbracoPage();
      } catch (ValidationException ve) {
        throw new ValidationException(ve.Errors);
      } catch (Exception ex) {
        return RedirectToCurrentUmbracoPage();
        //logger.Error(ex, "An error occurred.");
      }
    });
  }

  [HttpPost]
  public IActionResult UpdateCart(CartDto cart) {
    try {
      _commerceApi.Uow.Execute(uow => {
        var store = CurrentPage?.Value<StoreReadOnly>("store", fallback: Fallback.ToAncestors);

        if (store == null) { return; }

        var order = _commerceApi.GetCurrentOrder(store.Id)
        .AsWritable(uow);

        foreach (var orderLine in cart.OrderLines) {
          order.WithOrderLine(orderLine.Id)
          .SetQuantity(orderLine.Quantity);
        }

        _commerceApi.SaveOrder(order);

        uow.Complete();
      });
    } catch (ValidationException) {
      ModelState.AddModelError(string.Empty, "Failed to update cart");
      return CurrentUmbracoPage();
    }

    TempData["SuccessMessage"] = "Cart updated";

    return RedirectToCurrentUmbracoPage();
  }

  [HttpGet]
  public IActionResult RemoveFromCart(CartDto cart) {
    try {
      _commerceApi.Uow.Execute(uow => {
        var store = CurrentPage?.Value<StoreReadOnly>("store", fallback: Fallback.ToAncestors);

        if (store == null) { return; }

        var order = _commerceApi.GetOrCreateCurrentOrder(store.Id)
          .AsWritable(uow)
          .RemoveOrderLine(cart.OrderLineId);

        _commerceApi.SaveOrder(order);

        uow.Complete();
      });
    } catch (ValidationException) {
      ModelState.AddModelError(string.Empty, "Failed to remove product from cart");
      return CurrentUmbracoPage();
    }

    TempData["SuccessMessage"] = "Item removed";
    return RedirectToCurrentUmbracoPage();
  }
}