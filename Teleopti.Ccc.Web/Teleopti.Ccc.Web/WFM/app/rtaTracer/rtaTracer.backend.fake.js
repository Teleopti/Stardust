'use strict';

(function () {
	angular
		.module('wfm.rtaTracer')
		.service('RtaTracerBackendFake', constructor);

	constructor.$inject = ['BackendFaker'];

	function constructor(faker) {
		
		var service = {
			clear: clear,
			traceCalledForUserCode: null
		};

		function clear() {
			service.traceCalledForUserCode = null;
		}
		
		faker.fake(/\.\.\/api\/Tracer\/Trace(.*)/,
			function (params) {
				service.traceCalledForUserCode = params.userCode;
				return [200];
			});

		return service;
	}

})();
