/// <reference path="../../../../Content/Scripts/jquery.signalR-1.2.0.js" />
/// <reference path="../../../../Content/Scripts/jquery-1.9.1.js" />


Teleopti.MyTimeWeb.MessageBroker = (function () {
	var listeners = [], conn, hub;

	function _oneTime(options) {
		hub = $.connection.MessageBrokerHub;
		$.connection.hub.url = options.url + '/signalr';
		if(options.errCallback) {
			$.connection.hub.error(options.errCallback);			
		}

		hub.client.onEventMessage = function (notification, route) {
			//cant use "dictionary" array. may be multiple subscription with same route
			$.each(listeners, function(key, value) {
				if (value.Route == route) {
					value.Callback(notification);
				}
			});
		};

		conn = $.connection.hub.start();
	}
	
	function _addSubscription(options) {
		if (hub==null) {
			_oneTime(options);
		}

		conn
			.done(function () {
				hub.server.addSubscription({
					'DomainType': options.domainType,
					'BusinessUnitId': options.businessUnitId,
					'DataSource': options.datasource,
					'DomainReferenceId': options.referenceId
				})
				.done(function (route) {
					listeners.push({ Route: route, Callback: options.callback });
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
		},
		ConvertMbDateTimeToJsDate: function (mbDateTime) {
			var splitDatetime = mbDateTime.split('T');
			var splitDate = splitDatetime[0].split('-');
			return new Date(splitDate[0].substr(1), splitDate[1] - 1, splitDate[2]);
		},
		Stop: function() {
			$.connection.hub.stop(false, true);
		}
	};
})(jQuery)