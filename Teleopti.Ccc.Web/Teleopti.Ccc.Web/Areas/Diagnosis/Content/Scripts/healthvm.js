define([
		'knockout','jquery'
], function (
	ko, $
	) {

	return function () {
		var self = this;
		var startPromise;
		var loadEtlHistory;

		self.hub = undefined;
		self.configuredUrls = ko.observableArray();
		self.services = ko.observableArray();
		self.etlJobHistory = ko.observableArray();
		self.busResults = ko.observable();
		self.checkBusEnabled = ko.observable(true);

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
			self.checkBusEnabled(false);
			self.busResults(undefined);
			checkBusFunc();
		};

		self.initialize = function(options) {
			if (options) {
				self.hub = options.messageBroker;
				startPromise = options.signalR.start({url: '../../signalr'});
				checkBusFunc = options.checkBus;
				subscribe({
					domainType: 'ITeleoptiDiagnosticsInformation', businessUnitId: Teleopti.BusinessUnitId, datasource: Teleopti.DataSource
				});
				loadEtlHistory = options.loadEtlHistory;
			}
			if (self.hub && self.hub.client) {
				self.hub.client.onEventMessage = function(notification, route) {
					self.busResults(JSON.parse(atob(notification.BinaryData)));

					self.checkBusEnabled(true);
				}
			}
			if (loadEtlHistory) {
				loadEtlHistory(true);
			}
		}

		self.loadAllEtlJobHistory = function () {
			if (loadEtlHistory) {
				loadEtlHistory(false);
			}
		}
	};
});

