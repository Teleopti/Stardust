define([
	'jquery',
	'signalrhubs.started'
], function (
	$,
	started
	) {

	var subscriptions = [];
	var hub = $.connection.MessageBrokerHub;

	hub.client.onEventMessage = function (notification, route) {
		var clone = subscriptions.slice(0);
		$.each(clone, function (key, value) {
			if (value.route == route) {
				value.callback(notification);
			}
		});
	};

	var subscribe = function (options) {
		var deferred = $.Deferred();

		var subscription = {};
		if (options.datasource) subscription.DataSource = options.datasource;
		if (options.businessUnitId) subscription.BusinessUnitId = options.businessUnitId;
		if (options.domainType) subscription.DomainType = options.domainType;
		if (options.domainId) subscription.DomainId = options.domainId;
		if (options.domainReferenceType) subscription.DomainReferenceType = options.domainReferenceType;
		if (options.domainReferenceId) subscription.DomainReferenceId = options.domainReferenceId;
		if (options.lowerBoundary) subscription.LowerBoundary = options.lowerBoundary;
		if (options.upperBoundary) subscription.UpperBoundary = options.upperBoundary;
		subscription.callback = options.callback;
		subscription.promise = deferred.promise();

		started.done(function () {
			hub.server
		            .addSubscription(subscription)
		            .done(function (route) {
		            	subscription.route = route;
		            	subscriptions.push(subscription);
		            	deferred.resolve();
		            });
		});

		return subscription;
	};

	var unsubscribe = function (subscription) {
		subscription.promise
		    .done(function () {
		    	hub.server.removeSubscription(subscription.route);
		    	subscriptions.splice(subscriptions.indexOf(subscription), 1);
		    });
	};

	return {
		started: started,
		subscribe: subscribe,
		unsubscribe: unsubscribe
	};

});
