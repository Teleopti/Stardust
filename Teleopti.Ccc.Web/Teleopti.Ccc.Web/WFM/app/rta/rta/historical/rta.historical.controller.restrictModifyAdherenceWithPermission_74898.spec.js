'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should have permission to modify adherence', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.permissionToModifyAdherence(true);

		var c = t.createController();

		expect(c.hasModifyAdherencePermission).toEqual(true);
	});	
	
	it('should not have permission to modify adherence', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.permissionToModifyAdherence(false);
		
		var c = t.createController();

		expect(c.hasModifyAdherencePermission).toEqual(false);
	});
	
	it('should check permission with person id', function (t) {
		var personId = t.randomId();
		t.stateParams.personId = personId;
		
		t.createController();

		expect(t.backend.lastParams.permissionToModifyAdherence().personId).toEqual(personId);
	});

	it('should check permission with date', function (t) {
		var date = '2018051' + Math.round(Math.random());
		t.stateParams.date = date;
		
		t.createController();

		expect(t.backend.lastParams.permissionToModifyAdherence().date).toEqual(date);
	});
});