﻿using SoleMates.Website.Extensions.Sync.Adapters;
using SoleMates.Website.Extensions.Sync.Models;
using Umbraco.Commerce.Common.Events;
using Umbraco.Commerce.Core.Events.Notification;
using Umbraco.Commerce.Core.Models;

namespace SoleMates.Website.Extensions.EventHandlers;
public class OrderFinalizedHandler : NotificationEventHandlerBase<OrderFinalizedNotification> {
    private readonly OrderLineAdapter _orderLineAdapter;
    private readonly RestAdapter _restAdapter;

    public OrderFinalizedHandler(OrderLineAdapter orderLineAdapter, RestAdapter restAdapter) {
        _orderLineAdapter = orderLineAdapter;
        _restAdapter = restAdapter;
    }

    public override async void Handle(OrderFinalizedNotification evt) {
        var sizes = new List<SizeModel>();
        Guid storeId = evt.Order.StoreId;

        IReadOnlyCollection<OrderLineReadOnly> orderLines = evt.Order.OrderLines;
        foreach (OrderLineReadOnly line in orderLines) {
            SizeModel size = _orderLineAdapter.ConvertLineToSize(line, storeId);

            sizes.Add(size);
        }

        await _restAdapter.UpdateSource("http://IP_ADDRESS_HERE:8080/api/stock/poststockupdates", sizes);
    }
}
