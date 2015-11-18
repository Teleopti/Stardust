'use strict';
describe('RtaGridService', function() {
	var target, $state;

	beforeEach(module('wfm.rta'));
	beforeEach(inject(function(_$state_, RtaGridService) {
		target = RtaGridService;
		$state = _$state_;
	}));

	it('should not display adherence link when there is no adherence change', function() {
		expect(target.showAdherence(null)).toEqual(false);
	});

	it('should display adherence link when there is adherence change', function() {
		expect(target.showAdherence(0)).toEqual(true);
	});

	it('should not display last updated timestamp when there is no updates', function() {
		expect(target.showLastUpdate('')).toEqual(false);
		expect(target.showLastUpdate(null)).toEqual(false);
	});

	it('should display last updated timestamp', function() {
		expect(target.showLastUpdate('2015-11-16 08:30')).toEqual(true);
	});
});
