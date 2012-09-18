Teleopti.MyTimeWeb.MessageBroker = (function () {
	function _addSubscription(options) {
		var hub = $.connection.messageBrokerHub;
		$.connection.hub.url = options.url;

		$.connection.hub.error(options.errCallback);

		hub.onEventMessage = options.callback;

		$.connection.hub.start()
			.done(function () {
				hub.addSubscription({
					'DomainType': options.domainType,
					'BusinessUnitId': options.businessUnitId,
					'DataSource': options.datasource
				});
			});
	}


	return {
		AddSubscription: function (options) {
			/// <summary>Adds an event subscription.</summary>
			/// <param name="options">
			/// url = url to signalr server, 
			/// domainType = .net type to listen to, eg IPersistableScheduleData,
			/// businessUnitId = id of business unit,
			/// datasource = name of data source
			/// callback = function to call when successful subscription.
			/// errCallback (optional) = function to call if error (eg lost connection)
			/// </param>
			_addSubscription(options);
		}
	};
})(jQuery)