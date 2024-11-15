using Umbraco.Commerce.Common.Events;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Events.Notification;

namespace SoleMates.Website.Extensions.EventHandlers;
public class OrderCreatedHandler : NotificationEventHandlerBase<OrderFinalizedNotification> {
  private readonly IUmbracoCommerceApi _commerceApi;

  public OrderCreatedHandler(IUmbracoCommerceApi commerceApi) {
    _commerceApi = commerceApi;
  }

  public override void Handle(OrderFinalizedNotification evt) {
    var order = evt.Order;
    throw new NotImplementedException();
  }
}
