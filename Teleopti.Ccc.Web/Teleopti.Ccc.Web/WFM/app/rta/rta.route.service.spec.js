'use strict';
describe('rtaRouteService', function () {
	var target, $state, curDate;

	beforeEach(module('wfm.rta'));

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

		expect($state.go).toHaveBeenCalledWith('refact-rta');
	});

	it('should get change schedule url for an agent', function () {
		expect(target.urlForChangingSchedule("11610fe4-0130-4568-97de-9b5e015b2564"))
			.toEqual("#/teams/?personId=11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get sites overview url', function () {
		expect(target.urlForSites()).toEqual('#/rta');
	});

	it('should get sites by skill overview url', function () {
		expect(target.urlForSites('f08d75b3-fdb4-484a-ae4c-9f0800e2f753', undefined)).toEqual('#/rta/?skillIds=f08d75b3-fdb4-484a-ae4c-9f0800e2f753');
	});

	it('should get sites by skill area overview url', function () {
		expect(target.urlForSites(undefined, 'f08d75b3-fdb4-484a-ae4c-9f0800e2f753')).toEqual('#/rta/?skillAreaId=f08d75b3-fdb4-484a-ae4c-9f0800e2f753');
	});
});
