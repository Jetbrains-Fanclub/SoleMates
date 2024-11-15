using SoleMates.Website.Extensions.EventHandlers;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Core.Events.Notification;
using Umbraco.Commerce.Extensions;

namespace SoleMates.Website.Extensions.Composers;
public static class CommerceBuilderExtensions {
  public static IUmbracoCommerceBuilder AddEventListeners(IUmbracoCommerceBuilder builder) {
    builder.WithNotificationEvent<OrderFinalizedNotification>()
      .RegisterHandler<OrderCreatedHandler>();

    return builder;
  }
}
