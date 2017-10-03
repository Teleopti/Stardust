'use strict';

(function () {
	angular
		.module('wfm.rtaTracer')
		.service('RtaTracerBackendFake', constructor);

	constructor.$inject = ['BackendFaker'];

	function constructor(faker) {

		var service = {
			clear: clear,
			traceCalledForUserCode: null,
			stopCalled: false,
			clearCalled: false,
			withTracer: withTracer,
			withTracedUser: withTracedUser
		};

		function clear() {
			service.traceCalledForUserCode = null;
			service.stopCalled = false;
			service.clearCalled = false;
			tracedUsers = [];
			tracers = [];
		}

		var tracers = [];

		function withTracer(tracer) {
			tracers.push(tracer);
			return this;
		}

		var tracedUsers = [];

		function withTracedUser(tracedUser) {
			tracedUsers.push(tracedUser);
			return this;
		}

		faker.fake(/\.\.\/api\/RtaTracer\/Traces(.*)/,
			function () {
				return [200, {
					Tracers: tracers,
					TracedUsers: tracedUsers
				}];
			});

		faker.fake(/\.\.\/api\/RtaTracer\/Trace(.*)/,
			function (params) {
				service.traceCalledForUserCode = params.userCode;
				return [200];
			});

		faker.fake(/\.\.\/api\/RtaTracer\/Stop(.*)/,
			function () {
				service.stopCalled = true;
				return [200];
			});

		faker.fake(/\.\.\/api\/RtaTracer\/Clear(.*)/,
			function () {
				service.clearCalled = true;
				return [200];
			});
		
		return service;
	}

})();
