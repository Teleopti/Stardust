'use strict';
describe('timebankAdminService', function() {
	var $httpBackend,
		timebankAdminService;

	beforeEach(function() {
		module('wfm.timebank');
	});

	beforeEach(inject(function(_$httpBackend_, _timebankAdminService_) {
		$httpBackend = _$httpBackend_;
		timebankAdminService = _timebankAdminService_;

	}));

	it('should get people', function() {
    var result = timebankAdminService.getPeople();

    expect(result.length).toEqual(1);
	});

	it('should get contracts', function() {
		var result = timebankAdminService.getContracts();

		expect(result.length).toEqual(1);
	})


});
