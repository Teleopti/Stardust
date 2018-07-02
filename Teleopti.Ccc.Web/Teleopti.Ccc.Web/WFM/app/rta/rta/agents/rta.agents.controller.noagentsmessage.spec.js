'use strict';
rtaTester.describe('RtaAgentsController', function (it, fit, xit) {

	it('should display message if there are no agents', function (t) {
		var vm = t.createController();

		expect(vm.displayNoAgentsMessage()).toEqual(true);
	});

	it('should not display message if there are agents', function (t) {
		t.backend
			.withAgentState({
				PersonId: 'ashley'
			});
		
		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.displayNoAgentsMessage()).toEqual(false);
	});
});
