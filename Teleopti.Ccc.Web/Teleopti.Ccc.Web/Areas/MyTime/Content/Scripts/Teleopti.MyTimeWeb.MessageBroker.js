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
					'DataSource': options.datasource,
					'DomainReferenceId': options.referenceId
				});
			});
	}

	return {
		AddSubscription: function (options) {
			/// <summary>Adds an event subscription.</summary>
			/// <param name="options">
			/// url = url to signalr server,
			/// domainType = filter events on .net type, eg IPersistableScheduleData,
			/// businessUnitId = filter events on id of business unit,
			/// datasource = filter events based on name of data source,
			/// callback = function to call when successful subscription,
			/// errCallback (optional) = function to call if error (eg lost connection),
			/// referenceId (optional) = filter events on "reference id", eg agent id for schedules.
			/// </param>
			_addSubscription(options);
		}
	};
})(jQuery)