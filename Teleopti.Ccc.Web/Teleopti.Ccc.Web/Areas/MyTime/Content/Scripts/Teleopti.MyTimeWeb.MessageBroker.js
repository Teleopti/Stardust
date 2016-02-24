/// <reference path="../../../../Content/Scripts/jquery.signalR-2.2.0.js" />
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
			        try {
			            value.Callback(notification);
			        } catch (e) {
			            console.log('Failed to run callback for notification', e);
			        } 
					
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
					listeners.push({ Route: route, Callback: options.callback, Page: options.page });
				});
			});
	}
	function _remove(arr, lambda) {
	    for(var i = arr.length; i--;) {
	        if(lambda(arr[i])) {
	            arr.splice(i, 1);
	        }
	    }
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
		},
	    
		RemoveListeners: function(page) {
		    /// <summary>Removes all message callbacks that are used on the current page.</summary>
		    /// <param name="page">
            // The name of the current page
		    /// </param>
		    _remove(listeners, function(item) {
		        return item.Page != undefined && item.Page === page;
		    });
		}
	};
})(jQuery)