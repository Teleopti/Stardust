(function () {
	'use strict';
	angular
		.module('wfm.rtaTracer')
		.controller('RtaTracerController', constructor);

	constructor.$inject = ['$http', '$scope', 'rtaPollingService'];

	function constructor($http, $scope, rtaPollingService) {
		var vm = this;
		vm.userCode = '';
		vm.tracers = [];

		vm.trace = function () {
			$http.get('../api/RtaTracer/Trace', {params: {userCode: vm.userCode}});
		};
		
		vm.stop = function () {
			$http.get('../api/RtaTracer/Stop');
		};
		
		vm.clear = function () {
			$http.get('../api/RtaTracer/Clear');
		};

		var poller = rtaPollingService.create(function () {
			return $http.get('../api/RtaTracer/Traces').then(function (response) {
				vm.tracers = response.data.Tracers
					.map(function (tracer) {
						return {
							process: tracer.Process,
							dataReceivedAt: tracer.DataReceivedAt,
							activityCheckAt: tracer.ActivityCheckAt,
							tracing: tracer.Tracing
						};
					});
				vm.tracedUsers = response.data.TracedUsers
					.map(function (tracedUser) {
						return {
							user: tracedUser.User,
							states: (tracedUser.States || []).map(function (state) {
								return {
									stateCode: state.StateCode,
									traces: state.Traces
								}
							})
						};
					});
			});
		}, 1000).start();
		$scope.$on('$destroy', poller.destroy);

	}

})();
