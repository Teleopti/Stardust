
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
	
    ko.applyBindings(viewmodel);

    vm.initialize({
    	signalR: sigR
    });

});

