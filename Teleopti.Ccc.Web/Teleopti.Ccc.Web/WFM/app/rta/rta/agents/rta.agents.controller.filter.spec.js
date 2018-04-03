'use strict';

rtaTester.describe('RtaAgentsController', function (it, fit, xit, _,
													 $fakeBackend,
													 $controllerBuilder) {
	var vm;

	it('should have preselected skill in field for sites', function (t) {
		t.stateParams.skillIds = "phoneGuid";
		t.backend
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 3,
				SkillId: "phoneGuid"
			})
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillNew.Id).toEqual('phoneGuid');
		expect(vm.selectedSkillNew.Name).toEqual('Phone');
	});

	it('should have preselected skill area in field for sites', function (t) {
		t.stateParams.skillAreaId = "emailAndPhoneGuid";
		t.backend
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 3,
				SkillId: "phoneGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillArea.Id).toEqual('emailAndPhoneGuid');
		expect(vm.selectedSkillArea.Name).toEqual('Email and phone');
	});

	it('should have preselected skill in field for teams', function (t) {
		t.stateParams.skillIds = "phoneGuid";
		t.stateParams.siteIds = "parisGuid";
		t.backend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillNew.Id).toEqual('phoneGuid');
		expect(vm.selectedSkillNew.Name).toEqual('Phone');
	});

	it('should have preselected skill area in field for teams', function (t) {
		t.stateParams.skillAreaId = "emailAndPhoneGuid";
		t.stateParams.siteIds = ["parisGuid"];
		t.backend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillArea.Id).toEqual('emailAndPhoneGuid');
		expect(vm.selectedSkillArea.Name).toEqual('Email and phone');
	});
});
