'use strict';

rtaTester.describe('RtaTracerController', function (it, fit, xit) {

	it('should display a slap in the face when tracer for tenant', function (t) {
		var process = t.randomString('box1:');
		t.backend.withTracer({
			Process: process,
			Tenant: "tenant"
		});

		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeTruthy();
	});

	it('should display a slap in the face when data received logs for tenant', function (t) {
		var process = t.randomString('box1:');
		t.backend.withTracer({
			Process: process,
			DataReceived: [{Tenant: "tenant "}]
		});

		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeTruthy();
	});

	it('should not display a slap in the face', function (t) {
		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeFalsy();
	});

	it('should not display a slap in the face when only logs without tenant', function (t) {
		var process = t.randomString('box1:');
		t.backend.withTracer({
			Process: process,
			DataReceived: [{Tenant: null}],
			DataEnqueuing: [{Tenant: null}],
			DataProcessing: [{Tenant: null}],
			ActivityCheck: [{Tenant: null}],
			Exceptions: [{Tenant: null}],
			Tenant: null
		});

		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeFalsy();
	});

	it('should display a slap in the face when data processing logs for tenant', function (t) {
		var process = t.randomString('box1:');
		t.backend.withTracer({
			Process: process,
			DataProcessing: [{Tenant: "tenant "}]
		});

		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeTruthy();
	});

	it('should display a slap in the face when activity check logs for tenant', function (t) {
		var process = t.randomString('box1:');
		t.backend.withTracer({
			Process: process,
			ActivityCheck: [{Tenant: "tenant "}]
		});

		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeTruthy();
	});

	it('should display a slap in the face when exception logs for tenant', function (t) {
		var process = t.randomString('box1:');
		t.backend.withTracer({
			Process: process,
			Exceptions: [{Tenant: "tenant "}]
		});

		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeTruthy();
	});

});