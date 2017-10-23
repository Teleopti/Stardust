(function () {
	'use strict';
	angular
		.module('wfm.rtaTracer')
		.controller('RtaTracerController', constructor);

	constructor.$inject = ['$http', '$scope', 'rtaPollingService', '$stateParams'];

	function constructor($http, $scope, rtaPollingService, $stateParams) {
		var vm = this;
		vm.userCode = $stateParams.userCode || '';
		vm.tracers = [];

		vm.trace = function () {
			$http.get('../api/RtaTracer/Trace', {params: {userCode: vm.userCode}});
		};
		
		if ($stateParams.trace)
			vm.trace();
		
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
							dataReceivedBy: tracer.DataReceivedBy,
							dataReceivedCount: tracer.DataReceivedCount,
							activityCheckAt: tracer.ActivityCheckAt,
							tracing: tracer.Tracing,
							exception: tracer.Exception
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
