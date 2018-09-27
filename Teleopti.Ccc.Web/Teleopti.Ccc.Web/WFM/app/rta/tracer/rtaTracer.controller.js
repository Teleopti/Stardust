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

		vm.unknownTenantMessage = 'Tenant is known for some log entries. These will not be cleared manually, but purged later automatically.';
		
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
			vm.exception = undefined;
		};

		vm.displaySlapInTheFace = function () {
			return vm.tracers.some(function (t) {
				if (t.tenant != null)
					return true;
				var dataReceived = t.dataReceived.some(function (t) {
					return t.tenant != null
				});
				var dataEnqueuing = t.dataEnqueuing.some(function (t) {
					return t.tenant != null
				});
				var dataProcessing = t.dataProcessing.some(function (t) {
					return t.tenant != null
				});
				var activityCheck = t.activityCheck.some(function (t) {
					return t.tenant != null
				});
				var exceptions = t.exceptions.some(function (t) {
					return t.tenant != null
				});
				return dataReceived || dataEnqueuing || dataProcessing || activityCheck || exceptions;
			});
		};

		var poller = rtaPollingService.create(function () {
			return $http.get('../api/RtaTracer/Traces').then(function (response) {
				vm.tracers = response.data.Tracers
					.map(function (tracer) {
						return {
							process: tracer.Process,
							tracing: tracer.Tracing,
							tenant: tracer.Tenant,
							dataReceived: (tracer.DataReceived || []).map(function (d) {
								return {
									at: d.At,
									by: d.By,
									count: d.Count,
									tenant: d.Tenant
								}
							}),
							dataEnqueuing: (tracer.DataEnqueuing || []).map(function (d) {
								return {
									at: d.At,
									count: d.Count,
									tenant: d.Tenant
								}
							}),
							dataProcessing: (tracer.DataProcessing || []).map(function (d) {
								return {
									at: d.At,
									count: d.Count,
									tenant: d.Tenant
								}
							}),
							activityCheck: (tracer.ActivityCheck || []).map(function (d) {
								return {
									at: d.At,
									tenant: d.Tenant
								}
							}),
							exceptions: (tracer.Exceptions || []).map(function (e) {
								return {
									exception: e.Exception,
									at: e.At,
									tenant: e.Tenant,
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
