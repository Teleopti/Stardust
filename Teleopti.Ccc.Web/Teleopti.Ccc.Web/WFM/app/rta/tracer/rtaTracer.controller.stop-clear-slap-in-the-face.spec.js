'use strict';

rtaTester.describe('RtaTracerController', function (it, fit, xit) {

	it('should display a slap in the face', function (t) {
		var process = t.randomString('box1:');
		t.backend.withTracer({
			Process: process
		});

		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeTruthy();
	});

	it('should not display a slap in the face', function (t) {
		var vm = t.createController();

		expect(vm.displaySlapInTheFace()).toBeFalsy();
	});

});