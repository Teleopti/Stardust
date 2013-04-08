define([
		'jquery',
		'noext!../../../../signalr/hubs'
	], function (
		$,
		signalrHubs
	) {
		
		var startPromise;

	    var subscriptions = [];
	    var hub = $.connection.MessageBrokerHub;

	    hub.client.onEventMessage = function (notification, route) {
	        $.each(subscriptions, function (key, value) {
	            if (value.route == route) {
	                value.callback(notification);
	            }
	        });
	    };

		var start = function () {
			$.connection.hub.url = 'signalr';
			startPromise = $.connection.hub.start();
		    return startPromise;
		};

		var subscribe = function (options) {
		    var subscription = {};
		    if (options.datasource) subscription.DataSource = options.datasource;
		    if (options.businessUnitId) subscription.BusinessUnitId = options.businessUnitId;
		    if (options.domainType) subscription.DomainType = options.domainType;
		    if (options.domainId) subscription.DomainId = options.domainId;
		    if (options.domainReferenceType) subscription.DomainReferenceType = options.domainReferenceType;
		    if (options.domainReferenceId) subscription.DomainReferenceId = options.domainReferenceId;
		    if (options.lowerBoundary) subscription.LowerBoundary = options.lowerBoundary;
		    if (options.upperBoundary) subscription.UpperBoundary = options.upperBoundary;

		    startPromise.done(function() {
		        hub.server
		            .addSubscription(subscription)
		            .done(function(route) {
		                subscriptions.push({
		                    route: route,
		                    callback: options.callback
		                });
		            });
		    });
		};
	    
		return {
		    start: start,
		    subscribe: subscribe
		};

	});
