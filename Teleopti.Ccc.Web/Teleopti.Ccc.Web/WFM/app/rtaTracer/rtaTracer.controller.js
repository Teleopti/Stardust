(function () {
	'use strict';
	angular
		.module('wfm.rtaTracer')
		.controller('RtaTracerController', RtaToolController);

	RtaToolController.$inject = [];

	function RtaToolController() {
		var vm = this;
		vm.process1 = "box1:487";
		vm.process2 = "box1:1476";
		vm.process3 = "box2:1254";
		vm.process4 = "box2:157";
		
		vm.usercode1 = "usercode34";
		vm.name1 = "Ashley Andeen";
		vm.usercode2 = "usercode2";

	}

})();
