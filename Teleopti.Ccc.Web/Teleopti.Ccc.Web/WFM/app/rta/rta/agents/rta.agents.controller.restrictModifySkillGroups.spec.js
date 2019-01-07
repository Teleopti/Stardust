'use strict';

rtaTester.describe('RtaAgentsController', function (it, fit, xit) {
	it('should have permission to access modify skill group', function (t) {
		t.backend.with.permissions({
			ModifySkillGroup: true
		});

		var vm = t.createController();

		expect(vm.hasModifySkillGroupPermission).toEqual(true);
	});
	
	it('should not have permission to access modify skill group', function (t) {
		t.backend.with.permissions({
			ModifySkillGroup: false
		});

		var vm = t.createController();

		expect(vm.hasModifySkillGroupPermission).toEqual(false);
	});

});