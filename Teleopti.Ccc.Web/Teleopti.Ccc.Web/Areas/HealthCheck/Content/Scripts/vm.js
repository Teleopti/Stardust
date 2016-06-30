define([
		'knockout',
		'http'
], function (
	ko,
	http
	) {

	return function () {
		var self = this;
		var startPromise;
		var loadEtlHistory;

		self.hasPermission = ko.observable(false);
		self.hub = undefined;
		self.configuredUrls = ko.observableArray();
		self.services = ko.observableArray();
		self.etlJobHistory = ko.observableArray();
		self.busResults = ko.observable();
		self.readModelCheckStartDate = ko.observable();
		self.readModelCheckEndDate = ko.observable();
		self.readmodelCheckIsRunning = ko.observable(false);
		self.logObjects = ko.observable();
		self.checkBusEnabled = ko.observable(true);
		self.checkBusEnabled = ko.observable(true);
		self.StardustJobId = ko.observable('');
		self.StardustSuccess = ko.observable(false);
		self.HangfireFailCount = ko.observable(0);
		self.TimesChecked = 0;

		if (typeof (Storage) !== 'undefined') {
			self.trackReadModelCheckId = ko.observable(localStorage.trackReadModelCheckId);
			self.trackReadModelFixId = ko.observable(localStorage.trackReadModelFixId);
		} else {
			self.trackReadModelCheckId = ko.observable();
			self.trackReadModelFixId = ko.Observable();
		}

		self.getReadmodelCheckUrl = function (jobId) {
			return 'StardustDashboard/job/' + (typeof(jobId) === 'string'?jobId : self.trackReadModelCheckId());
		}
		
		if (self.trackReadModelCheckId()) {
			pollJobStatus(self.trackReadModelCheckId(), function (data) {
				if (data != null) {
					var job = JSON.parse(data.Serialized);
					self.readModelCheckStartDate(job.StartDate.substr(0, 10));
					self.readModelCheckEndDate(job.EndDate.substr(0, 10));
				}				
				self.readmodelCheckIsRunning(false);
			});
		}		

		function pollJobStatus(jobId, onComplete) {
			if (!jobId) {
				return;
			}
			self.readmodelCheckIsRunning(true);
			var polling = setInterval(function pollServer() {
				http.get(self.getReadmodelCheckUrl(jobId)).done(function (data) {					
					if (data == null || data.Result) {
						clearInterval(polling);
						onComplete(data);
					}
				});
				return pollServer;
			}(), 5000);
		}

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

		var checkReadModelFunc;
		self.checkReadModel = function () {
			var cb = function(id) {
				self.trackReadModelCheckId(id);
				if (typeof (Storage) !== 'undefined') {
					localStorage.trackReadModelCheckId = id;
				}
				pollJobStatus(id, function () { self.readmodelCheckIsRunning(false); });
			};
			checkReadModelFunc(cb);
		};

		var fixReadModelFunc;
		self.fixReadModel = function () {
			var cb = function(id) {
				self.trackReadModelFixId(id);
				if (typeof (Storage) !== 'undefined') {
					localStorage.trackReadModelFixId = id;
				}
				pollJobStatus(id, function () { self.readmodelCheckIsRunning(false); });
			};
			fixReadModelFunc(cb);
		};

		self.initialize = function(options) {
			if (options) {
				self.hub = options.messageBroker;
				startPromise = options.signalR.start();
				checkBusFunc = options.checkBus;
				subscribe({
					domainType: 'ITeleoptiDiagnosticsInformation', businessUnitId: Teleopti.BusinessUnitId, datasource: Teleopti.DataSource
				});
				loadEtlHistory = options.loadEtlHistory;
				checkReadModelFunc = options.requestReadModelCheck;
				fixReadModelFunc = options.requestReadModelFix;
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

