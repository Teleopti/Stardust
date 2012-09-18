Teleopti.MyTimeWeb.MessageBroker = (function () {
	function _addSubscription(conn, subscription) {
		var hub = $.connection.messageBrokerHub;
		$.connection.hub.url = conn.url;

		$.connection.hub.error(conn.errCallback);

		hub.onEventMessage = conn.callback;

		$.connection.hub.start()
			.done(function () {
				hub.addSubscription({
					'DomainType': subscription.domainType,
					'BusinessUnitId': subscription.businessUnitId,
					'DataSource': subscription.datasource
				});
			});
	}


	return {
		AddSubscription: function (conn, subscription) {
			/// <summary>Adds an event subscription.</summary>
			/// <param name="conn">
			/// url = url to signalr server, 
			/// callback = function to call when successful subscription.
			/// errCallback (optional) = function to call if error (eg lost connection)
			/// </param>
			/// <param name="subscription">
			/// domainType = .net type to listen to, eg IPersistableScheduleData,
			/// businessUnitId = id of business unit,
			/// datasource = name of data source
			/// </param>
			_addSubscription(conn, subscription);
		}
	};
})(jQuery)