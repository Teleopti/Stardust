
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

	var loadEtlHistory = function(showOnlyErrors) {
		http.get('api/HealthCheck/LoadEtlJobHistory', { date: new Date().toISOString(), showOnlyErrors: showOnlyErrors }).done(function (data) {
			vm.etlJobHistory(data);
		});
	};

	vm.initialize({
    	signalR: sigR,
    	messageBroker: $.connection.MessageBrokerHub,
		checkBus: function() {
			http.get('api/HealthCheck/CheckBus').done(function () {  });
		},
		loadEtlHistory: loadEtlHistory
    });

	http.get('api/HealthCheck/ServerDetails').done(function(data) {
		vm.configuredUrls(data.Data.UrlsReachable.UrlResults);
		vm.services(data.Data.RunningServices.Services);
	});

	http.get('api/HealthCheck/LoadEtlLogObject').done(function (data) {
		vm.logObjects(data);
	});
});

