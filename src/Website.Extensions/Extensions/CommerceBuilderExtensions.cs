using Microsoft.Extensions.DependencyInjection;
using SoleMates.Website.Extensions.EventHandlers;
using SoleMates.Website.Extensions.Sync.Adapters;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Core.Events.Notification;
using Umbraco.Commerce.Extensions;

namespace SoleMates.Website.Extensions.Extensions;
public static class CommerceBuilderExtensions {
    public static IUmbracoCommerceBuilder AddCommerceEventListeners(this IUmbracoCommerceBuilder builder) {
        builder.Services.AddSingleton<RestAdapter>();
        builder.WithNotificationEvent<OrderFinalizedNotification>()
          .RegisterHandler<OrderFinalizedHandler>();

        return builder;
    }
}
