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
			withUserCodeTrace: withUserCodeTrace
		};

		function clear() {
			service.traceCalledForUserCode = null;
			service.stopCalled = false;
			service.clearCalled = false;
			userCodeTraces = [];
			tracers = [];
		}

		var tracers = [];

		function withTracer(tracer) {
			tracers.push(tracer);
			return this;
		}

		var userCodeTraces = [];

		function withUserCodeTrace(userCodeTrace) {
			userCodeTraces.push(userCodeTrace);
			return this;
		}

		faker.fake(/\.\.\/api\/Tracer\/Traces(.*)/,
			function () {
				return [200, {
					Tracers: tracers,
					UserCodeTraces: userCodeTraces
				}];
			});

		faker.fake(/\.\.\/api\/Tracer\/Trace(.*)/,
			function (params) {
				service.traceCalledForUserCode = params.userCode;
				return [200];
			});

		faker.fake(/\.\.\/api\/Tracer\/Stop(.*)/,
			function () {
				service.stopCalled = true;
				return [200];
			});

		faker.fake(/\.\.\/api\/Tracer\/Clear(.*)/,
			function () {
				service.clearCalled = true;
				return [200];
			});
		
		return service;
	}

})();
