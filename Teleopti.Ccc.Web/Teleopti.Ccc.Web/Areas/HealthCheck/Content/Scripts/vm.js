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
		var localStorageIsSupported = typeof (Storage) !== 'undefined';

		self.hasPermission = ko.observable(false);
		self.hub = undefined;
		self.configuredUrls = ko.observableArray();
		self.services = ko.observableArray();
		self.etlJobHistory = ko.observableArray();
		self.readModelCheckStartDate = ko.observable();
		self.readModelCheckEndDate = ko.observable();
		self.readmodelCheckIsRunning = ko.observable(false);
		self.readModelsFixIsRunning = ko.observable(false);
		self.readModelCheckAndFixJobIsRunning = ko.observable(false);
		self.reinitReadModelsJobIsRunning = ko.observable(false);
		self.readModelCheckJobPollingResult = ko.observable('anything');
		self.readModelFixJobPollingResult = ko.observable('anything');
		self.readModelCheckAndFixJobPollingResult = ko.observable('anything');
		self.reinitReadModelsJobPollingResult = ko.observable('anything');
		self.logObjects = ko.observable();
		self.StardustJobId = ko.observable('');
		self.StardustSuccess = ko.observable(false);
		self.HangfireFailCount = ko.observable(0);
		self.TimesChecked = 0;

		self.iHaveEmptiedTables = ko.observable(false);

		if (localStorageIsSupported) {
			self.trackReadModelCheckId = ko.observable(localStorage.trackReadModelCheckId);
			self.trackReadModelFixId = ko.observable(localStorage.trackReadModelFixId);
			self.readModelCheckAndFixJobId = ko.observable(localStorage.readModelCheckAndFixJobId);
			self.reinitReadModelsJobId = ko.observable(localStorage.reinitReadModelsJobId);
		} else {
			self.trackReadModelCheckId = ko.observable();
			self.trackReadModelFixId = ko.observable();
			self.readModelCheckAndFixJobId = ko.observable();
			self.reinitReadModelsJobId = ko.observable();
		}

		self.getReadmodelCheckUrl = function (jobId) {
			if (typeof jobId !== 'string') {
				jobId = self.trackReadModelCheckId();
			}
			return self.getJobUrl(jobId);
		};

		self.getReadmodelFixUrl = function (jobId) {
			if (typeof jobId !== 'string') {
				jobId = self.trackReadModelFixId();
			}
			return self.getJobUrl(jobId);
		};

		self.getCheckAndFixJobUrl = function(jobId) {
			if (typeof jobId !== 'string') {
				jobId = self.readModelCheckAndFixJobId();
			}
			return self.getJobUrl(jobId);
		};

		self.getReinitJobUrl = function(jobId) {
			if (typeof jobId !== 'string') {
				jobId = self.reinitReadModelsJobId();
			}
			return self.getJobUrl(jobId);
		};

		self.getJobUrl = function getJobUrl(jobId) {
			if (!jobId) {
				throw new Error('Job ID cannot be falsy');
			}
			return 'StardustDashboard/job/' + jobId;
		};

		function pollJobStatus(jobId, onComplete) {
			if (!jobId) {
				return;
			}
			var polling = setInterval(function pollServer() {
				var jobUrl = self.getJobUrl(jobId);
				http.get(jobUrl).done(function (data) {					
					if (!data || data.Result) {
						clearInterval(polling);
						onComplete(data);
					}
				});
				return pollServer;
			}(), 5000);
		}

		function pollCheckJob(jobId) {
			self.readmodelCheckIsRunning(true);
			self.readModelCheckJobPollingResult('anything');
			pollJobStatus(jobId, function checkJobPollingDone(data) {
				self.readModelCheckJobPollingResult(data);
				if (data && data.Result) {
					var job = JSON.parse(data.Serialized);
					self.readModelCheckStartDate(job.StartDate.substr(0, 10));
					self.readModelCheckEndDate(job.EndDate.substr(0, 10));
				}
				self.readmodelCheckIsRunning(false);
			});
		}

		function pollFixJob(jobId) {
			self.readModelsFixIsRunning(true);
			self.readModelFixJobPollingResult('anything');
			pollJobStatus(jobId, function fixJobPollingDone(data) {
				self.readModelFixJobPollingResult(data);
				self.readModelsFixIsRunning(false);
			});
		}

		function pollCheckAndFixJob(jobId) {
			self.readModelCheckAndFixJobIsRunning(true);
			self.readModelCheckAndFixJobPollingResult('anything');
			pollJobStatus(jobId, function(data) {
				self.readModelCheckAndFixJobPollingResult(data);
				self.readModelCheckAndFixJobIsRunning(false);
			});
		}

		function pollReinitReadModelsJob(jobId) {
			self.reinitReadModelsJobIsRunning(true);
			self.reinitReadModelsJobPollingResult('anything');
			pollJobStatus(jobId, function(data) {
				self.reinitReadModelsJobPollingResult(data);
				self.reinitReadModelsJobIsRunning(false);
			});
		}

		if (self.trackReadModelCheckId()) {
			pollCheckJob(self.trackReadModelCheckId());
		}

		if (self.trackReadModelFixId()) {
			pollFixJob(self.trackReadModelFixId());
		}

		if (self.readModelCheckAndFixJobId()) {
			pollCheckAndFixJob(self.readModelCheckAndFixJobId());
		}

		if (self.reinitReadModelsJobId()) {
			pollReinitReadModelsJob(self.reinitReadModelsJobId());
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

		var checkReadModelFunc;
		self.checkReadModel = function () {
			var cb = function(id) {
				self.trackReadModelCheckId(id);
				if (localStorageIsSupported) {
					localStorage.trackReadModelCheckId = id;
				}
				pollCheckJob(id);
			};
			checkReadModelFunc(cb);
		};

		var fixReadModelFunc;
		self.fixReadModel = function () {
			var cb = function(id) {
				self.trackReadModelFixId(id);
				if (localStorageIsSupported) {
					localStorage.trackReadModelFixId = id;
				}
				pollFixJob(id);
			};
			fixReadModelFunc(cb);
		};

		var checkAndFixReadModelsFunc;
		self.checkAndFixReadModels = function() {
			var cb = function(id) {
				self.readModelCheckAndFixJobId(id);
				if (localStorageIsSupported) {
					localStorage.readModelCheckAndFixJobId = id;
				}
				pollCheckAndFixJob(id);
			};
			checkAndFixReadModelsFunc(cb);
		};

		var toggleIsEnabled;
		self.HealthCheck_EasyValidateAndFixReadModels_39696 = ko.observable(false);
		self.HealthCheck_ReinitializeReadModels_39697 = ko.observable(false);

		var reinitReadModelsFunc;
		self.reinitReadModels = function () {
			var cb = function(id) {
				self.reinitReadModelsJobId(id);
				if (localStorageIsSupported) {
					localStorage.reinitReadModelsJobId = id;
				}
				pollReinitReadModelsJob(id);
			};
			reinitReadModelsFunc(cb);
		};

		self.initialize = function(options) {
			if (options) {
				self.hub = options.messageBroker;
				startPromise = options.signalR.start();
				subscribe({
					domainType: 'ITeleoptiDiagnosticsInformation', businessUnitId: Teleopti.BusinessUnitId, datasource: Teleopti.DataSource
				});
				loadEtlHistory = options.loadEtlHistory;
				checkReadModelFunc = options.requestReadModelCheck;
				fixReadModelFunc = options.requestReadModelFix;
				checkAndFixReadModelsFunc = options.checkAndFixReadModels;
				toggleIsEnabled = options.toggleIsEnabled;
				reinitReadModelsFunc = options.requestReinitReadModels;
			}

			if (loadEtlHistory) {
				loadEtlHistory(true);
			}
			if (toggleIsEnabled) {
				var toggleNameFor39696 = 'HealthCheck_EasyValidateAndFixReadModels_39696';
				var toggleNameFor39697 = 'HealthCheck_ReinitializeReadModels_39697';
				toggleIsEnabled(toggleNameFor39696, function(toggleData) {
					self.HealthCheck_EasyValidateAndFixReadModels_39696(toggleData.IsEnabled);
				});
				toggleIsEnabled(toggleNameFor39697, function (toggleData) {
					self.HealthCheck_ReinitializeReadModels_39697(toggleData.IsEnabled);
				});
			}
		}

		self.loadAllEtlJobHistory = function () {
			if (loadEtlHistory) {
				loadEtlHistory(false);
			}
		}
	};
});

