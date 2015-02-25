
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
		http.get('HealthCheck/Application/LoadEtlJobHistory', { date: new Date().toISOString(), showOnlyErrors: showOnlyErrors }).done(function (data) {
			vm.etlJobHistory(data);
		});
	};

	vm.initialize({
    	signalR: sigR,
    	messageBroker: $.connection.MessageBrokerHub,
		checkBus: function() {
			http.get('HealthCheck/Application/CheckBus').done(function () {  });
		},
		loadEtlHistory: loadEtlHistory
    });

	http.get('HealthCheck/Application/ServerDetails').done(function(data) {
		vm.configuredUrls(data.UrlsReachable.UrlResults);
		vm.services(data.RunningServices.Services);
	});

	http.get('HealthCheck/Application/LoadEtlLogObject').done(function (data) {
		vm.logObjects(data);
	});
});

