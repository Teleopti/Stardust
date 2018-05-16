﻿'use strict';

rtaTester.fdescribe('RtaHistoricalController', function (it, fit, xit) {
	it('should have permission to modify adherence', function (t) {
		t.backend.with.modifyAdherencePermission(true);

		var c = t.createController();

		expect(c.hasModifyAdherencePermission).toEqual(true);
	});	
	
	it('should not have permission to modify adherence', function (t) {
		t.backend.with.modifyAdherencePermission(false);
		
		var c = t.createController();

		expect(c.hasModifyAdherencePermission).toEqual(false);
	});
	
	it('should check permission with person id', function (t) {
		var personId = t.randomId();
		t.stateParams.personId = personId;
		
		t.createController();

		expect(t.backend.lastParams.modifyAdherencePermission().personId).toEqual(personId);
	});

	it('should check permission with date', function (t) {
		var date = '2018051' + Math.round(Math.random());
		t.stateParams.date = date;
		
		t.createController();

		expect(t.backend.lastParams.modifyAdherencePermission().date).toEqual(date);
	});
});