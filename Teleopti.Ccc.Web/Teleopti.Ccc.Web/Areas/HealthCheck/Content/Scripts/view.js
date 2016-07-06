
define([
    'knockout',
    'vm',
	'signalRHubs',
	'http'
], function (
    ko,
    viewmodel,
	sigR,
	http
	) {

    var vm = new viewmodel();
    ko.applyBindings(vm);

    http.get('HealthCheck/CheckStardust').done(function (data) {
    	vm.StardustJobId(data);
    	if(data === '00000000-0000-0000-0000-000000000000')//toggle off
    		vm.StardustSuccess(true);
    });

	var loadEtlHistory = function(showOnlyErrors) {
		http.get('api/HealthCheck/LoadEtlJobHistory', { date: new Date().toISOString(), showOnlyErrors: showOnlyErrors }).done(function (data) {
			vm.hasPermission(true);
			vm.etlJobHistory(data);
		});
	};

	var requestReadModelCheck = function (cb) {
		http.get('HealthCheck/ClearCheckReadModelResult').done(function() {
			http.get('HealthCheck/CheckReadModels', { start: new Date(vm.readModelCheckStartDate()).toISOString(), end: new Date(vm.readModelCheckEndDate()).toISOString() })
				.done(cb);
		});
	};

	var requestReadModelFix = function(cb) {
		http.get('HealthCheck/FixScheduleProjectionReadOnly').done(cb);
	};

	vm.initialize({
    	signalR: sigR,
    	messageBroker: $.connection.MessageBrokerHub,
		checkBus: function() {
			http.get('api/HealthCheck/CheckBus').done(function () {  });
		},
		loadEtlHistory: loadEtlHistory,
		requestReadModelCheck: requestReadModelCheck,
		requestReadModelFix: requestReadModelFix
    });

	http.get('HealthCheck/CheckStardust').done(function (data) {
		vm.StardustJobId(data);
	});

	http.get('HealthCheck/CheckHangfireFailedQueue').done(function (data) {
		vm.HangfireFailCount(data);
	});

	http.get('api/HealthCheck/ServerDetails').done(function(data) {
		vm.configuredUrls(data.Data.UrlsReachable.UrlResults);
		vm.services(data.Data.RunningServices.Services);
	});

	http.get('api/HealthCheck/LoadEtlLogObject').done(function (data) {
		vm.logObjects(data);
	});

	function checkStatus() {
		if (vm.StardustJobId() !== '') {
			http.get('StardustDashboard/job/' + vm.StardustJobId() ).done(function (data) {
				if (data !== null && data.Result === 'Success') {
					vm.StardustSuccess(true);
					return;
				}
			});
		}
		setTimeout(checkStardustJobStatus, 5000);
	}

	function checkStardustJobStatus() {
		if (vm.StardustSuccess() === true) {
			return;
		}

		if (vm.TimesChecked > 25) { return; }

			vm.TimesChecked++;

			setTimeout(checkStatus, 5000);
		
	}
	
	checkStardustJobStatus();
});

