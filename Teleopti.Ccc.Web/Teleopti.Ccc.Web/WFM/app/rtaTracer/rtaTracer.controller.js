(function () {
	'use strict';
	angular
		.module('wfm.rtaTracer')
		.controller('RtaTracerController', RtaToolController);

	RtaToolController.$inject = ['$resource'];

	function RtaToolController($resource) {
		var vm = this;
		vm.userCode = '';
		
		vm.trace = function () {
			return $resource('../api/Tracer/Trace', {}, {
				query: {
					method: 'GET'
				}
			}).query({userCode:'userCode2'}).$promise
		};
		
		
		vm.process1 = "box1:487";
		vm.process2 = "box1:1476";
		vm.process3 = "box2:1254";
		vm.process4 = "box2:157";
		
		vm.usercode1 = "usercode34";
		vm.name1 = "Ashley Andeen";
		vm.usercode2 = "usercode2";

	}

})();
