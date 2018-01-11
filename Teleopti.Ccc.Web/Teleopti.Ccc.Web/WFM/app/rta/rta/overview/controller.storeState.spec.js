'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {

	it('should store selected site', function (t) {
		t.stateParams.siteIds = ['parisId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			});
		t.createController();
		expect(t.sessionStorage.rtaState.siteIds).toEqual(['parisId']);
	});

	it('should store selected site and skill being selected', function (t) {
		t.stateParams.siteIds = ['parisId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withSkill({
				Id: 'skillId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea({Id: 'skillId'})
		});

		expect(t.sessionStorage.rtaState.siteIds).toEqual(['parisId']);
		expect(t.sessionStorage.rtaState.skillIds).toEqual(['skillId']);
	});

});
