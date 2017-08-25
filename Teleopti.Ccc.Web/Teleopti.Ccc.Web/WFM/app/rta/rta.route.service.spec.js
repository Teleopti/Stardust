'use strict';
describe('rtaRouteService', function () {
	var target, $state, curDate;

	beforeEach(module('wfm.rta'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(inject(function (_$state_, rtaRouteService) {
		target = rtaRouteService;
		$state = _$state_;
		curDate = new Date();
	}));

	afterEach(function () {
		jasmine.clock().mockDate(curDate);
	});

	it('should go back to overview', function () {
		spyOn($state, 'go');

		target.goToOverview();

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should get change schedule url for an agent', function () {
		expect(target.urlForChangingSchedule("11610fe4-0130-4568-97de-9b5e015b2564"))
			.toEqual("#/teams/?personId=11610fe4-0130-4568-97de-9b5e015b2564");
	});

});
