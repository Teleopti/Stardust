'use strict';

rtaTester.describe('RtaTracerController', function (it, fit, xit) {

	it('should be able to input user code', function (t) {
		var vm = t.createController();

		expect(vm.userCode).toBe('');
	});

	it('should start tracing user code', function (t) {
		var vm = t.createController();
		var userCode = t.randomString('usercode');

		t.apply(function () {
			vm.userCode = userCode;
		});
		t.apply(function () {
			vm.trace();
		});

		expect(t.backend.traceCalledForUserCode).toBe(userCode);
	});

	it('should display tracer process', function (t) {
		var process = t.randomString('box1:');
		t.backend.withTracer({
			Process: process
		});

		var vm = t.createController();

		expect(vm.tracers[0].process).toBe(process);
	});

	it('should display 2 tracers', function (t) {
		t.backend
			.withTracer({
				Process: 'box1'
			})
			.withTracer({
				Process: 'box2'
			});

		var vm = t.createController();

		expect(vm.tracers[0].process).toBe('box1');
		expect(vm.tracers[1].process).toBe('box2');

	});

	it('should display tracer properties', function (t) {
		var random = Math.random().toString().substring(2, 4);
		t.backend.withTracer({
			Process: 'box1:hej',
			DataReceived: [{At: '2017-10-02 07:00:' + random}],
			ActivityCheckAt: '2017-10-02 09:21:' + random,
			Tracing: 'usercode34, Ashley Andeen' + random
		});

		var vm = t.createController();

		expect(vm.tracers[0].process).toBe('box1:hej');
		expect(vm.tracers[0].dataReceived[0].at).toBe('2017-10-02 07:00:' + random);
		expect(vm.tracers[0].activityCheckAt).toBe('2017-10-02 09:21:' + random);
		expect(vm.tracers[0].tracing).toBe('usercode34, Ashley Andeen' + random);
	});

	it('should display tracer received by and count', function (t) {
		t.backend.withTracer({
			DataReceived: [{
				By: 'method',
				Count: 123
			}]
		});

		var vm = t.createController();

		expect(vm.tracers[0].dataReceived[0].by).toBe('method');
		expect(vm.tracers[0].dataReceived[0].count).toBe(123);
	});

	it('should display tracer received 2 times', function (t) {
		t.backend.withTracer({
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

		var vm = t.createController();

		expect(vm.tracers[0].dataReceived[0].by).toBe('method1');
		expect(vm.tracers[0].dataReceived[0].count).toBe(1);
		expect(vm.tracers[0].dataReceived[1].by).toBe('method2');
		expect(vm.tracers[0].dataReceived[1].count).toBe(2);
	});

	it('should display tracer received something', function (t) {
		t.backend.withTracer({
			DataReceived: [{Count: 1}]
		});

		var vm = t.createController();

		expect(vm.tracers[0].dataReceived[0].count).toBe(1);
	});

	it('should display tracer received something', function (t) {
		t.backend.withTracer({
			DataReceived: [{Count: 0}]
		});

		var vm = t.createController();

		expect(vm.tracers[0].dataReceived[0].count).toBe(0);
	});

	it('should display tracer received nothing', function (t) {
		t.backend.withTracer({
			DataReceived: null
		});

		var vm = t.createController();

		expect(vm.tracers[0].dataReceived.length).toBe(0);
	});

	it('should display tracer received nothing', function (t) {
		t.backend.withTracer({
			DataReceived: undefined
		});

		var vm = t.createController();

		expect(vm.tracers[0].dataReceived.length).toBe(0);
	});

	it('should display user codes', function (t) {
		t.backend.withTracedUser({
			User: 'usercode34, Ashley Andeen'
		});

		var vm = t.createController();

		expect(vm.tracedUsers[0].user).toBe('usercode34, Ashley Andeen');
	});

	it('should display trace state code', function (t) {
		t.backend.withTracedUser({
			States: [{StateCode: 'AUX12'}]
		});

		var vm = t.createController();

		expect(vm.tracedUsers[0].states[0].stateCode).toBe('AUX12');
	});

	it('should display 2 traces', function (t) {
		t.backend.withTracedUser({
			States: [
				{StateCode: 'AUX12'},
				{StateCode: 'AUX13'}
			]
		});

		var vm = t.createController();

		expect(vm.tracedUsers[0].states[0].stateCode).toBe('AUX12');
		expect(vm.tracedUsers[0].states[1].stateCode).toBe('AUX13');
	});

	it('should display trace line', function (t) {
		t.backend.withTracedUser({
			States: [
				{Traces: ["Processing"]}
			]
		});

		var vm = t.createController();

		expect(vm.tracedUsers[0].states[0].traces[0]).toBe('Processing');
	});

	it('should display trace line', function (t) {
		t.backend.withTracedUser({
			States: [
				{Traces: ["ActivityCheck"]}
			]
		});

		var vm = t.createController();

		expect(vm.tracedUsers[0].states[0].traces[0]).toBe('ActivityCheck');
	});

	it('should display 2 trace lines', function (t) {
		t.backend.withTracedUser({
			States: [
				{Traces: ["Processing", "Processed"]}
			]
		});

		var vm = t.createController();

		expect(vm.tracedUsers[0].states[0].traces[0]).toBe('Processing');
		expect(vm.tracedUsers[0].states[0].traces[1]).toBe('Processed');
	});

	it('should poll', function (t) {
		var vm = t.createController();

		t.backend.withTracedUser({
			States: [
				{Traces: ["ActivityCheck"]}
			]
		});
		t.wait(1000);

		expect(vm.tracedUsers[0].states[0].traces[0]).toBe('ActivityCheck');
	});

	it('should stop', function (t) {
		var vm = t.createController();

		t.apply(function () {
			vm.stop();
		});

		expect(t.backend.stopCalled).toBe(true);
	});

	it('should clear', function (t) {
		var vm = t.createController();

		t.apply(function () {
			vm.clear();
		});

		expect(t.backend.clearCalled).toBe(true);
	});

	it('should display tracer exception', function (t) {
		t.backend.withTracer({
			Exceptions: [{At: '2018-01-09 12:00:00', Exception: 'ArgumentException'}]
		});

		var vm = t.createController();

		expect(vm.tracers[0].exceptions[0].at).toBe('2018-01-09 12:00:00');
		expect(vm.tracers[0].exceptions[0].exception).toBe('ArgumentException');
	});

	it('should display tracer exception info', function (t) {
		t.backend.withTracer({
			Exceptions: [{Exception: 'ArgumentException', Info: 'alot of stuff to display'}]
		});

		var vm = t.createController();
		t.apply(function () {
			vm.tracers[0].exceptions[0].toggleDisplay();
		});

		expect(vm.exception).toBe('alot of stuff to display');
	});

	it('should hide tracer exception info', function (t) {
		t.backend.withTracer({
			Exceptions: [{Exception: 'ArgumentException', Info: 'alot of stuff to display'}]
		});

		var vm = t.createController();
		t.apply(function () {
			vm.tracers[0].exceptions[0].toggleDisplay();
		});
		t.apply(function () {
			vm.tracers[0].exceptions[0].toggleDisplay();
		});

		expect(vm.exception).toBeFalsy();
	});

	it('should display other tracer exception info', function (t) {
		t.backend.withTracer({
			Exceptions: [{
				Exception: 'ArgumentException',
				Info: 'alot of stuff to display'
			}, {
				Exception: 'ArgumentException',
				Info: 'this should be displayed'
			}]
		});

		var vm = t.createController();
		t.apply(function () {
			vm.tracers[0].exceptions[0].toggleDisplay();
		});
		t.apply(function () {
			vm.tracers[0].exceptions[1].toggleDisplay();
		});

		expect(vm.exception).toBe('this should be displayed');
	});
	
	it('should clear tracer exception info', function (t) {
		t.backend.withTracer({
			Exceptions: [{Exception: 'ArgumentException', Info: 'alot of stuff to display'}]
		});

		var vm = t.createController();
		t.apply(function () {
			vm.tracers[0].exceptions[0].toggleDisplay();
		});
		t.apply(function () {
			vm.clear();
		});

		expect(vm.exception).toBeFalsy();
	});

});