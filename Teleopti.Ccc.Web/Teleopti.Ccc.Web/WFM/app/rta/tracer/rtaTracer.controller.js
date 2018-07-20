(function () {
	'use strict';
	angular
		.module('wfm.rtaTracer')
		.controller('RtaTracerController', constructor);

	constructor.$inject = ['$http', '$scope', 'rtaPollingService', '$stateParams', 'NoticeService'];

	function constructor($http, $scope, rtaPollingService, $stateParams, NoticeService) {
		var vm = this;
		vm.userCode = $stateParams.userCode || '';
		vm.tracers = [];
		var reminderInterval;

		vm.trace = function () {
			vm.highlightStopButton = true;
			
			var notice = 'You are now tracing user ' + vm.userCode + '. Please remember to manually stop' +
				' the tracer once you are done troubleshooting to avoid log errors.';
			NoticeService.warning(notice, 7000, true);
			
			reminderInterval = setInterval(function () {
				var reminder = 'Reminder: Please manually stop the tracer once you are done troubleshooting!';
				NoticeService.warning(reminder, 7000, true);
			}, 300000);
			
			$http.get('../api/RtaTracer/Trace', {params: {userCode: vm.userCode}});
		};

		if ($stateParams.trace)
			vm.trace();

		vm.stop = function () {
			clearInterval(reminderInterval);
			vm.highlightStopButton = false;
			$http.get('../api/RtaTracer/Stop');
		};

		vm.clear = function () {
			$http.get('../api/RtaTracer/Clear');
			vm.exception = undefined;
		};

		var poller = rtaPollingService.create(function () {
			return $http.get('../api/RtaTracer/Traces').then(function (response) {
				vm.tracers = response.data.Tracers
					.map(function (tracer) {
						return {
							process: tracer.Process,
							dataReceived: (tracer.DataReceived || []).map(function (d) {
								return {
									at: d.At,
									by: d.By,
									count: d.Count
								}
							}),
							activityCheckAt: tracer.ActivityCheckAt,
							tracing: tracer.Tracing,
							exceptions: (tracer.Exceptions || []).map(function (e) {
								return {
									exception: e.Exception,
									at: e.At,
									toggleDisplay: function () {
										if (vm.exception === e.Info)
											vm.exception = undefined;
										else
											vm.exception = e.Info
									}
								};
							})
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
