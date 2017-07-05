
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

	var checkAndFixReadModels = function (cb) {
		http.get('HealthCheck/ClearCheckReadModelResult').done(function () {
			http.get('HealthCheck/CheckAndFixReadModels', { start: new Date(vm.readModelCheckStartDate()).toISOString(), end: new Date(vm.readModelCheckEndDate()).toISOString() })
				.done(cb);
		});
	};

	var reinitReadModels = function (cb) {
		var period = {};
		try {
			period.start = new Date(vm.readModelCheckStartDate()).toISOString();
			period.end = new Date(vm.readModelCheckEndDate()).toISOString();
		} catch (e) {
			alert(e + '\n(Make sure to select valid dates)');
			console.error(e);
			return;
		}
		http.get('HealthCheck/ReinitializeReadModels', period)
			.done(cb);
	};

	var toggleIsEnabled = function(toggle, cb) {
		http.get('ToggleHandler/IsEnabled', {toggle: toggle}).done(cb);
	};

	vm.initialize({
    	signalR: sigR,
    	messageBroker: $.connection.MessageBrokerHub,
		loadEtlHistory: loadEtlHistory,
		requestReadModelCheck: requestReadModelCheck,
		requestReadModelFix: requestReadModelFix,
		checkAndFixReadModels: checkAndFixReadModels,
		toggleIsEnabled: toggleIsEnabled,
		requestReinitReadModels: reinitReadModels
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

