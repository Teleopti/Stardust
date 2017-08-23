'use strict';
describe('rtaStateStorageService', function () {
    var target;

	beforeEach(module('wfm.rta'));

	beforeEach(inject(function (rtaStateStorageService) {
		target = rtaStateStorageService;
	}));

	it('should get state', function () {
		target.setState('refact-rta', {skillIds: ['ChannelSales']});

		expect(target.getState()).toEqual({'state': 'refact-rta', 'params': {skillIds: ['ChannelSales']}});
    });
    
    it('should remove stored state', function () {
        target.setState('refact-rta', {skillIds: ['ChannelSales']});
        target.removeState();

		expect(target.getState()).toEqual({'state': null, 'params': null});
	});
});
