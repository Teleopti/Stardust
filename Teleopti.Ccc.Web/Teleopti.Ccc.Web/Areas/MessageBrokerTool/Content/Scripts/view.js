
define([
    'knockout',
    'vm',
	'signalRHubs'
], function (
    ko,
    viewmodel,
	sigR
	) {

    var vm = new viewmodel();
    ko.applyBindings(vm);
	
    vm.initialize({
    	signalR: sigR,
    	messageBroker: $.connection.MessageBrokerHub
    });

});

