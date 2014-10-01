define([
		'knockout','jquery'
], function (
	ko, $
	) {

	return function () {
		var self = this;
		var startPromise;

		self.hub = undefined;
		self.configuredUrls = ko.observableArray();
		self.services = ko.observableArray();
		self.busResults = ko.observable();

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

			startPromise.done(function () {
				self.hub.server
		            .addSubscription(subscription)
		            .done(function (route) {
		            	subscription.route = route;
		            	deferred.resolve();
		            });
			});

			return subscription;
		};

		var checkBusFunc;
		self.checkBus = function () {
			checkBusFunc();
		};

		self.initialize = function(options) {
			if (options) {
				self.hub = options.messageBroker;
				startPromise = options.signalR.start({url: '../../signalr'});
				checkBusFunc = options.checkBus;
				subscribe({ domainType: 'Teleopti.Interfaces.Domain.ITeleoptiDiagnosticsInformation' });
			}
			if (self.hub && self.hub.client) {
				self.hub.client.onEventMessage = function(notification, route) {
					self.busResults(notification);
				}
			}
		}
	};
});

