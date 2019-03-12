(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("statusController", statusController, []);

	function statusController() {
		var vm = this;
		
		vm.statusSteps = [{
				id : 1,
				name : 'jågej',
				description : 'hohohoho!',
				secondsLimit : 50,
				lastPing : '14 seconds ago',
				pingUrl : 'http://www.google.se/status/ping/12312312..123--123-12-32-13-321-213',
				statusUrl : 'http://www.google.se/status/jågej'
			},{
				id : 3,
				name : 'gurka',
				description : 'potatismos med ris',
				secondsLimit : 5,
				lastPing : 'Just now',
				pingUrl : 'http://www.google.se/status/ping/12312222..123--123-12-32-13-321-213',
				statusUrl : 'http://www.google.se/status/gurka'
			}
		]
	}
})();