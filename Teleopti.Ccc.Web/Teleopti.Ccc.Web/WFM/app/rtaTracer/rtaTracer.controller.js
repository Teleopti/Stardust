(function () {
	'use strict';
	angular
		.module('wfm.rtaTracer')
		.controller('RtaTracerController', constructor);

	constructor.$inject = ['$resource', '$http', '$interval'];

	function constructor($resource, $http, $interval) {
		var vm = this;
		vm.userCode = '';
		vm.tracers = [];

		vm.trace = function () {
			$http.get('../api/Tracer/Trace', {params: {userCode: vm.userCode}});
		};

		foo();
		
		function foo (){
			$http.get('../api/Tracer/Qwerty').then(function (response) {
				vm.tracers = response.data.Tracers
					.map(function (tracer) {
						return {
							process: tracer.Process,
							dataReceivedAt: tracer.DataReceivedAt,
							activityCheckAt: tracer.ActivityCheckAt,
							tracing: tracer.Tracing
						};
					});
				vm.userCodes = response.data.UserCodeTraces
					.map(function (userCodeTrace) {
						return {
							header: userCodeTrace.Header,
							traces: (userCodeTrace.Traces || []).map(function (trace) {
								return {
									stateCode: trace.StateCode,
									lines: trace.Lines
								}
							})
						};
					});
			});
		}
		
		$interval(foo, 1000);

		vm.process1 = "box1:487";
		vm.process2 = "box1:1476";
		vm.process3 = "box2:1254";
		vm.process4 = "box2:157";

		vm.usercode1 = "usercode34";
		vm.name1 = "Ashley Andeen";
		vm.usercode2 = "usercode2";

	}

})();
