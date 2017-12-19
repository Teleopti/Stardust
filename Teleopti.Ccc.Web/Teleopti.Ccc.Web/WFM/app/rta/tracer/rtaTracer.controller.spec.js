﻿'use strict';

fdescribe('RtaTracerController', function () {
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
				Process: 'box1'
			})
			.withTracer({
				Process: 'box2'
			});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].process).toBe('box1');
		expect(vm.tracers[1].process).toBe('box2');

	});
	
	it('should display tracer properties', function () {
		var random = Math.random().toString().substring(2, 4);
		$fakeBackend.withTracer({
			Process: 'box1:hej',
			DataReceived: [{At: '2017-10-02 07:00:' + random}],
			ActivityCheckAt: '2017-10-02 09:21:' + random,
			Tracing: 'usercode34, Ashley Andeen' + random
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].process).toBe('box1:hej');
		expect(vm.tracers[0].dataReceived[0].at).toBe('2017-10-02 07:00:' + random);
		expect(vm.tracers[0].activityCheckAt).toBe('2017-10-02 09:21:' + random);
		expect(vm.tracers[0].tracing).toBe('usercode34, Ashley Andeen' + random);
	});

	it('should display tracer exception', function () {
		$fakeBackend.withTracer({
			Exception: 'something is broken'
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].exception).toBe('something is broken');
	});

	it('should display tracer received by and count', function () {
		$fakeBackend.withTracer({
			DataReceived: [{
				By: 'method',
				Count: 123
			}]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].dataReceived[0].by).toBe('method');
		expect(vm.tracers[0].dataReceived[0].count).toBe(123);
	});

	it('should display tracer received 2 times', function () {
		$fakeBackend.withTracer({
			DataReceived: [
				{
					By: 'method1',
					Count: 1
				},
				{
					By: 'method2',
					Count: 2
				}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].dataReceived[0].by).toBe('method1');
		expect(vm.tracers[0].dataReceived[0].count).toBe(1);
		expect(vm.tracers[0].dataReceived[1].by).toBe('method2');
		expect(vm.tracers[0].dataReceived[1].count).toBe(2);
	});

	it('should display tracer received something', function () {
		$fakeBackend.withTracer({
			DataReceived: [{Count: 1}]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].dataReceived[0].count).toBe(1);
	});

	it('should display tracer received something', function () {
		$fakeBackend.withTracer({
			DataReceived: [{Count: 0}]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].dataReceived[0].count).toBe(0);
	});

	it('should display tracer received nothing', function () {
		$fakeBackend.withTracer({
			DataReceived: null
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].dataReceived.length).toBe(0);
	});

	it('should display tracer received nothing', function () {
		$fakeBackend.withTracer({
			DataReceived: undefined
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracers[0].dataReceived.length).toBe(0);
	});

	it('should display user codes', function () {
		$fakeBackend.withTracedUser({
			User: 'usercode34, Ashley Andeen'
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracedUsers[0].user).toBe('usercode34, Ashley Andeen');
	});

	it('should display trace state code', function () {
		$fakeBackend.withTracedUser({
			States: [{StateCode: 'AUX12'}]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracedUsers[0].states[0].stateCode).toBe('AUX12');
	});

	it('should display 2 traces', function () {
		$fakeBackend.withTracedUser({
			States: [
				{StateCode: 'AUX12'},
				{StateCode: 'AUX13'}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracedUsers[0].states[0].stateCode).toBe('AUX12');
		expect(vm.tracedUsers[0].states[1].stateCode).toBe('AUX13');
	});

	it('should display trace line', function () {
		$fakeBackend.withTracedUser({
			States: [
				{Traces: ["Processing"]}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracedUsers[0].states[0].traces[0]).toBe('Processing');
	});

	it('should display trace line', function () {
		$fakeBackend.withTracedUser({
			States: [
				{Traces: ["ActivityCheck"]}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracedUsers[0].states[0].traces[0]).toBe('ActivityCheck');
	});

	it('should display 2 trace lines', function () {
		$fakeBackend.withTracedUser({
			States: [
				{Traces: ["Processing", "Processed"]}
			]
		});

		var c = $controllerBuilder.createController();
		var vm = c.vm;

		expect(vm.tracedUsers[0].states[0].traces[0]).toBe('Processing');
		expect(vm.tracedUsers[0].states[0].traces[1]).toBe('Processed');
	});

	it('should poll', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;

		$fakeBackend.withTracedUser({
			States: [
				{Traces: ["ActivityCheck"]}
			]
		});
		c.wait(1000);

		expect(vm.tracedUsers[0].states[0].traces[0]).toBe('ActivityCheck');
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