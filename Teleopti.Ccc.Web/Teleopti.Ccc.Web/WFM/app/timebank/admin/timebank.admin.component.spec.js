'use strict';

describe('component: admin', function() {
	var $httpBackend,
		$controller,
		$componentController,
		ctrl;
	

	beforeEach(function() {
		module('wfm.timebank');
	});

	beforeEach(inject(function(_$httpBackend_,_$componentController_, _$controller_) {
		$httpBackend = _$httpBackend_;
		$componentController = _$componentController_;
		$controller = _$controller_;

	}));

	afterEach(function() {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should get people', function() {
		ctrl = $componentController('admin', null, {});

		ctrl.getPeople();

		expect(ctrl.people.length).toEqual(1);
	});

	it('should get contracts', function() {
		ctrl = $componentController('admin', null, {});

		ctrl.getContracts();

		expect(ctrl.contracts.length).toEqual(1);
	});

});
