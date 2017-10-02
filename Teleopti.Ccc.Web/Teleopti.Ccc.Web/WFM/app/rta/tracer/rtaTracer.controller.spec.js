'use strict';

describe('RtaTracerController', function () {
	var
		$controllerBuilder,
		$fakeBackend,
		$httpBackend;

	var
		stateParams,
		scope;

	beforeEach(module('wfm.rtaTracer'));
	beforeEach(module('wfm.rtaTestShared'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_RtaTracerBackendFake_, _ControllerBuilder_) {
		$controllerBuilder = _ControllerBuilder_;
		$fakeBackend = _RtaTracerBackendFake_;

		scope = $controllerBuilder.setup('RtaTracerController');

		$fakeBackend.clear();
	}));

	it('should be able to input user code', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.userCode).toBe('');
	});

	it('should start tracing user code', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		var userCode = 'userCode' + Math.random().toString().substring(5);

		c.apply(function () {
			vm.userCode = userCode;
		});
		c.apply(function () {
			vm.trace();
		});

		expect($fakeBackend.traceCalledForUserCode).toBe(userCode);
	});

	it('should display tracer process', function () {
		var process = 'box1:' + Math.random().toString().substring(2, 6);
		$fakeBackend.withTracer({
			Process: process
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].process).toBe(process);
	});

	it('should display 2 tracers', function () {
		$fakeBackend
			.withTracer({
				DataReceivedAt: '2017-10-02 08:00'
			})
			.withTracer({
				DataReceivedAt: '2017-10-02 07:00'
			});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].dataReceivedAt).toBe('2017-10-02 08:00');
		expect(vm.tracers[1].dataReceivedAt).toBe('2017-10-02 07:00');

	});

	it('should display tracer properties', function () {
		var random = Math.random().toString().substring(2, 4);
		$fakeBackend.withTracer({
			Process: 'box1:hej',
			DataReceivedAt: '2017-10-02 07:00:' + random,
			ActivityCheckAt: '2017-10-02 09:21:' + random,
			Tracing: 'usercode34, Ashley Andeen' + random
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].process).toBe('box1:hej');
		expect(vm.tracers[0].dataReceivedAt).toBe('2017-10-02 07:00:' + random);
		expect(vm.tracers[0].activityCheckAt).toBe('2017-10-02 09:21:' + random);
		expect(vm.tracers[0].tracing).toBe('usercode34, Ashley Andeen' + random);
	});

	it('should display user codes', function () {
		$fakeBackend.withUserCodeTrace({
			Header: 'usercode34, Ashley Andeen'
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.userCodes[0].header).toBe('usercode34, Ashley Andeen');
	});

	it('should display trace state code', function () {
		$fakeBackend.withUserCodeTrace({
			Traces: [{StateCode: 'AUX12'}]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.userCodes[0].traces[0].stateCode).toBe('AUX12');
	});

	it('should display 2 traces', function () {
		$fakeBackend.withUserCodeTrace({
			Traces: [
				{StateCode: 'AUX12'},
				{StateCode: 'AUX13'}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.userCodes[0].traces[0].stateCode).toBe('AUX12');
		expect(vm.userCodes[0].traces[1].stateCode).toBe('AUX13');
	});

	it('should display trace line', function () {
		$fakeBackend.withUserCodeTrace({
			Traces: [
				{Lines: ["Processing"]}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.userCodes[0].traces[0].lines[0]).toBe('Processing');
	});

	it('should display trace line', function () {
		$fakeBackend.withUserCodeTrace({
			Traces: [
				{Lines: ["ActivityCheck"]}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.userCodes[0].traces[0].lines[0]).toBe('ActivityCheck');
	});
	
	it('should display 2 trace lines', function () {
		$fakeBackend.withUserCodeTrace({
			Traces: [
				{Lines: ["Processing", "Processed"]}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.userCodes[0].traces[0].lines[0]).toBe('Processing');
		expect(vm.userCodes[0].traces[0].lines[1]).toBe('Processed');
	});

	it('should poll', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		
		$fakeBackend.withUserCodeTrace({
			Traces: [
				{Lines: ["ActivityCheck"]}
			]
		});
		c.wait(1000);
		
		expect(vm.userCodes[0].traces[0].lines[0]).toBe('ActivityCheck');
	});
	
	it('should stop', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;

		c.apply(function () {
			vm.stop();
		});

		expect($fakeBackend.stopCalled).toBe(true);
	});

	it('should clear', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;

		c.apply(function () {
			vm.clear();
		});

		expect($fakeBackend.clearCalled).toBe(true);
	});
});