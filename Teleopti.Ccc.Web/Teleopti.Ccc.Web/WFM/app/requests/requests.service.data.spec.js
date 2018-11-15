'use strict';

describe('requestsDataService tests ', function() {
	var $translate, targetSvc;

	beforeEach(function() {
		module('wfm.requests');
	});

	beforeEach(inject(function(_$translate_, requestsDataService) {
		$translate = _$translate_;
		targetSvc = requestsDataService;
	}));

	it('should get statuses list for absenceAndText', function() {
		var results = targetSvc.getAbsenceAndTextRequestsStatuses();

		expect(results.length).toEqual(5);
		expect(results[0].Name).toEqual($translate.instant('Pending'));
		expect(results[1].Name).toEqual($translate.instant('Denied'));
		expect(results[2].Name).toEqual($translate.instant('Approved'));
		expect(results[3].Name).toEqual($translate.instant('Waitlisted'));
		expect(results[4].Name).toEqual($translate.instant('Cancelled'));
	});

	it('should get statuses list for shiftTrade', function() {
		var results = targetSvc.getShiftTradeRequestsStatuses();

		expect(results.length).toEqual(3);
		expect(results[0].Name).toEqual($translate.instant('Pending'));
		expect(results[1].Name).toEqual($translate.instant('Denied'));
		expect(results[2].Name).toEqual($translate.instant('Approved'));
	});

	it('should get statuses list for overtime', function() {
		var results = targetSvc.getOvertimeRequestsStatuses();

		expect(results.length).toEqual(3);
		expect(results[0].Name).toEqual($translate.instant('Pending'));
		expect(results[1].Name).toEqual($translate.instant('Denied'));
		expect(results[2].Name).toEqual($translate.instant('Approved'));
	});
});
