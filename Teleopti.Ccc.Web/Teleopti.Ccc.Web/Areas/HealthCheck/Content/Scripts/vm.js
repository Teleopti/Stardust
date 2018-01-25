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
		self.readModelCheckAndFixJobIsRunning = ko.observable(false);
		self.reinitReadModelsJobIsRunning = ko.observable(false);
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
			self.readModelCheckAndFixJobId = ko.observable(localStorage.readModelCheckAndFixJobId);
			self.reinitReadModelsJobId = ko.observable(localStorage.reinitReadModelsJobId);
		} else {
			self.trackReadModelCheckId = ko.observable();
			self.readModelCheckAndFixJobId = ko.observable();
			self.reinitReadModelsJobId = ko.observable();
		}

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
			pollJobStatus(jobId, function checkJobPollingDone(data) {
				if (data && data.Result) {
					var job = JSON.parse(data.Serialized);
					self.readModelCheckStartDate(job.StartDate.substr(0, 10));
					self.readModelCheckEndDate(job.EndDate.substr(0, 10));
				}
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
				checkAndFixReadModelsFunc = options.checkAndFixReadModels;
				reinitReadModelsFunc = options.requestReinitReadModels;
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

